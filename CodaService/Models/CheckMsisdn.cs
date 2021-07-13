using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodaService.Models
{
    public class CheckMsisdn
    {
        public ResultCheckMsisdn CheckPhone(string Msisdn)
        {
            ResultCheckMsisdn result = new ResultCheckMsisdn();

            if ((Msisdn.StartsWith("205")) && (Msisdn.Length == 10) || (Msisdn.StartsWith("856205") && Msisdn.Length==13))
            {
                result.Msisdn =  Msisdn.PadRight(10);
                result.ResultCode = "200";
                result.ResultDescc = "Operation Succeeded.";
                result.Type = "LTC";
            }
            else if ((Msisdn.StartsWith("305") && Msisdn.Length==9) || (Msisdn.StartsWith("856305") &&  Msisdn.Length ==12))
            {
                result.Msisdn = Msisdn.PadRight(9);
                result.ResultCode = "200";
                result.ResultDescc = "Operation Succeeded.";
                result.Type = "LTC";
            }
            else if ((Msisdn.StartsWith("207") && Msisdn.Length == 10) || (Msisdn.StartsWith("856207") && Msisdn.Length == 13))
            {
                result.Msisdn = Msisdn.PadRight(9);
                result.ResultCode = "200";
                result.ResultDescc = "Operation Succeeded.";
                result.Type = "TPlus";
            }
            else if ((Msisdn.StartsWith("307") && Msisdn.Length == 9) || (Msisdn.StartsWith("856307") && Msisdn.Length == 12))
            {
                result.Msisdn = Msisdn.PadRight(9);
                result.ResultCode = "200";
                result.ResultDescc = "Operation Succeeded.";
                result.Type = "TPlus";
            }
            else
            {
                result.Msisdn = Msisdn;
                result.ResultCode = "500";
                result.ResultDescc = "Invalid phone number";
            }           
            return result;
        }
    }
    public class ResultCheckMsisdn
    {
        public string Type { get; set; }
        public string Msisdn { get; set; }
        public string ResultCode { get; set; }
        public string ResultDescc { get; set; }
    }
}