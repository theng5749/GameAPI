using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebRedirectHE.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //Get Header http://115.84.121.12:52130/api/showheader
            //Forward to 
            Response.Redirect("https://wapshop.gameloft.com/laotel_dev/5.0/index.php");

            return View("");
        }
    }
}