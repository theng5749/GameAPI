using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CodaService.bssSrv;
using CodaService.ocsSrv;
using CodaService.Models;
using CodaService.OcsBeeline;
using System.Text;
using System.IO;
using CodaService.smsapi;

namespace CodaService.Controllers
{
    [Authorize(Roles = "Admin, Super Admin")]
    public class DeductBalanceController : ApiController
    {
        public ResultBalance Post(RequestBalance Value)
        {

            #region Initial Values
            CodaPayDBEntities db = new CodaPayDBEntities();
            ResultBalance result = new ResultBalance();
            result.Msisdn = Value.Msisdn;
            result.TransactinoID = Value.TransactinoID;
#if DEBUG
            string _email = "boualytpv@gmail.com";
#else
                            string _email = User.Identity.Name;
#endif
            string Remote_IP = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            CheckDuplicate checkDuplicate = new CheckDuplicate();
            string BSSUser = System.Configuration.ConfigurationManager.AppSettings["BSSUser"];
            string BSSPass = System.Configuration.ConfigurationManager.AppSettings["BSSPassword"];
            string BSSIP = System.Configuration.ConfigurationManager.AppSettings["BSSIp"];
            string OCSUser = System.Configuration.ConfigurationManager.AppSettings["OCSUser"];
            string OCSPass = System.Configuration.ConfigurationManager.AppSettings["OCSPass"];
            string BeelineUser = System.Configuration.ConfigurationManager.AppSettings["BeelineUser"];
            string BeelinePass = System.Configuration.ConfigurationManager.AppSettings["BeelinePass"];
            BSSGSMSrv bss = new BSSGSMSrv();
            ocsService ocs = new ocsService();
            Services OCSBeeline = new Services();
            string ocsCode = "Empty";
            string ocsDesc = "Empty";
            string _operator = "None";
            int balanceNew = -1;
            int balanceOld = -1;
            string message = "Your transactoin is not success.";
            string sms_status = "ERR";
            result.ResultCode = "100";
            result.ResultDesc = "System error please try again.";
            result.TransactinoID = Value.TransactinoID;
            result.Username = _email;
            #endregion

            //Check null
            if (Value.Gamename == null || Value.CardType == null || Value.Msisdn == null || Value.TransactinoID == null)
            {
                result.ResultCode = "112";
                result.ResultDesc = "Parameters could not be null.";
                goto safeExit;
            }
            //Check empty
            if (Value.Gamename == "" || Value.CardType == "" || Value.Msisdn == "" || Value.TransactinoID == "")
            {
                result.ResultCode = "113";
                result.ResultDesc = "Parameters could not be empty.";
                goto safeExit;
            }
            //Check Amount 
            if (Value.PaidAmount < 1000 || Value.PaidAmount > 2000000)
            {
                result.ResultCode = "108";
                result.ResultDesc = "Amount should be 1000 - 2000000 LAK";
                goto safeExit;
            }
            //Check Transaction
            if (checkDuplicate.CheckDup(Value.TransactinoID) )
            {
                result.ResultCode = "107";
                result.ResultDesc = "Douplicate transaction ID.";
                goto safeExit;
            }
            else if (Value.TransactinoID.Length > 20)
            {
                result.ResultCode = "111";
                result.ResultDesc = "Invalid transaction ID.";
                goto safeExit;
            }
            else
            {
                // Check Phone number
                CheckMsisdn checkMsisdn = new CheckMsisdn();
                var _checkPhone = checkMsisdn.CheckPhone(Value.Msisdn);
                if (_checkPhone.ResultCode == "200")
                {
                    if (_checkPhone.Type == "LTC")
                    {
                        _operator = "LTC";
                        //=> LTC
                        #region LTC Number
                        //Check Phone number and network type
                        var network = bss.QueryNetworkType(_checkPhone.Msisdn, BSSUser, BSSPass, BSSIP, BSSUser);
                        if (network.NETWORK_CODE == "M" || network.NETWORK_CODE == "H" || network.NETWORK_CODE == "W")
                        {
                            //Check Balance
                            try
                            {
                                var requestocs = ocs.QueryBalance(Value.Msisdn, "C_MAIN_ACCOUNT", OCSUser, OCSPass);
                                double _balance = Convert.ToDouble(requestocs);
                                if (Value.PaidAmount <= _balance)
                                {
                                    //deduct balance
                                    ResultHeader resultHeader = new ResultHeader();
                                    ResultAdjust resultAdjust = new ResultAdjust();
                                    var requestocsservice = ocs.AdjustmentBalance(Value.Msisdn, "C_MAIN_ACCOUNT", -Convert.ToInt64(Value.PaidAmount), OCSUser, OCSPass);
                                    resultHeader = requestocsservice.ResultHeader;
                                    resultAdjust = requestocsservice.Respon;
                                    bool _status = false;
                                    _status = requestocsservice.Status;
                                    ocsCode = resultHeader.ResultCode;
                                    ocsDesc = resultHeader.ResultDesc;
                                    balanceNew = Convert.ToInt32(resultAdjust.NewBalance);
                                    balanceOld = Convert.ToInt32(resultAdjust.OldBalance);
                                    var _getHeader = db.tbl_header.Where(x => x.Username == _email).FirstOrDefault();
                                    if (_status)
                                    {
                                        message = "ຕັດເງິນໃນເບີໂທຂອງທ່ານຈາກບໍລິການ "+ _getHeader.Company + " ຈຳນວນເງິນ "+ Value.PaidAmount +"";
                                        result.ResultCode = "200";
                                        result.ResultDesc = "Operation Succeeded.";
                                    }
                                    else
                                    {
                                        message = "Could not deduct your balance.";
                                        result.ResultCode = "114";
                                        result.ResultDesc = "Could not deduct balance.";
                                    }
                                   ///// Get Header name from db
                                    string _headername = "LaoTelecom";
                                    if (_getHeader != null)
                                    {
                                        _headername = _getHeader.HeaderName;
                                    }
                                    else
                                    {
                                        _headername = "LaoTelecom";
                                    }
                                    smservice sms = new smservice();
                                    var servicesms = sms.SubmitSMS(_checkPhone.Msisdn, message, _headername);
                                    if (servicesms)
                                    {
                                        sms_status = "Success";
                                    }
                                    else
                                    {
                                        sms_status = "Not Success";
                                    }
                                }
                                else
                                {
                                    balanceNew = Convert.ToInt32(_balance);
                                    balanceOld = balanceNew;
                                    result.ResultCode = "103";
                                    result.ResultDesc = "Balance is not enough.";
                                }
                            }
                            catch (Exception)
                            {
                                result.ResultCode = "102";
                                result.ResultDesc = "Could not connect to ocs service.";
                            }
                        }
                        else
                        {
                            result.ResultCode = "101";
                            result.ResultDesc = " Phone number is not prepaid.";
                        }
                        #endregion
                    }
                    else
                    {
                        //=> Beeline
                        #region Beeline Number
                        _operator = "TPlus";
                        //Check Backlist
                        //Int32 _backlist = db.tbl_backlist.Where(x => x.Status == true && x.Msisdn == Value.Msisdn).Count();
                        //if (_backlist > 0)
                        //{
                        //    result.ResultCode = "115";
                        //    result.ResultDesc = "This phone number is in the blacklist.";
                        //    goto safeExit;
                        //}

                        if (_email == "product@codapayments.com" || _email == "anoloth@gmail.com" || _email== "ryan.hong@linkit360.com")
                        {
                            var requestocs = ocs.QueryBalance(Value.Msisdn, "C_MAIN_ACCOUNT", OCSUser, OCSPass);
                            double _balance = Convert.ToDouble(requestocs);
                            if (Value.PaidAmount <= _balance)
                            {
                                //deduct balance
                                ResultHeader resultHeader = new ResultHeader();
                                ResultAdjust resultAdjust = new ResultAdjust();
                                var requestocsservice = ocs.AdjustmentBalance(Value.Msisdn, "C_MAIN_ACCOUNT", -Convert.ToInt64(Value.PaidAmount), OCSUser, OCSPass);
                                resultHeader = requestocsservice.ResultHeader;
                                resultAdjust = requestocsservice.Respon;
                                bool _status = false;
                                _status = requestocsservice.Status;
                                ocsCode = resultHeader.ResultCode;
                                ocsDesc = resultHeader.ResultDesc;
                                balanceNew = Convert.ToInt32(resultAdjust.NewBalance);
                                balanceOld = Convert.ToInt32(resultAdjust.OldBalance);
                                var _getHeader = db.tbl_header.Where(x => x.Username == _email).FirstOrDefault();
                                if (_status)
                                {
                                    message = "ຕັດເງິນໃນເບີໂທຂອງທ່ານຈາກບໍລິການ " + _getHeader.Company + " ຈຳນວນເງິນ " + Value.PaidAmount + " ກີບ";
                                    result.ResultCode = "200";
                                    result.ResultDesc = "Operation Succeeded.";
                                }
                                else
                                {
                                    message = "Could not deduct your balance.";
                                    result.ResultCode = "114";
                                    result.ResultDesc = "Could not deduct balance.";
                                }
                                smservice sms = new smservice();
                                var servicesms = sms.SubmitSMS(_checkPhone.Msisdn, message, _getHeader.HeaderName);
                                if (servicesms)
                                {
                                    sms_status = "Success";
                                }
                                else
                                {
                                    sms_status = "Not Success";
                                }
                            }
                            else
                            {
                                balanceNew = Convert.ToInt32(_balance);
                                balanceOld = balanceNew;
                                result.ResultCode = "103";
                                result.ResultDesc = "Balance is not enough.";
                            }
                        }
                        else
                        {
                            result.ResultCode = "108";
                            result.ResultDesc = "This game is not allow to TPlus number.";
                        }
                      
                        #region Old Beeline
                        //CheckBalanceBeeline balanceBeeline = new CheckBalanceBeeline();
                        //var _beeline = balanceBeeline.Post(_checkPhone.Msisdn);
                        //double Mainbalance = Convert.ToDouble(_beeline.Balance);
                        //if (Value.PaidAmount <= Mainbalance)
                        //{
                        //    //deduct balancec beeline
                        //    DateTime dt = DateTime.Now;
                        //    string tran = dt.ToString("yyyyMMddHHmmss");
                        //    adjustBalanceRequest adjustBalanceRequest = new adjustBalanceRequest();
                        //    headerRequest headerRequest = new headerRequest();
                        //    headerResult headerResult = new headerResult();
                        //    headerRequest.chanel = "Codapay";
                        //    headerRequest.trans_id = Value.Msisdn + "Coda-Deduct" + tran;
                        //    headerRequest.userid = BeelineUser.ToString().Trim();
                        //    headerRequest.password = BeelinePass.ToString().Trim();
                        //    adjustBalanceRequest.amount = Convert.ToInt64(-Value.PaidAmount);
                        //    adjustBalanceRequest.header = headerRequest;
                        //    adjustBalanceRequest.msisdn = _checkPhone.Msisdn;
                        //    adjustBalanceRequest.type = 2;
                        //    adjustBalanceResult adjustBalanceResult = new adjustBalanceResult();
                        //    balanceDetail balanceDetail = new balanceDetail();
                        //    adjustBalanceResult = OCSBeeline.adjustBalance(adjustBalanceRequest);
                        //    balanceDetail = adjustBalanceResult.balanceDetail[0];
                        //    headerResult = adjustBalanceResult.header;
                        //    if (adjustBalanceResult.header.resultCode == "20")
                        //    {
                        //        message = "Your transaction successfully. Thank you for using our service.";
                        //        result.ResultCode = "200";
                        //        result.ResultDesc = "Operation Succeeded.";
                        //        balanceNew = Convert.ToInt32(balanceDetail.balance);
                        //        balanceOld = Convert.ToInt32(balanceDetail.balance + Value.PaidAmount);
                        //    }
                        //    else
                        //    {
                        //        balanceNew = Convert.ToInt32(Mainbalance);
                        //        balanceOld = balanceNew;
                        //        result.ResultCode = "103";
                        //        result.ResultDesc = "Balance is not enough.";
                        //    }
                        //    ocsCode = headerResult.resultCode;
                        //    ocsDesc = headerResult.resultDesc;
                        //    ServiceSMS sms = new ServiceSMS();
                        //    var servicesms = sms.SendSMS("856" + _checkPhone.Msisdn, message, "CODAPAY");
                        //    string _statussms = Convert.ToString(servicesms.ToString());
                        //    if (_statussms != "True")
                        //    {
                        //        sms_status = "Not Success";
                        //    }
                        //    else
                        //    {
                        //        sms_status = "Success";
                        //    }
                        //}
                        //else
                        //{
                        //    balanceNew = Convert.ToInt32(Mainbalance);
                        //    balanceOld = balanceNew;
                        //    result.ResultCode = "103";
                        //    result.ResultDesc = "Balance is not enough.";
                        //}
                        #endregion
                        #endregion
                        //result.ResultCode = "115";
                        //result.ResultDesc = "Beeline number is under maintained";
                    }
                }
                else
                {
                    result.ResultCode = "114";
                    result.ResultDesc = "Invalid phone number.";
                }
                _operator = _checkPhone.Type;
            }

            safeExit:
            // Insert Log
            try
            {
                tbl_deduct_log tbl_Deduct = new tbl_deduct_log
                {
                    amount = Convert.ToInt32(Value.PaidAmount),
                    cardType = Value.CardType,
                    deductDate = DateTime.Now,
                    gameName = Value.Gamename,
                    msisdn = Value.Msisdn,
                    resultCode = result.ResultCode,
                    resultDesc = result.ResultDesc,
                    transactionID = Value.TransactinoID,
                    username = _email,
                    ocsCode = ocsCode,
                    newBalance = balanceNew,
                    oldBalance= balanceOld,
                    ocsDesc = ocsDesc,
                    smsStatus = sms_status,
                    email = Value.CustomerEmail,
                    @operator = _operator,
                    ip  = Remote_IP.ToString().Trim()
                };
                db.tbl_deduct_log.Add(tbl_Deduct);
                db.SaveChanges();
            }
            catch (Exception)
            {
                // insert log here
            }
            return result;
        }
        //public static DetailsLog  SaveLog()
        //{
        //    DetailsLog result = new DetailsLog();
        //    StringBuilder sb = new StringBuilder();
        //    sb.Append(result.Amount);
        //    sb.Append(result.Amount);
        //    sb.Append(result.Amount);
        //    sb.Append(result.Amount);
        //    sb.Append(result.Amount);
        //    sb.Append(result.Amount);
        //    sb.Append(result.Amount);
        //    sb.Append(result.Amount);

        //   // flush every 20 seconds as you do it
        //   // File.AppendAllText(filePath +  "log.txt", sb.ToString().Split());
        //    sb.Clear();
        //    return result;
        //}
    }
    public class ResultBalance
    {
        public string Msisdn { get; set; }
        public string TransactinoID { get; set; }
        public string Username { get; set; }
        public string ResultCode { get; set; }
        public string ResultDesc { get; set; }
    }
    public class RequestBalance
    {
        public string Msisdn { get; set; }
        public string TransactinoID { get; set; }
        public double PaidAmount { get; set; }
        public string Gamename { get; set; }
        public string CardType { get; set; }
        public string CustomerEmail { get; set; }
    }
    public class DetailsLog
    {
        public Int32 Amount { get; set; }
        public string CardType { get; set; }
        public string GameName { get; set; }
        public string Msisdn { get; set; }
        public string ResultCode { get; set; }
        public string ResultDesc { get; set; }
        public string TransactionID { get; set; }
        public string Username { get; set; }
        public string OcsCode { get; set; }
        public string OcsDesc { get; set; }
        public int NewBalance { get; set; }
        public int OldBalance { get; set; }
        public string SMSStatus { get; set; }
    }
}
