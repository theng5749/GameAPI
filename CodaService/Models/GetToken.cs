using CodaService.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodaService.Models
{
    public class GetToken
    {
        //Token
        public static ResultToken MyToken(string username, string password)
        {

            ResultToken result = new ResultToken();
            var client = new RestClient("http://115.84.121.101:31153/ewallet-ltc-api/oauth/token.service");
            var request = new RestRequest(Method.POST);
            request.AddHeader("postman-token", "93429857-dd66-2f91-0f10-50ef6971ec1e");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", "username=" + username + "&password=" + password + "&grant_type=client_credentials", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            if (response.IsSuccessful)
            {
                dynamic json = JsonConvert.DeserializeObject<ResultToken>(response.Content);
                result = json;
            }
            return result;
        }
        //CashIn
        //public static ResultRequestCashIn RequestCashIn(RequestDeduct values, RequestValues req)
        //{
        //    ResultRequestCashIn result = new ResultRequestCashIn();
        //    var client = new RestClient("http://115.84.121.101:31153/ewallet-ltc-api/cash-management/request-cash-in.service");
        //    var request = new RestRequest(Method.POST);
        //    request.AddHeader("postman-token", "959198b2-0326-ac0d-5631-f186cf0e3a85");
        //    request.AddHeader("cache-control", "no-cache");
        //    request.AddHeader("content-type", "application/json");
        //    request.AddParameter("application/json",
        //        "{\n\"apiKey\":\"b7b7ef0830ff278262c72e57bc43d11f\"," +
        //        "\n\"apiToken\": \"" + req.Token + "\"," +
        //        "\n\"transID\": \"" + req.TransactionID + "\"," +
        //        "\n\"requestorID\":\"14\"," +
        //        "\n\"toAccountOption\":\"REF\"," +
        //        "\n\"toAccountRef\":\"" + values.Msisdn.Trim() + "\"," +
        //        "\n\"transAmount\":\"" + values.Amount.Trim() + "\"," +
        //        "\n\"transCurrency\":\"LAK\"," +
        //        "\n\"transRemark\":\"" + values.Remark + "\"," +
        //        "\n\"transRefCol1\":\"" + values.Ref + "\"," +
        //        "\n\"transRefCol2\":\"" + values.Ref2 + "\"," +
        //        "\n\"transRefCol3\":\"" + values.Ref3 + "\"," +
        //        "\n\"transRefCol4\":\"" + values.Ref4 + "\"," +
        //        "\n\"transCheckSum\":\"" + req.CheckSum.Trim() + "\"\n}", ParameterType.RequestBody);
        //    IRestResponse response = client.Execute(request);
        //    if (response != null)
        //    {
        //        dynamic json = JsonConvert.DeserializeObject<ResultRequestCashIn>(response.Content);
        //        result = json;
        //    }

        //    return result;
        //}
        //Confirm CashIn
        public static ResultConfirmCashIn ConfirmCashIn(RequestValues req)
        {
            ResultConfirmCashIn result = new ResultConfirmCashIn();
            var client = new RestClient("http://115.84.121.101:31153/ewallet-ltc-api/cash-management/confirm-cash-in.service");
            var request = new RestRequest(Method.POST);
            request.AddHeader("postman-token", "d5ec1a40-848a-131b-c0d0-62e799d74ea7");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json",
                "{\n    \"apiKey\": \"efca1d20e1bdfc07b249e502f007fe0c\"," +
                "\n    \"apiToken\": " + req.Token + "," +
                "\n    \"transID\": \"" + req.TransactionID + "\"," +
                "\n    \"requestorID\": \"14\"," +
                "\n    \"transCashInID\": \"" + req.ResultRequestCashInID + "\"\n}", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            if (response != null)
            {
                dynamic json = JsonConvert.DeserializeObject<ResultConfirmCashIn>(response.Content);
                result = json;
            }
            return result;
        }

        //CashOut
        public static ResultCashOut RequestCashOut(RequestCashOutUAT values, RequestValues req, string email, int reqID)
        {
            string Ref = "", Ref2 = "", Ref3 = "", Ref4 = "";
            ResultCashOut result = new ResultCashOut();
            try
            {
                var client = new RestClient("http://115.84.121.101:31153/ewallet-ltc-api/cash-management/request-cash-out.service");
                var request = new RestRequest(Method.POST);
                request.AddHeader("postman-token", "653bdf67-bd40-7e87-3223-2730acf72d98");
                request.AddHeader("cache-control", "no-cache");
                request.AddParameter("undefined",
                    "{\r\n    \"apiKey\": \"e31d5c83bd807fde9d6aed81f6076624\"," +
                    "\r\n    \"apiToken\": \"" + req.Token + "\"," +
                    "\r\n    \"transID\": \"" + req.TransactionID + "\"," +
                    "\r\n    \"requestorID\": \"" + reqID + "\"," +
                    "\r\n    \"fromAccountOption\": \"REF\"," +
                    "\r\n    \"fromAccountRef\": \"" + values.Msisdn + "\"," +
                    "\r\n    \"transAmount\": \"" + values.Amount + "\"," +
                    "\r\n    \"transCurrency\": \"LAK\"," +
                    "\r\n    \"transRemark\": \"" + values.Remark + "\"," +
                    "\r\n    \"transRefCol1\": \"" + Ref + "\"," +
                    "\r\n    \"transRefCol2\": \"" + Ref2 + "\"," +
                    "\r\n    \"transRefCol3\": \"" + Ref3 + "\"," +
                    "\r\n    \"transRefCol4\": \"" + Ref4 + "\"," +
                    "\r\n    \"transCheckSum\": \"" + req.CheckSum + "\"\r\n}", ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                if (response != null)
                {
                    dynamic json = JsonConvert.DeserializeObject<ResultCashOut>(response.Content);
                    result = json;
                }
            }
            catch (Exception ex)
            {
                result.responseCode = "006";
                result.responseMessage = "Could not connect to mmoney api " + ex.Message.ToString();
            }
            return result;
        }
        public static ResultConfirmCashOutAPI ConfirmCashOut(RequestConfirmCashOutAPI req)
        {
            ResultConfirmCashOutAPI result = new ResultConfirmCashOutAPI();
            try
            {
                var client = new RestClient("http://115.84.121.101:31153/ewallet-ltc-api/cash-management/confirm-cash-out.service");
                var request = new RestRequest(Method.POST);
                request.AddHeader("postman-token", "d4325cad-04f7-c06c-62a5-0ae71f66c63c");
                request.AddHeader("cache-control", "no-cache");
                request.AddParameter("undefined", "{\r\n    \"apiKey\": \"ddeec36baa6f07a37225d0f8b7b8ed09\","+
                    "\r\n    \"apiToken\": \""+ req.apiToken.Trim() + "\"," +
                    "\r\n    \"transID\": \""+ req.transID +"\"," +
                    "\r\n    \"requestorID\": \""+ req.requestorID + "\"," +
                    "\r\n    \"transCashOutID\": \""+ req.transCashOutID +"\"," +
                    "\r\n    \"otpRefNo\": \""+ req.otpRefNo +"\"," +
                    "\r\n    \"otpRefCode\": \""+ req.otpRefCode +"\"," +
                    "\r\n    \"otp\": \""+ req.otp +"\"\r\n}", ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                dynamic json = JsonConvert.DeserializeObject<ResultConfirmCashOutAPI>(response.Content);
                result = json;
            }
            catch (Exception ex)
            {
                result.responseCode = "006";
                result.responseMessage = "Could not connect to mmoney api " + ex.Message.ToString();
            }
            return result;
        }
    }
    //Token
    public class ResultToken
    {
        public string accessToken { get; set; }
        public string tokenType { get; set; }
        public int expiresIn { get; set; }
        public string userName { get; set; }
        public string issued { get; set; }
        public string expiry { get; set; }
    }

    //CashIn
    public class RequestValues
    {
        public string CheckSum { get; set; }
        public string Token { get; set; }
        public string TransactionID { get; set; }
        public string ResultRequestCashInID { get; set; }
    }
    public class ResultRequestCashIn
    {
        public string _14562 { get; set; }
        public Transdata[] transData { get; set; }
        public string responseCode { get; set; }
        public string responseMessage { get; set; }
        public string responseStatus { get; set; }
        public string transID { get; set; }
        public int processTime { get; set; }
        public string serverDatetime { get; set; }
        public long serverDatetimeMs { get; set; }
    }
    public class Transdata
    {
        public string transCashInID { get; set; }
        public string transStatus { get; set; }
        public string accountNo { get; set; }
        public string accountNameEN { get; set; }
        public string accountRef { get; set; }
        public string accountType { get; set; }
        public string transExpiry { get; set; }
    }
    public class ResultConfirmCashIn
    {
        public string _35069 { get; set; }
        public string transCashInID { get; set; }
        public string transStatus { get; set; }
        public string accountNo { get; set; }
        public string accountNameEN { get; set; }
        public string accountRef { get; set; }
        public string accountType { get; set; }
        public string transCancelCode { get; set; }
        public string transPointRef { get; set; }
        public string transPoint { get; set; }
        public string transRequestDate { get; set; }
        public string transCashInDate { get; set; }
        public string transRefCol1 { get; set; }
        public string transRefCol2 { get; set; }
        public string transRefCol3 { get; set; }
        public string transRefCol4 { get; set; }
        public string transExpiry { get; set; }
        public string responseCode { get; set; }
        public string responseMessage { get; set; }
        public string responseStatus { get; set; }
        public string transID { get; set; }
        public int processTime { get; set; }
        public string serverDatetime { get; set; }
        public long serverDatetimeMs { get; set; }
    }

    //CashOut
    public class RequestCashOut
    {
        public string apiKey { get; set; }
        public string apiToken { get; set; }
        public string transID { get; set; }
        public string requestorID { get; set; }
        public string fromAccountOption { get; set; }
        public string fromAccountRef { get; set; }
        public string transAmount { get; set; }
        public string transCurrency { get; set; }
        public string transRemark { get; set; }
        public string transRefCol1 { get; set; }
        public string transRefCol2 { get; set; }
        public string transRefCol3 { get; set; }
        public string transRefCol4 { get; set; }
        public string transCheckSum { get; set; }
    }
    public class ResultCashOut
    {
        public string _78550 { get; set; }
        public TransdataCashOut[] transData { get; set; }
        public string otpRefNo { get; set; }
        public string otpRefCode { get; set; }
        public string otpLockFlag { get; set; }
        public string openSMS { get; set; }
        public string responseCode { get; set; }
        public string responseMessage { get; set; }
        public string responseStatus { get; set; }
        public string transID { get; set; }
        public int processTime { get; set; }
        public string serverDatetime { get; set; }
        public long serverDatetimeMs { get; set; }
    }
    public class TransdataCashOut
    {
        public string transCashOutID { get; set; }
        public string transStatus { get; set; }
        public string accountNo { get; set; }
        public string accountNameEN { get; set; }
        public string accountRef { get; set; }
        public string accountType { get; set; }
        public string transExpiry { get; set; }
    }
    public class RequestConfirmCashOutAPI
    {
        public string apiKey { get; set; }
        public string apiToken { get; set; }
        public string transID { get; set; }
        public string requestorID { get; set; }
        public string transCashOutID { get; set; }
        public string otpRefNo { get; set; }
        public string otpRefCode { get; set; }
        public string otp { get; set; }
    }
    public class ResultConfirmCashOutAPI
    {
        public string _59432 { get; set; }
        public int processTime { get; set; }
        public string serverDatetime { get; set; }
        public long serverDatetimeMs { get; set; }
        public string responseCode { get; set; }
        public string responseMessage { get; set; }
        public string responseStatus { get; set; }
    }

}



