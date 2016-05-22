using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Library2.Models;

namespace Library2.Controllers
{
    public class HomeController : Controller
    {
        

        public ActionResult Index()
        {
            ViewBag.Message = "Online-library by Denizzz";
            //Library2.Models.UserBase.CurrentUser = UserBase.Users.Find(d => d.Email == "hfsx@ukr.net");
           // Library2.Models.UserBase.CurrentUser = UserBase.Users.Find(d => d.Email == "denizz@ukr.net");
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
