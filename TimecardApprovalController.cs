using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EmployeePortal.Models;
using EmployeePortal.ViewModels;
using PagedList;
using EmployeePortal.Services;
using Microsoft.AspNet.Identity;

namespace EmployeePortal.Controllers
{
    public class TimecardApprovalController : Controller
    {
        private TimecardService timecardService = new TimecardService();
        //private SendGridEmailService emailService = new SendGridEmailService();
        private static int _page;

        private int SelectedPageSize { get; set; }

        #region ProcessTimecards
        public ActionResult ProcessSelectedTimecards(int[] selectedTimecardIds, string submit)
        {
            List<Timecard> timecards = new List<Timecard>();
            if (selectedTimecardIds != null)
            {
                foreach (int id in selectedTimecardIds)
                {
                    Timecard timecard = timecardService.GetById(id);
                    timecards.Add(timecard);
                }
                TimecardApprovalViewModel vm = new TimecardApprovalViewModel
                {
                    ApprovalStatus = submit,
                    Timecards = timecards,
                };
                return PartialView("_processSelectedTimecards", vm);
            }
            return View("Index");
        }

        // I have the emails & text message stuff commented out for now but it DOES WORK - i just don't want a thousand emails
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessSelectedTimecards(TimecardApprovalViewModel vm)
        {
            Guid userId = new Guid(User.Identity.GetUserId());
            if (vm != null && vm.Timecards != null)
            {
                foreach (var item in vm.Timecards)
                {
                    Timecard timecard = timecardService.GetById(item.Id);
                    Applicant employee = timecardService.GetEmployeeByTimecardId(timecard);
                    if (vm.ApprovalStatus == "Approve")
                    {
                        timecard.ApprovalStatus = "Approved";
                        timecard.Approved = true;
                        timecard.ApprovedDate = DateTime.Now;
                        //EmailTemplate template = SendGridEmailService.GetEmailTemplate("Timecard Approved");
                        string path = string.Empty;
                        //if (template.Active)
                        //{
                        //    if (!string.IsNullOrWhiteSpace(employee.Email))
                        //    {
                        //        //SendGridEmailService.SendEmail(template, employee.Email, userId.ToString(), employee.FirstName, employee.LastName, "", path);
                        //    }
                        //    if (!string.IsNullOrWhiteSpace(employee.CellPhone))
                        //    {
                        //        //SMSService.SendSMS(template, employee.CellPhone, userId.ToString(), null);
                        //    }
                        //}
                    }
                    else if (vm.ApprovalStatus == "Reject")
                    {
                        timecard.ApprovalStatus = "Rejected";
                        timecard.Approved = false;
                        timecard.LastRejectedDate = DateTime.Now;
                        //EmailTemplate template = SendGridEmailService.GetEmailTemplate("Timecard Rejected");
                        string path = string.Empty;
                        //if (template.Active)
                        //{
                        //    if (!string.IsNullOrWhiteSpace(employee.Email))
                        //    {
                        //        //SendGridEmailService.SendEmail(template, employee.Email, userId.ToString(), employee.FirstName, employee.LastName, "", path);
                        //    }
                        //    if (!string.IsNullOrWhiteSpace(employee.CellPhone))
                        //    {
                        //        //SMSService.SendSMS(template, employee.CellPhone, userId.ToString(), null);
                        //    }
                        //}
                    }
                    var errors = ModelState.Values.SelectMany(v => v.Errors);
                    // try / catch please
                    // put this in a try/catch
                    if (ModelState.IsValid)
                    {
                        timecardService.Update(timecard);
                    }
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult ProcessAllTimecards(string submit)
        {
            List<Timecard> timecards = timecardService.GetSubmittedTimecards().ToList();
            if (timecards != null)
            {
                TimecardApprovalViewModel vm = new TimecardApprovalViewModel
                {
                    ApprovalStatus = submit,
                    Timecards = timecards
                };
                return PartialView("_processAllTimecards", vm);
            }
            return View("Index");
        }

        // I have the emails & text message stuff commented out for now but it DOES WORK - i just don't want a thousand emails
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessAllTimecards(TimecardApprovalViewModel vm)
        {
            //var db = new ApplicationDbContext();
            Guid userId = new Guid(User.Identity.GetUserId());
            if (vm != null && vm.Timecards != null)
            {
                foreach (var item in vm.Timecards)
                {
                    //Applicant employee = db.Applicants.Where(i => i.Id == item.EmployeeID).FirstOrDefault();
                    Timecard timecard = timecardService.GetById(item.Id);
                    Applicant employee = timecardService.GetEmployeeByTimecardId(timecard);
                    if (vm.ApprovalStatus == "Approve All")
                    {
                        timecard.ApprovalStatus = "Approved";
                        timecard.Approved = true;
                        timecard.ApprovedDate = DateTime.Now;
                        //EmailTemplate template = SendGridEmailService.GetEmailTemplate("Timecard Approved");
                        string path = string.Empty;
                        //if (template.Active)
                        //{
                        //    if (!string.IsNullOrWhiteSpace(employee.Email))
                        //    {
                        //        //SendGridEmailService.SendEmail(template, employee.Email, userId.ToString(), employee.FirstName, employee.LastName, "", path);
                        //    }
                        //    if (!string.IsNullOrWhiteSpace(employee.CellPhone))
                        //    {
                        //        //SMSService.SendSMS(template, employee.CellPhone, userId.ToString(), null);
                        //    }
                        //}
                    }
                    else if (vm.ApprovalStatus == "Reject All")
                    {
                        timecard.ApprovalStatus = "Rejected";
                        timecard.Approved = false;
                        timecard.LastRejectedDate = DateTime.Now;
                        //EmailTemplate template = SendGridEmailService.GetEmailTemplate("Timecard Rejected");
                        string path = string.Empty;
                        //if (template.Active)
                        //{
                        //    if (!string.IsNullOrWhiteSpace(employee.Email))
                        //    {
                        //        //SendGridEmailService.SendEmail(template, employee.Email, userId.ToString(), employee.FirstName, employee.LastName, "", path);
                        //    }
                        //    if (!string.IsNullOrWhiteSpace(employee.CellPhone))
                        //    {
                        //        //SMSService.SendSMS(template, employee.CellPhone, userId.ToString(), null);
                        //    }
                        //}
                    }
                    var errors = ModelState.Values.SelectMany(v => v.Errors);
            // try / catch please
                // put this in a try/catch
                    if (ModelState.IsValid)
                    {
                        timecardService.Update(timecard);
                    }
                }
            }
            return RedirectToAction("Index");
        }
#endregion

        // GET: TimecardApproval
        public ActionResult Index(int? page, string sortOrder)
        {
            int lastPage = 0; // get the last page you were on --- this is used when you are selecting the checkboxes and approving / rejecting them, they disappear from the view
            List<Timecard> timecards = timecardService.GetSubmittedTimecards().ToList();
            int pageNumber, pagesize;

            AddSortingAndPaging(page, ref sortOrder, ref timecards, out pageNumber, out pagesize);

            // if we decide to let them choose how many record per page to display *************************
            //if (selectedPageSize == 0 || selectedPageSize == null) 
            //    pagesize = 10;
            //else
            //    pagesize = (int)selectedPageSize;
            if (timecards.Count > pagesize)
            {
                lastPage = timecards.Count % pagesize == 0 ? timecards.Count / pagesize : (timecards.Count / pagesize) + 1;
                if (lastPage > 0)
                    if (Request.UrlReferrer != null)
                    {
                        {
                            if (!Request.UrlReferrer.ToString().ToLower().Contains("timecardapproval"))
                            {
                                page = 1;
                            }
                        }

                        if (page == null && pageNumber > lastPage)
                        {
                            pageNumber = lastPage;
                        }
                    }
                ViewBag.Paging = "ShowPaging";
                if (!string.IsNullOrWhiteSpace(sortOrder))
                    return View(timecards.ToPagedList(pageNumber, pagesize));
                else
                    return View(timecards.OrderByDescending(c => c.CreatedDate).ToPagedList(pageNumber == 0 ? 1 : pageNumber, pagesize));
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(sortOrder))
                    return View(timecards.ToList());
                else
                    return View(timecards.OrderByDescending(c => c.CreatedDate).ToList());
            }
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Timecard timecard = timecardService.GetById((int)id);
            TimecardViewModel vm = new TimecardViewModel();
            vm.ApprovalStatus = timecard.ApprovalStatus;
            vm.WeekEnding = timecard.WeekEnding;
            vm.JobCode = timecard.JobCode;
            vm.CustomerCode = timecard.CustomerCode;
            vm.TotalHours = timecard.TotalHours;
            vm.TotalMiles = timecard.TotalMiles;
            vm.EmployeeName = timecard.EmployeeName;
            vm.TimecardId = timecard.Id;
            vm.TotalRegular = timecard.TotalRegular;
            vm.TimecardSubmitted = timecard.TimecardSubmitted;
            vm.EmployeeId = timecard.EmployeeID;
            vm.TimeEntries = timecardService.GetTimecardEntries(timecard.Id).OrderBy(w => w.WorkDate).ToList();
            vm.Adjustments = timecardService.GetTimecardAdjustments(timecard.Id).OrderByDescending(a => a.AdjustmentDate).ToList();
            vm.HasAdjustments = vm.Adjustments.Count > 0 ? true : false;
            for (int i = 0; i < vm.TimeEntries.Count; i++)
            {
                vm.StartMiles = vm.TimeEntries[i].StartMiles;
                vm.EndMiles = vm.TimeEntries[i].EndMiles;
            }
            if (timecard == null)
            {
                return HttpNotFound();
            }
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(TimecardViewModel vm, string submit)
        {
            Guid userId = new Guid(User.Identity.GetUserId());
            var timecard = timecardService.GetById(vm.TimecardId);
            Applicant employee = timecardService.GetEmployeeByTimecardId(timecard);
            if (submit == "Approve")
            {
                //EmailTemplate template = SendGridEmailService.GetEmailTemplate("Timecard Approved");
                string path = string.Empty;
                //if (template.Active)
                //{
                //    if (!string.IsNullOrWhiteSpace(employee.Email))
                //    {
                //        SendGridEmailService.SendEmail(template, employee.Email, userId.ToString(), employee.FirstName, employee.LastName, "", path);
                //    }
                //    if (!string.IsNullOrWhiteSpace(employee.CellPhone))
                //    {
                //        SMSService.SendSMS(template, employee.CellPhone, userId.ToString(), null);
                //    }
                //}
                timecard.ApprovalStatus = "Approved";
                timecard.Approved = true;
                timecard.ApprovedDate = DateTime.Now;
            }
            else if (submit == "Reject")
            {
                timecard.ApprovalStatus = "Rejected";
                timecard.Approved = false;
                timecard.LastRejectedDate = DateTime.Now;
                //EmailTemplate template = SendGridEmailService.GetEmailTemplate("Timecard Rejected");
                string path = string.Empty;
                //if (template.Active)
                //{
                //    if (!string.IsNullOrWhiteSpace(employee.Email))
                //    {
                //        SendGridEmailService.SendEmail(template, employee.Email, userId.ToString(), employee.FirstName, employee.LastName, "", path);
                //    }
                //    if (!string.IsNullOrWhiteSpace(employee.CellPhone))
                //    {
                //        SMSService.SendSMS(template, employee.CellPhone, userId.ToString(), null);
                //    }
                //}
            }
            if (ModelState.IsValid)
            {
                timecardService.Update(timecard);
                return RedirectToAction("Index");
            }
            return View(vm);
        }

        // GET: TimecardApproval/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Timecard timecardDetails = timecardService.GetById((int)id);
            if (timecardDetails == null)
            {
                return HttpNotFound();
            }
            return View(timecardDetails);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                timecardService.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Helper Methods
        // if we decide to allow them to select a page size we will need this ajax call
        public void SetPageSize(int selectedPageSize)
        {
            SelectedPageSize = selectedPageSize;
        }

        public void AddSortingAndPaging(int? page, ref string sortOrder, ref List<Timecard> timecards, out int pageNumber, out int pagesize)
        {
            if (Request.UrlReferrer != null)
            {
                if (!Request.UrlReferrer.ToString().ToLower().Contains("timecardapproval"))
                {
                    page = 1;
                }
            }
            pageNumber = (page ?? 1);
            if (page != null)
            {
                TempData["page"] = page;
                _page = (int)TempData["page"];
            }
            else
            {
                if (TempData["page"] != null)
                {
                    pageNumber = (int)TempData["page"];
                }
                else
                {
                    pageNumber = _page;
                }
            }
            if (sortOrder == null)
                if (Session["sort"] != null)
                    sortOrder = Session["sort"].ToString();
            pagesize = 10;
            ViewBag.WeekEndingSort = "WeekEndingDesc";
            ViewBag.ApprovalStatusSort = "ApprovalStatusDesc";
            ViewBag.CustomerCodeSort = "CustomerCodeDesc";
            ViewBag.JobCodeSort = "JobCodeDesc";
            ViewBag.TotalHoursSort = "TotalHoursDesc";
            ViewBag.TotalMilesSort = "TotalMilesDesc";
            ViewBag.EmployeeNameSort = "EmployeeNameDesc";
            switch (sortOrder)
            {
                case "WeekEndingAsc":
                    timecards = timecards.OrderByDescending(a => a.WeekEnding).ToList();
                    ViewBag.WeekEndingSort = sortOrder;
                    Session["sort"] = sortOrder;
                    break;
                case "WeekEndingDesc":
                    timecards = timecards.OrderBy(a => a.WeekEnding).ToList();
                    ViewBag.WeekEndingSort = sortOrder;
                    Session["sort"] = sortOrder;
                    break;
                case "ApprovalStatusAsc":
                    timecards = timecards.OrderByDescending(a => a.ApprovalStatus).ToList();
                    ViewBag.ApprovalStatusSort = sortOrder;
                    Session["sort"] = sortOrder;
                    break;
                case "ApprovalStatusDesc":
                    timecards = timecards.OrderBy(a => a.ApprovalStatus).ToList();
                    ViewBag.ApprovalStatusSort = sortOrder;
                    Session["sort"] = sortOrder;
                    break;
                case "CustomerCodeAsc":
                    timecards = timecards.OrderByDescending(c => c.CustomerCode).ToList();
                    ViewBag.CustomerCodeSort = sortOrder;
                    Session["sort"] = sortOrder;
                    break;
                case "CustomerCodeDesc":
                    timecards = timecards.OrderBy(c => c.CustomerCode).ToList();
                    ViewBag.CustomerCodeSort = sortOrder;
                    Session["sort"] = sortOrder;
                    break;
                case "JobCodeAsc":
                    timecards = timecards.OrderByDescending(j => j.JobCode).ToList();
                    ViewBag.JobCodeSort = sortOrder;
                    Session["sort"] = sortOrder;
                    break;
                case "JobCodeDesc":
                    timecards = timecards.OrderBy(j => j.JobCode).ToList();
                    ViewBag.JobCodeSort = sortOrder;
                    Session["sort"] = sortOrder;
                    break;
                case "TotalHoursAsc":
                    timecards = timecards.OrderByDescending(th => th.TotalHours).ToList();
                    ViewBag.TotalHoursSort = sortOrder;
                    Session["sort"] = sortOrder;
                    break;
                case "TotalHoursDesc":
                    timecards = timecards.OrderBy(th => th.TotalHours).ToList();
                    ViewBag.TotalHoursSort = sortOrder;
                    Session["sort"] = sortOrder;
                    break;
                case "TotalMilesAsc":
                    timecards = timecards.OrderByDescending(tm => tm.TotalMiles).ToList();
                    ViewBag.TotalMilesSort = sortOrder;
                    Session["sort"] = sortOrder;
                    break;
                case "TotalMilesDesc":
                    timecards = timecards.OrderBy(tm => tm.TotalMiles).ToList();
                    ViewBag.TotalMilesSort = sortOrder;
                    Session["sort"] = sortOrder;
                    break;
                case "EmployeeNameAsc":
                    timecards = timecards.OrderByDescending(tm => tm.EmployeeName).ToList();
                    ViewBag.EmployeeNameSort = sortOrder;
                    Session["sort"] = sortOrder;
                    break;
                case "EmployeeNameDesc":
                    timecards = timecards.OrderBy(tm => tm.EmployeeName).ToList();
                    ViewBag.EmployeeNameSort = sortOrder;
                    Session["sort"] = sortOrder;
                    break;
                default:
                    timecards = timecards.OrderByDescending(w => w.WeekEnding).ToList();
                    ViewBag.WeekEndingSort = sortOrder;
                    Session["sort"] = sortOrder;
                    break;
            }
        }

        #endregion
        #region Unused Code
        // GET: TimecardApproval/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TimecardApproval/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Timecard timecard)
        {
            if (ModelState.IsValid)
            {
                timecardService.AddTimecard(timecard);
                return RedirectToAction("Index");
            }
            return View(timecard);
        }


        // GET: TimecardApproval/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Timecard timecard = timecardService.GetById((int)id);
            if (timecard == null)
            {
                return HttpNotFound();
            }
            return View(timecard);
        }

        // POST: TimecardApproval/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Timecard timecard = timecardService.GetById((int)id);
            timecardService.DeleteTimecard(timecard);
            return RedirectToAction("Index");
        }
        #endregion
    }
}




















































