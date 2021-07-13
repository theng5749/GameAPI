using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI.WebControls;

namespace SendSMS
{
    /// <summary>
    /// Summary description for smservice
    /// </summary>
    [WebService(Namespace = "http://laotel.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class smservice : System.Web.Services.WebService
    {
        [WebMethod]
        public bool SubmitSMS(string msisdn, string message, string header)
        {
            bool result = false;
            try
            {
                ManageClass manage = new ManageClass();
                result = manage.sendSMS(msisdn.Trim(), message.Trim(), header.Trim());
            }
            catch (Exception)
            {
            }
            return result;
        }
    }
}
