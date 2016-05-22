using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Library2.Models;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;

namespace Library2.Controllers
{
    public class BooksController : Controller
    {

        public dbBooks MyDb = new dbBooks();
        static string tookstr;

        public ActionResult Index(bool flag = true)
        {
            ViewBag.TookMessage = tookstr;
            tookstr = null;
            ViewBag.ShowAll = true;
            if (flag)
            {
                ViewBag.ShowAll = true;
                return View(MyDb.MyBooks);
            }
            else
            {
                ViewBag.ShowAll = false;
                return View(MyDb.MyBooks.FindAll(p => p.Quantity != 0));
            }
        }


        public ActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Book b)
        {
            if (ModelState.IsValid)
            {
                MyDb.MAddBook(b);
                return RedirectToAction("Index");
            }

            return View(b);
        }


        public ActionResult Edit(int id = 0)
        {
            Book b = MyDb.MyBooks.Find(p => p.ID == id);
            if ( b== null)
            {
                return HttpNotFound();
            }
            return View(b);
        }


        [HttpPost]
        public ActionResult Edit(Book b)
        {
            if (ModelState.IsValid)
            {
                MyDb.MUpdateBook(b);
                return RedirectToAction("Index");
            }
            return View(b);
        }


        public ActionResult Delete(int id = 0)
        {
            Book b = null;
            foreach (Book x in MyDb.MyBooks)
            {
                if (x.ID == id) b = x;
            }
            if (b == null)
            {
                return HttpNotFound();
            }
            return View(b);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MyDb.MDelBook(id);
            return RedirectToAction("Index");
        }


        public async Task<ActionResult> TakeBook(int id = 0)
        {
            try
            {
                if (MyDb.BookTake(id))
                {
                    tookstr = "Congratulations! You have taken a book \"" + MyDb.MyBooks.Find(p => p.ID == id).Name + "\"";
                    await AsyncSendMessage("Поздравляем, вы взяли книгу \"" + MyDb.MyBooks.Find(p => p.ID == id).Name + "\"");
                }
                else
                    tookstr = "We`re sorry, but this book is no available at the moment.";
            }
            catch (Exception e)
            {
                tookstr = "We`re sorry, en error has occured while sending your email notofication! Maybe you should check you internet connection.";
            }
            return RedirectToAction("Index");
        }

        public ActionResult History(int id = 0)
        {
            ViewBag.Count = MyDb.MyRecords.Where(x => x.Book == MyDb.MyBooks.Find(d => d.ID == id).Name).ToList<Record>().Count;
            ViewBag.BookName = "\"" + MyDb.MyBooks.Find(d=> d.ID==id).Name +  "\"" + " by " + MyDb.MyBooks.Find(d=> d.ID==id).Author;
            return View(MyDb.MyRecords.Where(x => x.Book == MyDb.MyBooks.Find(d=> d.ID==id).Name));
        }

        public void SendMessage(string text)
        {
            MailAddress from = new MailAddress("rassylocnyjbot@gmail.com", "Library");
            MailAddress to = new MailAddress(UserBase.CurrentUser.Email);
            MailMessage m = new MailMessage(from, to);
            m.Subject = "Notification";
            m.Body = "<h2>"+ text +"</h2>";
            m.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.Credentials = new NetworkCredential(" rassylocnyjbot@gmail.com", "12365478");
            smtp.EnableSsl = true;
            smtp.Send(m);
        }

        public static async Task AsyncSendMessage(string text)
        {
            MailAddress from = new MailAddress("rassylocnyjbot@gmail.com", "Library");
            MailAddress to = new MailAddress(UserBase.CurrentUser.Email);
            MailMessage m = new MailMessage(from, to);
            m.Subject = "Notification";
            m.Body = text ;
           // m.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.Credentials = new NetworkCredential(" rassylocnyjbot@gmail.com", "12365478");
            smtp.EnableSsl = true;
            await smtp.SendMailAsync(m);
        }

    }
}
