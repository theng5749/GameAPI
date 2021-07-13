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
    [Authorize]
    public class UnsubscribeController : ApiController
    {
        public ResultUnsubscribe Post(RequestUnsubscribe request)
        {
            ResultUnsubscribe result = new ResultUnsubscribe();
            CodaPayDBEntities db = new CodaPayDBEntities();
            string _email = User.Identity.Name;
            string _username = string.Empty;
            string _header = "LaoTelecom";
            string _company = "Lao Telecom";
            try
            {
                if (request.Channel.ToUpper() == "USSD")
                {
                    if (!string.IsNullOrWhiteSpace(request.USSDCode))
                    {
                        var checkcode = db.tbl_header.Where(x => x.USSDCode == request.USSDCode).FirstOrDefault();
                        if (checkcode != null)
                        {
                            _username = checkcode.Username;
                            _header = checkcode.HeaderName;
                            _company = checkcode.Company;
                            if (request.USSDCode == "727") //727 Playzone
                            {
                                //Connect to API Playzone
                                if (MainClass.CanMemberPlayzone(request.Msisdn) != true)
                                {
                                    result.ResultCode = "102";
                                    result.ResultDesc = $"ຂໍອະໄພ, ບໍ່ສາມາດຍົກເລີກບໍລິການເກມຂອງ {_company}";
                                } 
                            }
                        }
                    }
                    else
                    {
                        result.ResultCode = "101";
                        result.ResultDesc = "Please input USSD code";
                    }
                }
                else
                {
                    _username = _email;
                    var checkcode = db.tbl_header.Where(x => x.Username == _email).FirstOrDefault();
                    if (checkcode != null)
                    {
                        _username = checkcode.Username;
                        _header = checkcode.HeaderName;
                        _company = checkcode.Company;
                    }
                }

                //Check member
                var member = db.tbl_member.Where(x => x.Msisdn == request.Msisdn.Trim() && x.Username == _username && x.Status == true).FirstOrDefault();
                if (member != null)
                {
                    //Update tbl_member
                    member.CancelDate = DateTime.Now;
                    member.ResultDesc = "Unsubscribe";
                    member.ResultCode = "300";
                    member.Status = false;
                    db.Entry(member).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    //Submit SMS
                    smservice sms = new smservice();
                    string Message = $"ສຳເລັດ, ຍົກເລີກບໍລິການເກມຂອງ {_company}";
                    var servicesms = sms.SubmitSMS(request.Msisdn, Message, _header);
                    if (servicesms)
                    {
                        result.ResultCode = "200";
                        result.ResultDesc = "Operation Succeeded";
                    }
                    else
                    {
                        result.ResultCode = "2000";
                        result.ResultDesc = "Operation Success SMS not send";
                    }
                }
                else
                {
                    result.ResultCode = "100";
                    result.ResultDesc = $"ຂໍອະໄພ, ບໍ່ພົບເບີຂອງທ່ານໃນບໍລິການເກມຂອງ {_company}";
                }
            }
            catch (Exception)
            {
            }

            //Get header detail
            var _detail = db.tbl_header.Where(x => x.Username == _email).FirstOrDefault();
            if (_detail != null)
            {
                tbl_cancel_log log = new tbl_cancel_log
                {
                    CancelDate = DateTime.Now,
                    Gamename = _detail.Company,
                    Msisdn = request.Msisdn,
                    ResultCode = result.ResultCode,
                    ResultDesc = result.ResultDesc,
                    Username = _email,
                    Channel = request.Channel,
                    USSDCode = request.USSDCode
                };
                db.tbl_cancel_log.Add(log);
                db.SaveChanges();
            }
            return result;
        }
    }
    public class RequestUnsubscribe
    {
        public string Msisdn { get; set; }
        public string USSDCode { get; set; }
        public string Channel { get; set; }
    }
    public class ResultUnsubscribe
    {
        public string ResultCode { get; set; } = "001";
        public string ResultDesc { get; set; } = "Please try again later";
    }
}
