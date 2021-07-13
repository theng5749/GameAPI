using CodaService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;

namespace CodaService.Controllers
{

    [Authorize(Roles = "Super Admin, Admin")]
    public class CashOutUATController : ApiController
    {
        public ResultCashOutUAT Post(RequestCashOutUAT Values)
        {
            CodaPayDBEntities db = new CodaPayDBEntities();
            ResultCashOutUAT result = new ResultCashOutUAT();
            string _email = User.Identity.Name;
           // string _email = "product@codapayments.com";
            string _remoteIP = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            string Ref = "", Ref2 = "", Ref3 = "", Ref4 = "";
            if (Values.Amount > 0)
            {
                try
                {
                    //Get infor from tbl_header
                    var _getinfo = db.tbl_header.Where(x => x.Username == _email).FirstOrDefault();
                    ResultToken _token = new ResultToken();
                    _token = GetToken.MyToken(_getinfo.userUAT, _getinfo.passUAT);
                    if (_token != null)
                    {
                        #region Payload and Checksum
                        string payload = "REF" + ","
                                        + Values.Msisdn.Trim() + ","
                                        + Values.Amount + ","
                                        + "LAK" + ","
                                        + Values.Remark.Trim() + ","
                                        + Ref.Trim() + ","
                                        + Ref2.Trim() + ","
                                        + Ref3.Trim() + ","
                                        + Ref4.Trim() + ",ltc";
                        SHA256 mySHA256 = SHA256.Create();
                        byte[] hashValue = mySHA256.ComputeHash(Encoding.UTF8.GetBytes(payload));
                        var _checksum = Convert.ToBase64String(hashValue);
                        #endregion
                        if (_token != null && _checksum != null)
                        {
                            RequestValues request = new RequestValues();
                            request.Token = _token.accessToken.Trim();
                            request.TransactionID = Values.TransactinoID;
                            request.CheckSum = _checksum.Trim();
                            ResultCashOut cashOut = GetToken.RequestCashOut(Values, request, _email, Convert.ToInt32(_getinfo.ReqIDUAT));
                            if (cashOut != null)
                            {
                                if (cashOut.responseCode == "0000")
                                {
                                    result.APIToken = _token.accessToken;
                                    result.ResultCode = "200";
                                    result.ResultDesc = "Operation Succeeded.";
                                }
                                else
                                {
                                    result.ResultCode = cashOut.responseCode;
                                    result.ResultDesc = cashOut.responseMessage;
                                }
                                string _transCashOutID = "None";
                                if (cashOut.transData != null)
                                {
                                    if (cashOut.transData.Length > 0)
                                    {
                                        _transCashOutID = cashOut.transData.FirstOrDefault().transCashOutID;
                                    }   
                                }
                                result.OTPRefCode = cashOut.otpRefCode;
                                result.OTPRefNo = cashOut.otpRefNo;
                                result.TransactionID = cashOut.transID;
                                result.TransactionCashOutID = _transCashOutID;
                                result.Amount = Values.Amount;

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.ResultCode = "005";
                    result.ResultDesc = "Could not make transaction " + ex.Message.ToString();
                }
            }
            else
            {
                result.ResultCode = "201";
                result.ResultDesc = "Please correct your amount.";
            }

            try
            {
                tbl_request_uat _log = new tbl_request_uat
                {
                    Msisdn = Values.Msisdn,
                    IP = _remoteIP,
                    ResultDesc = result.ResultDesc,
                    Amount = Convert.ToInt32(Values.Amount),
                    RequestDate = DateTime.Now,
                    ResultCode = result.ResultCode,
                    Status = true,
                    TransactionID = Values.TransactinoID,
                    Type = "UAT",
                    Username = _email,
                };
                db.tbl_request_uat.Add(_log);
                db.SaveChanges();
            }
            catch (Exception)
            {
                // insert log here
            }
            return result;
        }
    }
    public class RequestCashOutUAT
    {
        public string Msisdn { get; set; } = "None";
        public double Amount { get; set; } = 0;
        public string Remark { get; set; } = "None";
        public string TransactinoID { get; set; } = "None";
    }
    public class ResultCashOutUAT
    {
        public string APIToken { get; set; } = "None";
        public string OTPRefCode { get; set; } = "None";
        public string OTPRefNo { get; set; } = "None";
        public string TransactionID { get; set; } = "None";
        public string TransactionCashOutID { get; set; } = "None";
        public string ResultCode { get; set; } = "01";
        public string ResultDesc { get; set; } = "Please try again later.";
        public string Msisdn { get; set; }
        public double Amount { get; set; }
    }

}
