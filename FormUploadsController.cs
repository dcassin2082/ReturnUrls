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
using System.Text.RegularExpressions;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using EmployeePortal.ViewModels;
using EmployeePortal.Services;
using EmployeePortal.common;

namespace EmployeePortal.Controllers
{
    public class FormUploadsController : Controller
    {
        private FormUploadService formUploadService = new FormUploadService();
        private UserService userService = new UserService();

        private static int FormTypeId { get; set; }

        private static Guid UserId { get; set; }

        // GET: FormUploads
        public ActionResult Index()
        {
            UserId = GetUserId();
            //var applicant = db.Applicants.Where(a => a.RegisterId == UserId)
            //            .FirstOrDefault();
            var applicant = formUploadService.GetApplicant(UserId);
            //GetFormType();
            if (applicant == null)
            {
                return RedirectToAction("Index", "Applicant");
            }
            FormUploadsViewModel vm = new FormUploadsViewModel
            {
                FormUploads = formUploadService.GetFormUploads(applicant.Id).ToList(),
                UploadDate = DateTime.Now
            };
            return View(vm);
            //return View(db.FormUploads.Where(a => a.ApplicantId == applicant.Id).ToList());
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

        private void GetFormType()
        {
            List<SelectListItem> formTypes = new List<SelectListItem>();
            //var types = db.FormTypes.ToList();
            var types = formUploadService.GetFormTypes();
            foreach (var v in types)
            {
                formTypes.Add(new SelectListItem
                {
                    Value = v.Id.ToString(),
                    Text = v.Description
                });
            }
            ViewBag.FormTypeId = new SelectList(types, "Id", "Description", null);
        }

        [HttpPost]
        public ActionResult UploadForm()
        {
            //var applicant = db.Applicants.Where(a => a.RegisterId == UserId)
            //            .FirstOrDefault();
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
                    string formType = formUploadService.GetFormType(FormTypeId);
                    formUploadService.UploadForm(FormTypeId, applicant, file, formId, path, formType);
                }
            }
            return RedirectToAction("Index", "FormUploads");
        }

        public ActionResult DisplayForm(string filename)
        {
            var path = Server.MapPath(@"~/UploadedForms/" + filename);
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                formUploadService.Dispose();
            }
            base.Dispose(disposing);
        }

        // GET: FormUploads/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //FormUpload formUpload = db.FormUploads.Find(id);
            FormUpload formUpload = formUploadService.GetById((int)id);
            if (formUpload == null)
            {
                return HttpNotFound();
            }
            return View(formUpload);
        }

        // POST: FormUploads/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            int formId = formUploadService.GetFormByFormUploadId(id).Id;
            formUploadService.DeleteForm(formId, id);
            return RedirectToAction("Index");
        }

        public void GetFormTypeId(int formTypeId)
        {
            FormTypeId = formTypeId;
        }
    }
}
