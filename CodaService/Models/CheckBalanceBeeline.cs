using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CodaService.OcsBeeline;
using CodaService.Models;
namespace CodaService.Models
{
    public class CheckBalanceBeeline
    {
        public ResultBalanceBeeline Post(string Msisdn)
        {
            string BeelineUser = System.Configuration.ConfigurationManager.AppSettings["BeelineUser"];
            string BeelinePass = System.Configuration.ConfigurationManager.AppSettings["BeelinePass"];
            ResultBalanceBeeline result = new ResultBalanceBeeline();
            result.ResultCode = "100";
            result.ResultDesc = "Sorry, Please try again later.";
            Services beeline = new Services();
            queryBalanceRequest balanceRequest = new queryBalanceRequest();
            headerRequest _header = new headerRequest();
            _header.chanel = "Codapay";
            _header.userid = BeelineUser;
            _header.password = BeelinePass;
            DateTime dt = DateTime.Now;
            string tran = dt.ToString("yyyyMMddHHmmss");
            _header.trans_id = Msisdn +  "Coda" + tran.Trim();
            balanceRequest.header = _header;
            balanceRequest.msisdn = Msisdn;
            try
            {
                queryBalanceResult balanceResult = new queryBalanceResult();
                balanceDetail balanceDetail = new balanceDetail();
                var _beeline = beeline.queryBalance(balanceRequest);
                var _balanceDetail = _beeline.balanceDetail;
                double Mainbalance = 0;
                for (int i = 0; i < _balanceDetail.Length; i++)
                {
                    if (_balanceDetail[i].balanceType.Trim() == "2000")
                    {
                        Mainbalance = Convert.ToDouble(_balanceDetail[i].balance);
                    }
                }
                result.Balance = Mainbalance;
                result.ResultCode = "200";
                result.ResultDesc = "Operation succeeded.";
            }
            catch (Exception)
            {
                result.ResultCode = "102";
                result.ResultDesc = "Could not connect to ocs service (beeline).";
            }
            return result;
        }
    }
    public class ResultBalanceBeeline
    {
        public string ResultCode { get; set; }
        public string ResultDesc { get; set; }
        public double Balance { get; set; }
    }
}