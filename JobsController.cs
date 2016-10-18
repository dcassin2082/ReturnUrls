using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EmployeePortal.Models;
using DevExpress.Web.Mvc;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using EmployeePortal.common;
using EmployeePortal.Services;

namespace EmployeePortal.Controllers
{
    public class JobsController : Controller
    {
        //private ApplicationDbContext db = new ApplicationDbContext();
        private JobService jobService = new JobService();
        private UserService userService = new UserService();

        // GET: Jobs
        public ActionResult Index()
        {
            //List<Job> jobs = new List<Job>();
            //ApplicationUser user = GetUser();
            //if (user != null)
            //{
            //    if (User.IsInRole("Admin") || User.IsInRole("Recruiter"))
            //    {
            //        jobs = jobService.GetJobs().ToList();
            //    }
            //}
            //else
            //{
            //    jobs = jobService.GetActiveOpenJobList().ToList();
            //}

            return View();
        }

        public ActionResult ApplyForJob(int? id)
        {
            ApplicationUser user = GetUser();
            if (user == null)
            {
                TempData["CurrentUrl"] = Request.Url.ToString();
                Session["url"] = TempData["CurrentUrl"].ToString();
                return RedirectToAction("Login", "Account");
            }
            Guid userId = new Guid(User.Identity.GetUserId());
            var applicant = jobService.GetApplicant(userId);
            if (applicant == null)
            {
                TempData["CurrentUrl"] = Request.Url.ToString();
                Session["url"] = TempData["CurrentUrl"].ToString();
                return RedirectToAction("Index", "Applicant", new { returnUrl = Request.Url.ToString() });
            }
            else
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Job job = jobService.GetById((int)id);
                if (job == null)
                {
                    return HttpNotFound();
                }
                if (job.Active == false)
                    return RedirectToAction("JobNoLongerActive");

                if (job.ClosedDate < DateTime.Now)
                    return RedirectToAction("JobClosed");

                if (job.Active && job.ClosedDate > DateTime.Now)
                {
                    List<ApplicantJobs> jobs = jobService.GetApplicantJobs(applicant.Id).ToList();
                    foreach (var item in jobs)
                    {
                        if (item.JobId == id)
                        {
                            return RedirectToAction("JobAlreadyAppliedTo");
                        }
                    }
                    ApplicantJobs appJob = new ApplicantJobs();
                    appJob.ApplicantId = applicant.Id;
                    appJob.JobId = (int)id;
                    Session["url"] = null;
                    return RedirectToAction("Apply", "Jobs", job);
                }
                return RedirectToAction("Index");
            }
        }

        private ApplicationUser GetUser()
        {
            //ApplicationDbContext db = new ApplicationDbContext();
            ApplicationUser user = new ApplicationUser();
            //UserStore<ApplicationUser> uStore = new UserStore<ApplicationUser>(db);
            //UserManager<ApplicationUser> uManager = new UserManager<ApplicationUser>(uStore);
            UserManager<ApplicationUser> uManager = userService.GetUserManager();
            user = uManager.FindById(User.Identity.GetUserId());
            return user;
        }

        [HttpPost]
        public ActionResult SubmitJobApplication(Job job)
        {
            Guid userId = new Guid(User.Identity.GetUserId());

            //Checking to see if the user is already in the database
            //Applicant applicant = db.Applicants.Where(a => a.RegisterId == userId).FirstOrDefault();
            Applicant applicant = jobService.GetApplicant(userId);
            ApplicantJobs appJob = jobService.SubmitJobApplication(job, applicant);
            jobService.SendConfirmationEmail(userId, applicant, appJob);
            Session["url"] = null;
            // send them to a confirmation page then redirect them to the index page
            return View("JobAppliedToConfirmation", job);
        }

        public ActionResult Apply(Job job)
        {
            return View(job);
        }

        public ActionResult JobAlreadyAppliedTo()
        {
            return View();
        }

        public ActionResult JobClosed()
        {
            return View();
        }

