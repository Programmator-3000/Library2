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
            return View();
        }


        public ActionResult UserGuide()
        {
            return View();
        }
    }
}
