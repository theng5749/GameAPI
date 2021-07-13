using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CodaService.Models;
using CodaService.smsapi;

namespace CodaService.Controllers
{
   [Authorize(Roles = "Admin, Super Admin")]
    public class SendSMSController : ApiController
    {
        public ResultSendSMS Post(RequestSendSMS Value)
        {
            ResultSendSMS result = new ResultSendSMS();
            CodaPayDBEntities db = new CodaPayDBEntities();
            smservice sms = new smservice();

            string _email = User.Identity.Name;
           // string _email = "codapay@gmail.com";
            result.ResultCode = "100";
            result.ResultDesc = "System error please try again.";
            result.Msisdn = Value.Msisdn;
            result.Email = _email;
            if (Value.Message == null)
            {
                result.ResultCode = "110";
                result.ResultDesc = "Message could not be null.";
                goto SafeExit;
            }
            if (Value.Msisdn == "" || Value.Message == "")
            {
                result.ResultCode = "104";
                result.ResultDesc = "Invalid input parameters. (SMS)";
                goto SafeExit;
            }          
            else
            {
                //Check Phone number
                CheckMsisdn checkMsisdn = new CheckMsisdn();
                var _checkPhone= checkMsisdn.CheckPhone(Value.Msisdn);
                if (_checkPhone.ResultCode =="200")
                {
                    if (Value.Message.Length > 160)
                    {
                        result.ResultCode = "109";
                        result.ResultDesc = "Message is over length.";
                        goto SafeExit;
                    }
                    var _getHeader = db.tbl_header.Where(x => x.Username == _email).FirstOrDefault();
                    string _headername = "LaoTelecom";
                    if (_getHeader != null)
                    {
                        _headername = _getHeader.HeaderName;
                    }
                    else
                    {
                        _headername = "LaoTelecom";
                    }
                    try
                    {
                        var servicesms = sms.SubmitSMS(_checkPhone.Msisdn, Value.Message, _headername);
                        string _status = Convert.ToString(servicesms.ToString());
                        if (_status == "True")
                        {
                            result.Msisdn = Value.Msisdn;
                            result.ResultCode = "200";
                            result.ResultDesc = "Operation Succeeded.";
                        }
                        else
                        {
                            result.ResultCode = "105";
                            result.ResultDesc = "Could not send sms.";
                        }
                    }
                    catch (Exception)
                    {
                        result.ResultCode = "101";
                        result.ResultDesc = "Could not connect sms service.";
                    }
                }
                else
                {
                        result.ResultCode = "114";
                        result.ResultDesc = "Invalid phone number.";
                }               
            }
            SafeExit:
            // Insert log
            try
            {
                tbl_sms_log sms_Log = new tbl_sms_log
                {
                    msisdn = Value.Msisdn,
                    message = Value.Message,
                    send_date = DateTime.Now,
                    resultCode = result.ResultCode,
                    resultDesc = result.ResultDesc,
                    email = _email
                };
                db.tbl_sms_log.Add(sms_Log);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                result.ResultCode = "115";
                result.ResultDesc = "Could not insert sms log." + ex.Message;
            }
            return result;
        }
    }
    public class RequestSendSMS
    {
        public string Msisdn { get; set; }
        public string Message { get; set; }
    }
    public class ResultSendSMS
    {
        public string Msisdn { get; set; }
        public string ResultCode { get; set; }
        public string ResultDesc { get; set; }
        public string Email { get; set; }
    }
}
