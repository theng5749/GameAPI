using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace CodaService.Controllers
{
    [Authorize(Roles = "Admin, Super Admin")]
    public class GetHEController : ApiController
    {
        public ResultHE Get()
        {
            ResultHE result = new ResultHE();
            result.ResultCode = "100";
            result.ResultDesc = "System error please try again.";
           string _email = User.Identity.Name;
           // string _email = "ferbi.wiratno@gameloft.com";
            result.Email = _email; 
            result.Msisdn = "None";
            try
            {
                string URL = "http://115.84.121.12:52130/api/showheader";
                string JsonString = "";
                WebRequest request = WebRequest.Create(URL);
                request.Credentials = CredentialCache.DefaultCredentials;
                WebResponse response = request.GetResponse();
                Console.WriteLine(((HttpWebResponse)response).StatusDescription);
                using (Stream dataStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(dataStream);
                    JsonString = reader.ReadToEnd();
                    JavaScriptSerializer oJson = new JavaScriptSerializer();
                    string _getResult = oJson.Deserialize<string>(JsonString);
                    result.Msisdn = _getResult;
                    result.ResultCode = "200";
                    result.ResultDesc = "Operation Succeeded.";
                }
            }
            catch (Exception ex)
            {
                result.ResultCode = "101";
                result.ResultDesc = "Could not connect HE service." + ex.Message;
            }
            return result;
        }
        public class ResultHE
        {
            public string Msisdn { get; set; }
            public string ResultCode { get; set; }
            public string ResultDesc { get; set; }
            public string Email { get; set; }
        }
    }
}
