using CodaService.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CodaService.Controllers
{
    [Authorize(Roles = "Super Admin, Admin")]
    public class ConfirmCashOutUATController : ApiController
    {
        public ResultConfirmCashOut Post(RequestConfirmCashOut Values)
        {
            CodaPayDBEntities db = new CodaPayDBEntities();
            ResultConfirmCashOut result = new ResultConfirmCashOut();
            result.Msisdn = Values.Msisdn;
              string _email = User.Identity.Name;
            //string _email = "product@codapayments.com";
            string _remoteIP = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            //Check Phone
            string _operator = string.Empty;
            CheckMsisdn checkMsisdn = new CheckMsisdn();
            var _checkPhone = checkMsisdn.CheckPhone(Values.Msisdn);
            if (_checkPhone.Type == "LTC")
            {
                _operator = "LTC";
            }
            else
            {
                _operator = "TPlus";
            }
            try
            {
                var _getinfo = db.tbl_header.Where(x => x.Username == _email).FirstOrDefault();
                if (_getinfo != null)
                {
                    RequestConfirmCashOutAPI req = new RequestConfirmCashOutAPI();
                    req.apiToken = Values.AccessToken;
                    req.otp = Values.OTP;
                    req.otpRefCode = Values.OTPRefCode;
                    req.otpRefNo = Values.OTPRefNo;
                    req.requestorID = Convert.ToString(_getinfo.ReqIDUAT);
                    req.transCashOutID = Values.TransactionCashOutID;
                    req.transID = Values.TransactionID;
                    ResultConfirmCashOutAPI cashOut = GetToken.ConfirmCashOut(req);
                    if (cashOut != null)
                    {
                        if (cashOut.responseCode == "0000")
                        {
                            result.ResultCode = "200";
                            result.ResultDesc = "Operation Succeeded.";
                        }
                        else
                        {
                            result.ResultCode = cashOut.responseCode;
                            result.ResultDesc = cashOut.responseMessage;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.ResultCode = "002";
                result.ResultDesc = "Could not confirm cashout " + ex.Message.ToString();
            }
            // Insert Log
            try
            {
                tbl_deduct_test tbl_Deduct = new tbl_deduct_test
                {
                    amount = Convert.ToInt32(Values.Amount),
                    cardType = Values.CardType,
                    deductDate = DateTime.Now,
                    gameName = Values.Gamename,
                    msisdn = Values.Msisdn,
                    resultCode = result.ResultCode,
                    resultDesc = result.ResultDesc,
                    transactionID = Values.TransactionID,
                    username = _email,
                    email = Values.CustomerEmail,
                    @operator = _operator,
                    ip = _remoteIP.ToString().Trim(),
                };
                db.tbl_deduct_test.Add(tbl_Deduct);
                db.SaveChanges();
            }
            catch (Exception)
            {
                // insert log here
            }
            return result;
        }
    }

    public class RequestConfirmCashOut
    {
        [Required]
        public string Msisdn { get; set; }
        [Required]
        public double Amount { get; set; }
        [Required]
        public string AccessToken { get; set; }
        [Required]
        public string TransactionID { get; set; }
        [Required]
        public string TransactionCashOutID { get; set; }
        [Required]
        public string OTPRefNo { get; set; }
        [Required]
        public string OTPRefCode { get; set; }
        [Required]
        public string OTP { get; set; }
        [Required]
        public string Gamename { get; set; } = "None";
        [Required]
        public string CardType { get; set; } = "None";
        [Required]
        public string CustomerEmail { get; set; } = "None";
    }
    public class ResultConfirmCashOut
    {
        public string ResultCode { get; set; } = "01";
        public string ResultDesc { get; set; } = "Please try again later.";
        public string Msisdn { get; set; }
    }
}
