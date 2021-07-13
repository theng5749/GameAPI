//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Web.Http;
//using CodaService.Models;
//using System.IO;
//using System.Security.Cryptography;
//using System.Text;
//using RestSharp;

//namespace CodaService.Controllers
//{
//    public class DeductMoneyController : ApiController
//    {
//        [Authorize(Roles = "Super Admin")]
//        public ResultDeduct Post(RequestDeduct Values)
//        {
//            ResultDeduct result = new ResultDeduct();
//            string _email = User.Identity.Name;
//            string _remoteIP = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
//            string _tranID = Guid.NewGuid().ToString("N");
//            ResultToken _token = new ResultToken();
//            _token = GetToken.MyToken("","");
//            if (_token != null)
//            {
//                #region Payload and Checksum
//                string payload = "REF" + ","
//                                + Values.Msisdn.Trim() + ","
//                                + Values.Amount.Trim() + ","
//                                + "LAK" + ","
//                                + Values.Remark.Trim() + ","
//                                + Values.Ref.Trim() + ","
//                                + Values.Ref2.Trim() + ","
//                                + Values.Ref3.Trim() + ","
//                                + Values.Ref4.Trim() + ",ltc";
//                SHA256 mySHA256 = SHA256.Create();
//                byte[] hashValue = mySHA256.ComputeHash(Encoding.UTF8.GetBytes(payload));
//                var _checksum = Convert.ToBase64String(hashValue);
//                #endregion
//                if (_token != null && _checksum != null)
//                {
//                    RequestValues request = new RequestValues();
//                    request.Token = _token.accessToken.Trim();
//                    request.TransactionID = _tranID.Trim();
//                    request.CheckSum = _checksum.Trim();
//                    ResultRequestCashIn _reqCashIn = GetToken.RequestCashIn(Values, request);
//                    if (_reqCashIn != null)
//                    {
//                        request.ResultRequestCashInID = _reqCashIn.transData.FirstOrDefault().transCashInID;
//                        //Confirm cash in
//                        ResultConfirmCashIn _confirm = GetToken.ConfirmCashIn(request);
//                        if (_confirm.responseCode == "0000")
//                        {
//                            result.ResultCode = "200";
//                            result.ResultDesc = "Operation Succeeded";
//                            result.Msisdn = Values.Msisdn.Trim();
//                        }
//                    }
//                }
//            }
//            return result;
//        }
//    }
//    public class RequestDeduct
//    {
//        public string Msisdn { get; set; }
//        public string Amount { get; set; }
//        public string Remark { get; set; }
//        public string Ref { get; set; }
//        public string Ref2 { get; set; }
//        public string Ref3 { get; set; }
//        public string Ref4 { get; set; }
//    }
//    public class ResultDeduct
//    {
//        public string ResultCode { get; set; }
//        public string ResultDesc { get; set; }
//        public string Msisdn { get; set; }
//    }

   

//}
