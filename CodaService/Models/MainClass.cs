using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using CodaService.ocsSrv;
using CodaService.smsapi;
using System.Threading.Tasks;
using CodaService.Controllers;
using RestSharp;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace CodaService.Models
{
    public class MainClass
    {
        public static string GetRandom()
        {
            string result = string.Empty;
            Random r = new Random();
            int rInt = r.Next(00001, 99999);
            result = Convert.ToString(rInt);
            return result; 
        }
        public static bool CheckTransaction(string TransactionID)
        {
            CodaPayDBEntities db = new CodaPayDBEntities();
            bool result = false;
            try
            {
                //await Task.Delay(200);
                int _check = db.tbl_request_log_production.Where(x => x.TransactionID == TransactionID).Count();
                if (_check > 0)
                {
                    result = true;
                }
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }
        public static async Task<ResultDuductPrivate> DuductBalancePrivate(RequestDeducts req, ReqDeduct reqDeduct)
        {
            CodaPayDBEntities db = new CodaPayDBEntities();
            ResultDuductPrivate result = new ResultDuductPrivate();
            ocsService ocs = new ocsService();
            ResultHeader resultHeader = new ResultHeader();
            ResultAdjust resultAdjust = new ResultAdjust();
           
            try
            {
                System.Threading.Thread.Sleep(2000);
                var requestocsservice = ocs.AdjustmentBalance(req.Msisdn, "C_MAIN_ACCOUNT", -Convert.ToInt64(req.Amount), reqDeduct.OCSUser, reqDeduct.OCSPass);
                resultHeader = requestocsservice.ResultHeader;
                resultAdjust = requestocsservice.Respon;
                result.ResultCode = resultHeader.ResultCode;
                result.ResultDesc = resultHeader.ResultDesc;
                result.OldAmount = Convert.ToInt32(resultAdjust.NewBalance);
                result.NewBalance = Convert.ToInt32(resultAdjust.OldBalance);
                if (result.ResultCode == "0")
                {
                    result.ResultCode = "200";
                    result.ResultDesc = "Operation Success";
                    result.Status = true;
                    string Message = $"ຕັດເງິນເບີໂທຂອງທ່ານຈຳນວນ {req.Amount} ຈາກບໍລິການ {reqDeduct.Company}";
                    
                    //Submit SMS
                    result.SMSStatus  = await SubmitSMS(req.Msisdn.Trim(), Message.Trim(), reqDeduct.Header);
                }
            }
            catch (Exception ex)
            {
                result.ResultCode = "602";
                result.ResultDesc = "ERR: " + ex.Message;
            }

            //Insert Log
            tbl_confirm_deduct log = new tbl_confirm_deduct
            {
                Cardname = req.CardName,
                DeductDate = DateTime.Now,
                DeductType = "Recurring",
                RequestType = req.RequestType,
                Email = reqDeduct.Email,
                Gamename = req.GameName,
                IPAddress = reqDeduct.IPAddress,
                Msisdn = req.Msisdn,
                NewBalance = Convert.ToInt32(result.NewBalance),
                OldBalance = Convert.ToInt32(result.OldAmount),
                Operator = reqDeduct.Operator,
                RefCode= reqDeduct.RefCode,
                ResultCode = result.ResultCode,
                ResultDesc = result.ResultDesc,
                SMSStatus = result.SMSStatus,
                TransactionID = req.TransactionID,
                Username = reqDeduct.Email
            };
            db.tbl_confirm_deduct.Add(log);
            db.SaveChanges();
            return result;
        }
        public static async Task<bool> SubmitSMS(string Msisdn, string Message, string Header)
        {
            bool result = false;
            smservice sms = new smservice();
            try
            {
                System.Threading.Thread.Sleep(1000);
                result = sms.SubmitSMS(Msisdn, Message, Header);
            }
            catch (Exception)
            {
                result = false;
            }
            finally
            {
                sms.Dispose();   
            }
            return result;
        }

        public static bool CreateMemberPlayzone(string Msisdn)
        {
            bool result = false;
            string apikey = MD5.Create(Msisdn).ToString();
            try
            {
                var client = new RestClient("http://la.playzone.id/api/set_member_playzone");
                var request = new RestRequest(Method.POST);
                string datetimerequest = DateTime.Now.ToString("yyyyMMddHHmmss");
                //request.AddHeader("postman-token", "a39876cc-0239-9886-c511-b1919b5792b9");
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("apikey", apikey);
                request.AddHeader("content-type", "application/json");
                request.AddParameter("application/json", "{\n\t\"msisdn\": "+ Msisdn+",\n\t\"trx_date\": "+ datetimerequest + "\n}", ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                if (response.Content != null)
                {
                    ResultPlayzone playzone = new ResultPlayzone();
                    dynamic json = JsonConvert.DeserializeObject<ResultPlayzone>(response.Content);
                    playzone = json;
                    if (playzone.status == "OK")
                    {
                        result = true;
                    }
                }
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        public static bool CanMemberPlayzone(string Msisdn)
        {
            bool result = false;
            string apikey = MD5.Create(Msisdn).ToString();
            string datetimerequest = DateTime.Now.ToString("yyyyMMddHHmmss");
            try
            {
                var client = new RestClient("http://la.playzone.id/api/unset_member_playzone");
                var request = new RestRequest(Method.POST);
                //request.AddHeader("postman-token", "a81dd792-b7df-d995-5d8f-d5b7958e1a7c");
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("apikey", apikey);
                request.AddHeader("content-type", "application/json");
                request.AddParameter("application/json", "{\n\t\"msisdn\": "+ Msisdn.Trim() +",\n\t\"trx_date\": "+ datetimerequest + "\n}", ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                ResultPlayzone playzone = new ResultPlayzone();
                dynamic json = JsonConvert.DeserializeObject<ResultPlayzone>(response.Content);
                playzone = json;
                if (playzone.status == "OK")
                {
                    result = true;
                }
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }
        public class ResultDuductPrivate
        {
            public string ResultCode { get; set; } = "601";
            public string ResultDesc { get; set; } = "Please try again";
            public double OldAmount { get; set; } = 0;
            public double NewBalance { get; set; } = 0;
            public bool Status { get; set; } = false;
            public bool SMSStatus { get; set; } = false;
        }
        public class ReqDeduct
        {
            public string Email { get; set; }
            public string OCSUser { get; set; }
            public string OCSPass { get; set; }
            public string IPAddress { get; set; }
            public string Operator { get; set; }
            public string Header { get; set; }
            public string Company { get; set; }
            public string RefCode { get; set; }
        }
        public class ResultPlayzone
        {
            public string status { get; set; }
            public string message { get; set; }
        }
    }
} 