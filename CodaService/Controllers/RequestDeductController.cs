using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CodaService.Models;
using CodaService.bssSrv;
using CodaService.ocsSrv;
using CodaService.OcsBeeline;
using CodaService.smsapi;

namespace CodaService.Controllers
{
    [Authorize]
    public class RequestDeductController : ApiController
    {
        public ResultRequestDeduct Post(RequestDeducts request)
        {
            #region Initial values
            ResultRequestDeduct result = new ResultRequestDeduct();
            CodaPayDBEntities db = new CodaPayDBEntities();
            BSSGSMSrv bss = new BSSGSMSrv();
            ocsService ocs = new ocsService();
            string BSSUser = System.Configuration.ConfigurationManager.AppSettings["BSSUser"];
            string BSSPass = System.Configuration.ConfigurationManager.AppSettings["BSSPassword"];
            string BSSIP = System.Configuration.ConfigurationManager.AppSettings["BSSIp"];
            string OCSUser = System.Configuration.ConfigurationManager.AppSettings["OCSUser"];
            string OCSPass = System.Configuration.ConfigurationManager.AppSettings["OCSPass"];
            string BeelineUser = System.Configuration.ConfigurationManager.AppSettings["BeelineUser"];
            string BeelinePass = System.Configuration.ConfigurationManager.AppSettings["BeelinePass"];
            string Remote_IP = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

            //#if DEBUG
            //    string _email = "testmba@gmail.com";
            //#else
            //    string _email = User.Identity.Name;
            //#endif

            string _email = User.Identity.Name;
            var RefCode = Guid.NewGuid().ToString("N");
            result.RefCode = RefCode;
            result.TransactionID = request.TransactionID;
            string OTP = string.Empty;
            bool OTPStatus = false;
            bool Status = false;
            string ServiceType = "Register";
            string OperatorName = "None";
            #endregion

            try
            {
                //Check Transaction
                var _trans = MainClass.CheckTransaction(request.TransactionID);
                if (_trans)
                {
                    result.ResultCode = "003";
                    result.ResultDesc = "Duplicate transaction";
                    goto SafeExit;
                }
                //Check Amount
                if (request.Amount < 0 && request.Amount > 500000)
                {
                    result.ResultCode = "106";
                    result.ResultDesc = "Invalid amount";
                    goto SafeExit;
                }
                //Check Phone number
                if (request.RequestType == "Production" || request.RequestType == "UAT")
                {
                    CheckMsisdn checkMsisdn = new CheckMsisdn();
                    var _checkphone = checkMsisdn.CheckPhone(request.Msisdn);
                    OperatorName = _checkphone.Type;
                    if (_checkphone.ResultCode == "200")
                    {
                        //Get Header                      
                        var _getHeader = db.tbl_header.Where(x => x.Username == _email).FirstOrDefault();
                        string _headername = "LaoTelecom";
                        string _company = "Lao Telecom";
                        if (_getHeader != null)
                        {
                            _headername = _getHeader.HeaderName;
                            _company = _getHeader.Company;
                        }
                      
                        //Check member
                        var _checkRegister = db.tbl_member.Where(x => x.Username == _email && x.Status == true && x.Msisdn == request.Msisdn).Count();
                        if (_checkRegister > 0)
                        {
                            if (request.IsRecurring == false)
                            {
                                result.ResultCode = "107";
                                result.ResultDesc = "This msisdn already register in our game service";
                                goto SafeExit;
                            }
                            else
                            {
                                MainClass.ReqDeduct deduct = new MainClass.ReqDeduct();
                                deduct.Email = _email.Trim();
                                deduct.IPAddress = Remote_IP.Trim();
                                deduct.OCSUser = OCSUser.Trim();
                                deduct.OCSPass = OCSPass.Trim();
                                deduct.Operator = _checkphone.Type.Trim();
                                deduct.Header = _headername.Trim();
                                deduct.Company = _company.Trim();
                                deduct.RefCode = RefCode.Trim();

                                ServiceType = "Recurring";
                                //Deduct balance
                                var _deduct = MainClass.DuductBalancePrivate(request, deduct);
                                result.ResultCode = _deduct.Result.ResultCode;
                                result.ResultDesc = _deduct.Result.ResultDesc;
                                Status = _deduct.Result.Status;
                                goto SafeExit;
                            }
                        }
                        else
                        {
                            //LTC
                            if (_checkphone.Type == "LTC")
                            {
                                if ((bool)_getHeader.IsLTC)
                                {
                                    #region LTC Number
                                    //Check Network Type
                                    var network = bss.QueryNetworkType(_checkphone.Msisdn, BSSUser, BSSPass, BSSIP, BSSUser);
                                    if (network.NETWORK_CODE == "M" || network.NETWORK_CODE == "H" || network.NETWORK_CODE == "W")
                                    {
                                        //Check Balance
                                        try
                                        {
                                            var requestocs = ocs.QueryBalance(request.Msisdn, "C_MAIN_ACCOUNT", OCSUser, OCSPass);
                                            if (request.Amount <= requestocs)
                                            {
                                                //Submit OTP
                                                smservice sms = new smservice();
                                                OTP = MainClass.GetRandom();
                                                string Message = $"ລະຫັດ OTP ຂອງທ່ານສຳຫລັບບໍລິການ {_company} ແມ່ນ: {OTP}";
                                                var servicesms = sms.SubmitSMS(request.Msisdn, Message, _headername);
                                                if (servicesms)
                                                {
                                                    OTPStatus = true;
                                                    result.ResultCode = "200";
                                                    result.ResultDesc = "Operation Succeeded.";
                                                }
                                                else
                                                {
                                                    result.ResultCode = "104";
                                                    result.ResultDesc = "Could not send sms";
                                                }
                                            }
                                            else
                                            {
                                                result.ResultCode = "103";
                                                result.ResultDesc = "Balance is not enough";
                                                goto SafeExit;
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            result.ResultCode = "102";
                                            result.ResultDesc = "Could not connect to ocs service (LTC)";
                                            goto SafeExit;
                                        }
                                    }
                                    else
                                    {
                                        result.ResultCode = "101";
                                        result.ResultDesc = "Phone number is not prepaid";
                                        goto SafeExit;
                                    }
                                    #endregion
                                }
                                else
                                {
                                    result.ResultCode = "005";
                                    result.ResultDesc = "Your user can not use this function";
                                    goto SafeExit;
                                }
                            }
                            //TPlus
                            else if (_checkphone.Type == "TPlus")
                            {
                                if ((bool)_getHeader.IsTPlus)
                                {
                                    //Check Balance
                                    try
                                    {
                                        var requestocs = ocs.QueryBalance(request.Msisdn, "C_MAIN_ACCOUNT", OCSUser, OCSPass);
                                        if (request.Amount <= requestocs)
                                        {
                                            //Submit OTP
                                            smservice sms = new smservice();
                                            OTP = MainClass.GetRandom();
                                            string Message = $"ລະຫັດ OTP ຂອງທ່ານສຳຫລັບບໍລິການ {_company} ແມ່ນ: {OTP}";
                                            var servicesms = sms.SubmitSMS(request.Msisdn, Message, _headername);
                                            if (servicesms)
                                            {
                                                OTPStatus = true;
                                                result.ResultCode = "200";
                                                result.ResultDesc = "Operation Succeeded";
                                            }
                                            else
                                            {
                                                result.ResultCode = "105";
                                                result.ResultDesc = "Could not send sms";
                                            }
                                        }
                                        else
                                        {
                                            result.ResultCode = "103";
                                            result.ResultDesc = "Balance is not enough";
                                            goto SafeExit;
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        result.ResultCode = "102";
                                        result.ResultDesc = "Could not connect to ocs service (TPlus)";
                                        goto SafeExit;
                                    }
                                }
                            }
                            else
                            {
                                result.ResultCode = _checkphone.ResultCode;
                                result.ResultDesc = _checkphone.ResultDescc;
                                goto SafeExit;
                            }
                        }
                    }
                }
                else
                {
                    result.ResultCode = "004";
                    result.ResultDesc = "Invalid request type";
                    goto SafeExit;
                }
            }
            catch (Exception ex)
            {
                result.ResultCode = "002";
                result.ResultDesc = "ERR: " + ex.Message;
            }
        SafeExit:
            result.ResultType = ServiceType;
            try
            {
                //Save log
                tbl_request_log_production log = new tbl_request_log_production
                {
                    Msisdn = request.Msisdn.Trim(),
                    OTP = OTP.Trim(),
                    OTPStatus = OTPStatus,
                    RefCode = RefCode.Trim(),
                    PaidAmount = Convert.ToInt32(request.Amount),
                    RequestType = request.RequestType.Trim(),
                    ResultCode = result.ResultCode.Trim(),
                    ResultDesc = result.ResultDesc.Trim(),
                    ResultDate = DateTime.Now,
                    TransactionID = result.TransactionID.Trim(),
                    UserName = _email.Trim(),
                    Operator = OperatorName.Trim(),
                    Status = Status,
                    ServiceType = ServiceType.Trim(),
                };
                db.tbl_request_log_production.Add(log);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                result.ResultCode = "006";
                result.ResultDesc = "ERR: " + ex.Message;
            }           
            return result;
        }
    }
    public class ResultRequestDeduct
    {
        public string ResultCode { get; set; } = "001";
        public string ResultDesc { get; set; } = "Please try again";
        public string TransactionID { get; set; } = "None";
        public string RefCode { get; set; } = "None";
        public string ResultType { get; set; } = "None";
    }
    public class RequestDeducts
    {
        [Required(ErrorMessage = "Please input a transaction ID")]
        public string TransactionID { get; set; }

        [Required(ErrorMessage = "Please input a msisdn")]
        public string Msisdn { get; set; }

        [Required(ErrorMessage = "Please input amount")]
        public double Amount { get; set; }

        [Required(ErrorMessage = "Please input Request type")]
        public string RequestType { get; set; }

        [Required(ErrorMessage = "Please input game name")]
        public string GameName { get; set; }

        [Required(ErrorMessage = "Please input card name")]
        public string CardName { get; set; }

        [Required(ErrorMessage = "Please choose Recurring or not")]
        public bool IsRecurring { get; set; }
    }
}
