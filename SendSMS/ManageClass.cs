using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml;

namespace SendSMS
{
    public class ManageClass
    {
        public bool sendSMS(string msisdn, string message, string header)
        {
            bool result = false;
            try
            {
                string requestURL = getValueXML("sendsms");
                WebRequest req;
                WebResponse rp;
                string msg = Encodeurl(message.Trim());
                string SmsData = "";
                SmsData = "PhoneNumber=856" + msisdn.Trim();
                SmsData += "&Text=" + msg.Trim();
                SmsData += "&Sender=" + header.Trim();
                SmsData += "&ReceiptRequested=Yes";
                req = WebRequest.Create(requestURL + SmsData);
                req.Timeout = 10000;
                req.Credentials = CredentialCache.DefaultCredentials;
                rp = req.GetResponse();
              //  WriteLogSendSMSResponse(msisdn, header, ((HttpWebResponse)rp).StatusDescription.ToUpper().Trim());
                if (((HttpWebResponse)rp).StatusDescription.ToUpper().Trim() == "OK")
                {
                    result = true;
                }
                rp.Dispose();
            }
            catch { }
            return result;
        }
        private string Encodeurl(string str)
        {
            //str = str.Replace("%", "%25");
            //str = str.Replace(" ", "%20");
            //str = str.Replace(@"""", "%22");
            //str = str.Replace("\n", "%0A");
            //str = str.Replace("\r", "%0D");
            //str = str.Replace("!", "%2521");
           // str = str.Replace("#", "%2523");
            //str = str.Replace("$", "%24");
            //str = str.Replace("&", "%26");
            //str = str.Replace("'", "%27");
            //str = str.Replace("(", "%28");
            //str = str.Replace(")", "%29");
            //str = str.Replace("*", "%252A");
            //str = str.Replace("+", "%2B");
            //str = str.Replace(",", "%2C");
            //str = str.Replace("/", "%2F");
            //str = str.Replace(":", "%3A");
            //str = str.Replace(";", "%3B");
            //str = str.Replace("=", "%3D");
            //str = str.Replace("?", "%3F");
            //str = str.Replace("@", "%40");
            //str = str.Replace("[", "%5B");
            //str = str.Replace("]", "%5D");
            //str = str.Replace("-", "%2D");
            str = str.Replace("%", "%25");
            str = str.Replace(" ", "%20");
            str = str.Replace(@"""", "%22");
            str = str.Replace("\n", "%0A");
            str = str.Replace("\r", "%0D");
            str = str.Replace("!", "%21");
            str = str.Replace("#", "%23");
            str = str.Replace("$", "%24");
            str = str.Replace("&", "%26");
            str = str.Replace("'", "%27");
            str = str.Replace("(", "%28");
            str = str.Replace(")", "%29");
            str = str.Replace("*", "%2A");
            str = str.Replace("+", "%2B");
            str = str.Replace(",", "%2C");
            str = str.Replace("/", "%2F");
            str = str.Replace(":", "%3A");
            str = str.Replace(";", "%3B");
            str = str.Replace("=", "%3D");
            str = str.Replace("?", "%3F");
            str = str.Replace("@", "%40");
            str = str.Replace("[", "%5B");
            str = str.Replace("]", "%5D");
            return str;
        }
        public string getValueXML(string NoteName)
        {
            string rp = "";
            try
            {
                string path = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                var directory = System.IO.Path.GetDirectoryName(path);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(directory + "\\data.xml");

                // Get elements
                XmlNodeList girlAddress = xmlDoc.GetElementsByTagName(NoteName.ToUpper().Trim());
                rp = girlAddress[0].InnerText.ToString().Trim();
            }
            catch { }
            return rp;
        }
    }
}