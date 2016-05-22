using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using Library2.Models;

namespace Library2.Controllers
{
    public class AccountController : Controller
    {
        dbBooks MyDb = new dbBooks();


        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public ActionResult Login(User u, string returnUrl)
        {

            if (UserBase.IsRegistered(ref u))
            {
                UserBase.CurrentUser = u;
                return RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError("", "E-mail указан неверно.");
            return View(u);
        }


        public ActionResult LogOff()
        {

            UserBase.CurrentUser = null;
            return RedirectToAction("Index", "Home");
        }


        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }


        public ActionResult Register()
          {
                return View();
          }


        [HttpPost]
        public ActionResult Register(User model)
          {
              if (UserBase.IsRegistered(ref model))
              {
                  ViewBag.ExistsError = "Такой пользователь уже зарегистрирован";
                  return View(model);
              }
              if (ModelState.IsValid && !UserBase.IsRegistered(ref model))
              {
                  UserBase.UserAdd(model.Email);
                  UserBase.CurrentUser = UserBase.Users[UserBase.Users.Count - 1];
                  return RedirectToAction("Index", "Home");
              }
              return View(model);
          }

        public ActionResult History()
          {
              ViewBag.Count = MyDb.MyRecords.Where(x => x.User == UserBase.CurrentUser.Email).ToList<Record>().Count;
              return View(MyDb.MyRecords.Where(x=>x.User==UserBase.CurrentUser.Email));
          }

    }
}
