using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EmployeePortal.Models;
using System.IO;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using EmployeePortal.Services;

namespace EmployeePortal.Controllers
{
    public class FormsController : Controller
    {
        //private ApplicationDbContext db = new ApplicationDbContext();

        private FormUploadService formUploadService = new FormUploadService();
        private UserService userService = new UserService();

        private static Guid UserId { get; set; }

        // GET: Forms
        public ActionResult Index()
        {
            UserId = GetUserId();
            //Applicant applicant = db.Applicants.Where(a => a.RegisterId == UserId).FirstOrDefault();
            Applicant applicant = formUploadService.GetApplicant(UserId);

            if (applicant == null)
            {
                return RedirectToAction("Index", "Applicant");
            }
            //return View(db.Forms.Where(a => a.ApplicantId == applicant.Id).ToList());
            return View(formUploadService.GetForms(applicant.Id));
        }

        private Guid GetUserId()
        {
            ApplicationUser user = new ApplicationUser();
            //UserStore<ApplicationUser> uStore = new UserStore<ApplicationUser>(db);
            //UserManager<ApplicationUser> uManager = new UserManager<ApplicationUser>(uStore);
            UserManager<ApplicationUser> uManager = userService.GetUserManager();
            user = uManager.FindById(User.Identity.GetUserId());
            Guid userId = new Guid(User.Identity.GetUserId());
            return userId;
        }

        public ActionResult DisplayForm(int formUploadId, string filename)
        {
            //var formId = db.FormUploads.Where(u => u.Id == formUploadId).Select(f => f.FormId).FirstOrDefault();
            Guid formId = formUploadService.GetById(formUploadId).FormId;
            var path = Server.MapPath(string.Format(@"~/UploadedForms/{0}{1}", formId, filename));
            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            // need to determine the file extension, .docx, .pdf, etc the 
            string extension = Path.GetExtension(filename);
            if (!string.IsNullOrWhiteSpace(extension))
            {
                FileStreamResult fsResult = new FileStreamResult(fileStream, "pdf");
                switch (extension)
                {
                    case ".pdf":
                        fsResult = new FileStreamResult(fileStream, "application/pdf");
                        break;
                    case ".doc":
                    case ".docx":
                        fsResult = new FileStreamResult(fileStream, "application/docx");
                        break;
                    case ".txt":
                        fsResult = new FileStreamResult(fileStream, "application/txt");
                        break;
                }
                Response.AddHeader("Content-Disposition", "inline;" + filename);
                return fsResult;
            }
            return View();  // document type not supported
        }

        // GET: Forms/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Form form = db.Forms.Find(id);
            Form form = formUploadService.GetFormById((int)id);
            if (form == null)
            {
                return HttpNotFound();
            }
            return View(form);
        }

        // GET: Forms/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Forms/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ApplicantId,OfficeID,Active,Default,Signature,SignatureType,Name,FilePath,Comment,Timestamp")] Form form)
        {
            if (ModelState.IsValid)
            {
                //db.Forms.Add(form);
                //db.SaveChanges();

                formUploadService.AddForm(form);
                return RedirectToAction("Index");
            }

            return View(form);
        }

        // GET: Forms/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Form form = db.Forms.Find(id);
            Form form = formUploadService.GetFormById((int)id);
            if (form == null)
            {
                return HttpNotFound();
            }
            return View(form);
        }

        // POST: Forms/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ApplicantId,OfficeID,Active,Default,Signature,SignatureType,Name,FilePath,Comment,Timestamp")] Form form)
        {
            if (ModelState.IsValid)
            {
                //db.Entry(form).State = EntityState.Modified;
                //db.SaveChanges();
                formUploadService.UpdateForm(form);
                return RedirectToAction("Index");
            }
            return View(form);
        }

        // GET: Forms/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Form form = db.Forms.Find(id);
            Form form = formUploadService.GetFormById((int)id);
            if (form == null)
            {
                return HttpNotFound();
            }
            return View(form);
        }

        // POST: Forms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            //Form form = db.Forms.Find(id);
            //db.Forms.Remove(form);
            //db.SaveChanges();

            Form form = formUploadService.GetFormById(id);
            
            formUploadService.DeleteForm(id, form.FormUploadId);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                formUploadService.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
