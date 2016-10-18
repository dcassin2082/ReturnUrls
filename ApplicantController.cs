using EmployeePortal.common;
using EmployeePortal.Models;
using EmployeePortal.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace EmployeePortal.Controllers
{
    public class ApplicantController : Controller
    {
        private ApplicantService applicantService = new ApplicantService();
        private UserService userService = new UserService();
        private FormUploadService formUploadService = new FormUploadService();

        private static Guid UserId { get; set; }
        private static string _filename;
        private static string SelectedStateAbbreviation { get; set; }

        [Authorize]
        public ActionResult Index(string fFile, string returnUrl)
        {

            ApplicationUser user = GetUser();
            if (user == null)
                return RedirectToAction("Login", "Account");
            _filename = fFile;
            Guid userId = GetUserId();
            Applicant applicant = applicantService.GetApplicant(userId);

            string emailAddress = GetUserEmail();
            if (applicant == null)
            {
                //applicant = appService.CreateApplicant(userId, emailAddress);
                applicant = new Applicant
                {
                    RegisterId = userId,
                    InternalDatabaseID = Guid.NewGuid(),
                    Email = emailAddress
                };
            }
            else
            {
                applicant.Email = emailAddress;
            }

            if (fFile != null)
            {
                string daxUrl = WebConfigurationManager.AppSettings["daxtraurl"];
                string daxAccountName = WebConfigurationManager.AppSettings["daxtrausername"];
                string path = Path.Combine(Server.MapPath("~/resumes"), fFile);
                string hrxml_profile = ApplicantService.ParseResume(applicant, path);
                
            }
            if (returnUrl == null)
            {
                if (Session["url"] != null)
                {
                    if (applicant.Id != 0)
                    {
                        returnUrl = Session["url"].ToString();
                        return Redirect(returnUrl);
                    }
                }
            }
            IEnumerable<SelectListItem> states = AppHelper.GetStateList();
            var db = new ApplicationDbContext();
            StateLookup state;
            if (!string.IsNullOrWhiteSpace(applicant.State))
            {
                state = db.StateLookups.Where(a => a.Abbreviation == applicant.State).FirstOrDefault();
                ViewBag.StateList = new SelectList(states, "Value", "Text", state.Id);
                foreach (var item in states)
                {
                    if (item.Text == state.State)
                        item.Selected = true;
                }
            }
            else
            {
                ViewBag.StateList = new SelectList(states, "Value", "Text", null);
            }
            return View(applicant);
        }

        public void PopulateStatesDropdown(Applicant applicant)
        {
            IEnumerable<SelectListItem> states = AppHelper.GetStateList();
            var db = new ApplicationDbContext();
            var state = db.StateLookups.Where(a => a.Abbreviation == applicant.State);
        }
        #region Move this stuff to UserService
        private ApplicationUser GetUser()
        {
            ApplicationUser user = new ApplicationUser();
            UserManager<ApplicationUser> uManager = userService.GetUserManager();
            user = uManager.FindById(User.Identity.GetUserId());
            return user;
        }

        private Guid GetUserId()
        {
            ApplicationUser user = new ApplicationUser();
            UserManager<ApplicationUser> uManager = userService.GetUserManager();
            user = uManager.FindById(User.Identity.GetUserId());
            Guid userId = new Guid(User.Identity.GetUserId());
            return userId;
        }

        private string GetUserEmail()
        {
            ApplicationUser user = new ApplicationUser();
            UserManager<ApplicationUser> uManager = userService.GetUserManager();
            user = uManager.FindById(User.Identity.GetUserId());
            return user == null ? "" : user.Email;
        }
        #endregion

        [HttpPost]
        public ActionResult UploadResume(/*FormCollection formCollection*/)
        {
            string fileName = "";
            byte[] fileBytes = new byte[0];
            if (Request != null)
            {
                HttpPostedFileBase file = Request.Files["UploadedFile"];

                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    fileName = Guid.NewGuid().ToString() + file.FileName;
                    string fileContentType = file.ContentType;
                    var path = Path.Combine(Server.MapPath("~/resumes"), fileName);
                    file.SaveAs(path);
                    fileBytes = new byte[file.ContentLength];
                    file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                }
            }
            return RedirectToAction("Index", "Applicant", new { fFile = fileName });
        }

        [HttpPost]
        public ActionResult UploadForm(/*FormCollection formCollection*/)
        {
            //var applicant = db.Applicants.Where(a => a.RegisterId == UserId)
            //            .FirstOrDefault();
            UserId = GetUserId();
            var applicant = formUploadService.GetApplicant(UserId);
            string fileName = string.Empty;
            byte[] fileBytes = new byte[0];

            if (Request != null)
            {
                HttpPostedFileBase file = Request.Files["UploadedFile"];
                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {

                    Guid formId = Guid.NewGuid();
                    fileName = formId.ToString() + file.FileName;
                    string fileContentType = file.ContentType;
                    var path = Path.Combine(Server.MapPath("~/uploadedforms"), fileName);
                    file.SaveAs(path);
                    fileBytes = new byte[file.ContentLength];
                    file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                    //var formType = db.FormTypes.Where(i => i.Id == (int)formTypeId).Select(t => t.Description).FirstOrDefault();
                    string formType = "Resume";
                    formUploadService.UploadForm(1, applicant, file, formId, path, formType);
                }
            }
            return RedirectToAction("Index", "Applicant", new { fFile = fileName });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveRecord(Applicant applicant)
        {
            if (!string.IsNullOrWhiteSpace(SelectedStateAbbreviation))
                applicant.State = SelectedStateAbbreviation;
            var errors = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => new { x.Key, x.Value.Errors }).ToArray();
            if (applicant.Id != 0)
                applicantService.UpdateApplicant(applicant);
            else
                applicantService.SaveApplicant(applicant);
            if (!string.IsNullOrWhiteSpace(_filename))
                applicantService.PostResume(_filename, applicant);
            return RedirectToAction("Index", "Applicant");
        }

        public void SetSelectedState(int selectedState)
        {
            var db = new ApplicationDbContext();
            SelectedStateAbbreviation = db.StateLookups.Where(i => i.Id == selectedState).Select(a => a.Abbreviation).FirstOrDefault();
        }
    }
}