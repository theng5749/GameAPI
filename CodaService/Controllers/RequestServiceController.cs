using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using CodaService.Models;
using CodaService.bssSrv;
using CodaService.ocsSrv;
using CodaService.OcsBeeline;
using System.Web.Http;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;

namespace CodaService.Controllers
{
   [Authorize(Roles = "Admin, Super Admin")]
    public class RequestServiceController : ApiController
    {
        public ResultService Post(RequestSrv Value)
        {
            #region Initial Value
            ResultService result = new ResultService();
            CodaPayDBEntities db = new CodaPayDBEntities();
            string BSSUser = System.Configuration.ConfigurationManager.AppSettings["BSSUser"];
            string BSSPass = System.Configuration.ConfigurationManager.AppSettings["BSSPassword"];
            string BSSIP = System.Configuration.ConfigurationManager.AppSettings["BSSIp"];
            string OCSUser = System.Configuration.ConfigurationManager.AppSettings["OCSUser"];
            string OCSPass = System.Configuration.ConfigurationManager.AppSettings["OCSPass"];
            string BeelineUser = System.Configuration.ConfigurationManager.AppSettings["BeelineUser"];
            string BeelinePass = System.Configuration.ConfigurationManager.AppSettings["BeelinePass"];
            BSSGSMSrv bss = new BSSGSMSrv();
            ocsService ocs = new ocsService();
            #endregion

            if (Value.Msisdn == "" || Value.GameName =="" || Value.CardType == "")
            {
                Value.Msisdn = "Empty";
                result.ResultCode = "104";
                result.ResultDesc = "Invalid input parameters.";
                result.CardType = "Empty";
                result.GameName = "Empty";
                goto SafeExit;
            }
            else
            {
                result.Msisdn = Value.Msisdn;
                result.Balance = -1;
                result.PaidAmount = Value.PaidAmount;
                result.ResultCode = "100";
                result.ResultDesc = "System error please try again.";
                result.CardType = Value.CardType;
                result.GameName = Value.GameName;
                //Check Phone number
                CheckMsisdn checkMsisdn = new CheckMsisdn();
                var _checkphone = checkMsisdn.CheckPhone(Value.Msisdn);
                if (_checkphone.ResultCode == "200")
                {
                    //LTC
                    if (_checkphone.Type=="LTC")
                    {
                        #region LTC Number
                        //Check Network Type
                        var network = bss.QueryNetworkType(_checkphone.Msisdn, BSSUser, BSSPass, BSSIP, BSSUser);
                        if (network.NETWORK_CODE == "M" || network.NETWORK_CODE == "H" || network.NETWORK_CODE == "W")
                        {
                            //Check Balance
                            try
                            {
                                var requestocs = ocs.QueryBalance(Value.Msisdn, "C_MAIN_ACCOUNT", OCSUser, OCSPass);
                                result.Balance = Convert.ToDouble(requestocs);
                                if (Value.PaidAmount <= result.Balance)
                                {
                                    result.ResultCode = "200";
                                    result.ResultDesc = "Operation succeeded.";
                                }
                                else
                                {
                                    result.ResultCode = "103";
                                    result.ResultDesc = "Balance is not enough.";
                                    goto SafeExit;
                                }
                            }
                            catch (Exception)
                            {
                                result.ResultCode = "102";
                                result.ResultDesc = "Could not connect to ocs service (LTC).";
                                goto SafeExit;
                            }
                        }
                        else
                        {
                            result.ResultCode = "101";
                            result.ResultDesc = "Phone number is not prepaid.";
                            goto SafeExit;
                        }
                        #endregion
                    }
                    else
                    {
                        //result.ResultCode = "115";
                        //result.ResultDesc = "Beeline number is under maintained";
                        #region Beeline Number

                        //Check Backlist
                        //Int32 _backlist = db.tbl_backlist.Where(x => x.Status == true && x.Msisdn == Value.Msisdn).Count();
                        //if (_backlist > 0)
                        //{
                        //    result.ResultCode = "115";
                        //    result.ResultDesc = "This phone number is in the blacklist.";
                        //    goto SafeExit;
                        //} 

                        //check operator with email
                        string _email = User.Identity.Name;
                        if (_email == "product@codapayments.com" || _email == "anoloth@gmail.com" || _email== "ryan.hong@linkit360.com")
                        {
                            var requestocs = ocs.QueryBalance(Value.Msisdn, "C_MAIN_ACCOUNT", OCSUser, OCSPass);
                            result.Balance = Convert.ToDouble(requestocs);
                            if (Value.PaidAmount <= result.Balance)
                            {
                                result.ResultCode = "200";
                                result.ResultDesc = "Operation succeeded.";
                            }
                            else
                            {
                                result.ResultCode = "103";
                                result.ResultDesc = "Balance is not enough.";
                                goto SafeExit;
                            }
                        }
                        else
                        {
                            result.ResultCode = "108";
                            result.ResultDesc = "This game is not allow to TPlus number.";
                            goto SafeExit;
                        }
                        #endregion
                    }
                }
                else
                {
                    result.ResultCode = "114";
                    result.ResultDesc = "Invalid phone number.";
                }             
            }
            SafeExit:
            //Insert log
            try
            {
                tbl_request_log request_Log = new tbl_request_log
                {
                    msisdn = Value.Msisdn,
                    paidAmount = Convert.ToInt32(Value.PaidAmount),
                    resultDate = DateTime.Now,
                    resultCode = result.ResultCode,
                    resultDesc = result.ResultDesc,
                    cardType = result.CardType,
                    gameName = result.GameName,                            
                };
                db.tbl_request_log.Add(request_Log);
                db.SaveChanges();
            }
            catch (Exception ex) 
            {
                string err = ex.Message;
            }
            return result;
        }
    }
    public class RequestSrv
    {
        public string Msisdn { get; set; }
        public double PaidAmount { get; set; }
        public string GameName { get; set; }
        public string CardType { get; set; }
    }
    public class ResultService
    {
        public string Msisdn { get; set; }
        public double Balance { get; set; }
        public string GameName { get; set; }
        public string CardType { get; set; }
        public double PaidAmount { get; set; }
        public string ResultCode { get; set; }
        public string ResultDesc { get; set; }
    }
}