        public ActionResult JobNoLongerActive()
        {
            return View();
        }
        // GET: Jobs/Details/5
        public ActionResult Details(int? id)
        {
            //TempData["CurrentUrl"] = Request.Url.ToString();
            //Session["url"] = TempData["CurrentUrl"].ToString();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Job job = db.Jobs.Find(id);
            Job job = jobService.GetById((int)id);
            if (job == null)
            {
                return HttpNotFound();
            }
            return View(job);
        }

        // GET: Jobs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Jobs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Active,PostedDate,ClosedDate,Title,Description,CityStatePostalCode,JobType,Salary,SalaryType,Experience,Licenses,EducationLevel,Languages,EmailNotify,SMSNotify")] Job job)
        {
            if (ModelState.IsValid)
            {
                //db.Jobs.Add(job);
                //db.SaveChanges();
                jobService.AddJob(job);
                return RedirectToAction("Index");
            }

            return View(job);
        }

        // GET: Jobs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Job job = db.Jobs.Find(id);
            Job job = jobService.GetById((int)id);
            if (job == null)
            {
                return HttpNotFound();
            }
            return View(job);
        }

        // POST: Jobs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Active,PostedDate,ClosedDate,Title,Description,CityStatePostalCode,JobType,Salary,SalaryType,Experience,Licenses,EducationLevel,Languages,EmailNotify,SMSNotify")] Job job)
        {
            if (ModelState.IsValid)
            {
                //db.Entry(job).State = EntityState.Modified;
                //db.SaveChanges();
                jobService.Update(job);
                return RedirectToAction("Index");
            }
            return View(job);
        }

        // GET: Jobs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Job job = db.Jobs.Find(id);
            Job job = jobService.GetById((int)id);
            if (job == null)
            {
                return HttpNotFound();
            }
            return View(job);
        }

        // POST: Jobs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            //Job job = db.Jobs.Find(id);
            //db.Jobs.Remove(job);
            //db.SaveChanges();
            jobService.DeleteJob(id);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                jobService.Dispose();
            }
            base.Dispose(disposing);
        }

        #region partial
        ApplicationDbContext db1 = new ApplicationDbContext();
        [ValidateInput(false)]
        public ActionResult GridViewPartial()
        {
            ApplicationUser user = GetUser();
            List<Job> jobs = new List<Job>();
            if (user != null)
            {
                if (User.IsInRole("Admin") || User.IsInRole("Recruiter"))
                {
                    jobs = jobService.GetJobs().ToList();
                    return PartialView("_GridViewPartial", jobs);
                }
            }
            return PartialView("_GridViewPartial", jobService.GetActiveOpenJobList());
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult GridViewPartialAddNew([ModelBinder(typeof(DevExpressEditorsBinder))] EmployeePortal.Models.Job item)
        {
            var model = db1.Jobs;
            if (ModelState.IsValid)
            {
                try
                {
                    model.Add(item);
                    db1.SaveChanges();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
                ViewData["EditError"] = "Please, correct all errors.";
            return PartialView("_GridViewPartial", model.ToList());
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult GridViewPartialUpdate([ModelBinder(typeof(DevExpressEditorsBinder))] EmployeePortal.Models.Job item)
        {
            //var model = db1.Jobs;
            var model = jobService.GetJobs().ToList();
            if (ModelState.IsValid)
            {
                try
                {
                    var modelItem = model.FirstOrDefault(it => it.Id == item.Id);
                    if (modelItem != null)
                    {
                        this.UpdateModel(modelItem);
                        db1.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
                ViewData["EditError"] = "Please, correct all errors.";
            return PartialView("_GridViewPartial", model.ToList());
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult GridViewPartialDelete(System.Int32 Id)
        {
            var model = db1.Jobs;
            if (Id >= 0)
            {
                try
                {
                    var item = model.FirstOrDefault(it => it.Id == Id);
                    if (item != null)
                        model.Remove(item);
                    db1.SaveChanges();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            return PartialView("_GridViewPartial", model.ToList());
        }
        #endregion
    }
}
