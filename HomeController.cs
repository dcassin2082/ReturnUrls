using EmployeePortal.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EmployeePortal.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Employee Portal";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contact Page";

            return View();
        }

        public ActionResult Tos()
        {
            ViewBag.Message = "Terms of Service";

            return View();
        }

        public ActionResult GetTimecards()
        {
            ViewBag.Title = "Timecards";
            return View();
        }

        public ActionResult UploadTooBig()
        {
            return View(new UploadTooBigViewModel());
        }
        
    }
}