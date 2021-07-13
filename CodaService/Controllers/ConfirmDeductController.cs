using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CodaService.Models;
using CodaService.bssSrv;
using CodaService.ocsSrv;
using CodaService.smsapi;

namespace CodaService.Controllers
{
    [Authorize]
    public class ConfirmDeductController : ApiController
    {
        public ResultConfirmDeduct Post(RequestConfrimDeduct request)
        {

            #region Initial value
            ResultConfirmDeduct result = new ResultConfirmDeduct();
            
            CodaPayDBEntities db = new CodaPayDBEntities();
            BSSGSMSrv bss = new BSSGSMSrv();
            ocsService ocs = new ocsService();
            smservice sms = new smservice();
            ResultHeader resultHeader = new ResultHeader();
            ResultAdjust resultAdjust = new ResultAdjust();
            bool smsStatus = false;
            double OldAmount = 0;
            double NewAmount = 0;
            string _headername = "LaoTelecom";
            string _company = "Lao Telecom";
            string BSSUser = System.Configuration.ConfigurationManager.AppSettings["BSSUser"];
            string BSSPass = System.Configuration.ConfigurationManager.AppSettings["BSSPassword"];
            string BSSIP = System.Configuration.ConfigurationManager.AppSettings["BSSIp"];
            string OCSUser = System.Configuration.ConfigurationManager.AppSettings["OCSUser"];
            string OCSPass = System.Configuration.ConfigurationManager.AppSettings["OCSPass"];
            string BeelineUser = System.Configuration.ConfigurationManager.AppSettings["BeelineUser"];
            string BeelinePass = System.Configuration.ConfigurationManager.AppSettings["BeelinePass"];
            string Remote_IP = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            int OTPTimeout = Convert.ToInt32(System.Web.HttpContext.Current.Request.ServerVariables["OTPTimeout"]);

            //#if DEBUG
            //            string _email = "testmba@gmail.com";
            //#else
            //            string _email = User.Identity.Name;
            //#endif

            string _email = User.Identity.Name;
            result.Email = _email;
            result.RefCode = request.RefCode;
            result.TransactionID = request.TransactionID;
            #endregion

            //Check Transaction, RefCode and OTP
            var GetDetails = db.tbl_request_log_production.Where(x => x.RefCode == request.RefCode).FirstOrDefault();
            
            try
            {
                //Check Parameter
                if (request.TransactionID == "" || 
                    request.Gamename == "" ||
                    request.GameType == ""||
                    request.OTP == ""||
                    request.RefCode == "")
                {
                    result.ResultCode = "003";
                    result.ResultDesc = "Please input parameter";
                    goto SaveExit;
                }
                else
                {
                    
                    if (GetDetails != null)
                    {
                        //Check Transaction
                        if (GetDetails.TransactionID == request.TransactionID)
                        {
                            //Check OTP
                            if (GetDetails.OTP == request.OTP)
                            {
                                DateTime _current = DateTime.Now;
                                //Check Expired
                                if ((_current-GetDetails.ResultDate).Value.Minutes > OTPTimeout)
                                {
                                    result.ResultCode = "008";
                                    result.ResultDesc = "This OTP is expired";
                                    goto SaveExit;
                                }

                                //Check Status
                                if ((bool)GetDetails.Status)
                                {
                                    result.ResultCode = "007";
                                    result.ResultDesc = "This OTP is already used";
                                    goto SaveExit;
                                }

                                //Get Header                      
                                var _getHeader = db.tbl_header.Where(x => x.Username == _email).FirstOrDefault();
                                if (_getHeader != null)
                                {
                                    _headername = _getHeader.HeaderName;
                                    _company = _getHeader.Company;
                                }

                                //Check Channel
                                if (request.GameType == "USSD")
                                {
                                    //Connect to each game
                                    if (_getHeader.USSDCode == "727")
                                    {
                                        if (MainClass.CreateMemberPlayzone(GetDetails.Msisdn.Trim()) != true)
                                        {
                                            result.ResultCode = "900";
                                            result.ResultDesc = "Could not create member in playzone side";
                                            goto SaveExit;
                                        }
                                    }
                                    else
                                    {
                                        result.ResultCode = "101";
                                        result.ResultDesc = "This game does not assign USSD Code";
                                        goto SaveExit;
                                    }
                                }

                                //Deduct Balance
                                var requestocsservice = ocs.AdjustmentBalance(GetDetails.Msisdn, "C_MAIN_ACCOUNT", -Convert.ToInt64(GetDetails.PaidAmount), OCSUser, OCSPass);
                                resultHeader = requestocsservice.ResultHeader;
                                resultAdjust = requestocsservice.Respon;
                                result.ResultCode = resultHeader.ResultCode;
                                result.ResultDesc = resultHeader.ResultDesc;
                                OldAmount = resultAdjust.NewBalance;
                                NewAmount = resultAdjust.OldBalance;
                                if (result.ResultCode == "0")
                                {
                                    result.ResultCode = "200";
                                    result.ResultDesc = "Operation Success";
                                    string Message = $"ຕັດເງິນເບີໂທຂອງທ່ານຈຳນວນ {GetDetails.PaidAmount} ຈາກບໍລິການ {_company}";
                                    //Submit SMS
                                    smsStatus = sms.SubmitSMS(GetDetails.Msisdn.Trim(), Message.Trim(), _headername);
                                    //Insert Member
                                    tbl_member member = new tbl_member
                                    {
                                        GameName = request.Gamename.Trim(),
                                        Msisdn = GetDetails.Msisdn.Trim(),
                                        Operator = GetDetails.Operator.Trim(),
                                        RegisterDate = DateTime.Now,
                                        ResultCode = result.ResultCode.Trim(),
                                        ResultDesc = result.ResultDesc.Trim(),
                                        Status = true,
                                        Username = _email,
                                        Header = _headername,
                                    };
                                    db.tbl_member.Add(member);
                                    db.SaveChanges();

                                    //Update Log
                                    GetDetails.Status = true;
                                    db.Entry(GetDetails).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }
                            else
                            {
                                result.ResultCode = "006";
                                result.ResultDesc = "Invalid request please check your OTP";
                                goto SaveExit;
                            }
                        }
                        else
                        {
                            result.ResultCode = "005";
                            result.ResultDesc = "Invalid request please check your transaction ID";
                            goto SaveExit;
                        }
                    }
                    else
                    {
                        result.ResultCode = "004";
                        result.ResultDesc = "Invalid request please check your ref code";
                        goto SaveExit;
                    }
                }
            }
            catch (Exception ex)
            {
                result.ResultCode = "002";
                result.ResultDesc = "ERR: " + ex.Message;
            }

            SaveExit:

            //Insert Log
            tbl_confirm_deduct log = new tbl_confirm_deduct
            {
                Cardname = request.Gamename.Trim(),
                DeductDate = DateTime.Now,
                DeductType = "Register",
                RequestType = GetDetails.RequestType.Trim(),
                Email = _email.Trim(),
                Gamename = request.Gamename.Trim(),
                IPAddress = Remote_IP.Trim(),
                Msisdn = GetDetails.Msisdn,
                NewBalance = Convert.ToInt32(NewAmount),
                OldBalance = Convert.ToInt32(OldAmount),
                Operator = GetDetails.Operator.Trim(),
                RefCode = request.RefCode.Trim(),
                ResultCode = result.ResultCode.Trim(),
                ResultDesc = result.ResultDesc.Trim(),
                SMSStatus = smsStatus,
                TransactionID = request.TransactionID.Trim(),
                Username = _email.Trim(),
                Amount = GetDetails.PaidAmount
            };
            db.tbl_confirm_deduct.Add(log);
            db.SaveChanges();

            return result;
        }
    }
    public class RequestConfrimDeduct
    {
        public string TransactionID { get; set; }
        public string RefCode { get; set; }
        public string OTP { get; set; }
        public string Gamename { get; set; }
        public string GameType { get; set; }
    }
    public class ResultConfirmDeduct
    {
        public string ResultCode { get; set; } = "001";
        public string ResultDesc { get; set; } = "Please try again later";
        public string Email { get; set; }
        public string TransactionID { get; set; }
        public string RefCode { get; set; }
    }
}
