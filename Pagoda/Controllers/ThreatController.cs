using BoldReports.Linq;
using PagedList;
using Pagoda.Models;
using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace Pagoda.Controllers
{
    [CheckLogin]
    public class ThreatController : Controller
    {
        public ViewResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
        string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);


            ViewBag.CurrentSort = sortOrder;
            ViewBag.ThreatName = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.ThreatAge = sortOrder == "Age" ? "age_desc" : "Age";

            searchString = Request.QueryString["searchString"];

            var threats = from s in context.Threats select s;

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                threats = threats.Where(s => s.ThreatName.ToLower().Contains(searchString)
                                       || s.Age.ToString().Contains(searchString));
            }

            ViewBag.CurrentFilter = searchString;

            switch (sortOrder)
            {
                case "name_desc":
                    threats = threats.OrderByDescending(s => s.ThreatName.Substring(s.ThreatName.LastIndexOf(" ") + 1));
                    break;
                case "Age":
                    threats = threats.OrderBy(s => s.Age);
                    break;
                case "age_desc":
                    threats = threats.OrderByDescending(s => s.Age);
                    break;
                default:  // Name ascending 
                    threats = threats.OrderBy(s => s.ThreatName.Substring(s.ThreatName.LastIndexOf(" ") + 1));
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(threats.ToPagedList(pageNumber, pageSize));
        }

        [HttpGet]
        public PartialViewResult Create()   //Insert PartialView  
        {
            return PartialView(new Pagoda.Models.Threat());
        }

        [HttpPost]
        public JsonResult Create(Threat threat) // Record Insert  
        {
        string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            context.Threats.InsertOnSubmit(threat);
            context.SubmitChanges();
            TempData["Message"] = "Dữ liệu của bạn đã được tạo thành công!!";

            return Json(threat, JsonRequestBehavior.AllowGet);
        }

        [CheckPermission]
        public ActionResult Delete(string ThreatID)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            Threat threat = context.Threats.FirstOrDefault(x => x.ThreatID.ToString() == ThreatID);

            context.Threats.DeleteOnSubmit(threat);
            context.SubmitChanges();

            TempData["Message"] = "Dữ liệu của bạn đã được xóa thành công!!";

            return RedirectToAction("Index");
        }

        [CheckPermission]
        [HttpPost]
        public ActionResult DeleteAll(string ids)
        {
        string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            if (!string.IsNullOrEmpty(ids))
            {
                var items = ids.Split(',').Select(x => Convert.ToInt32(x)).ToList();
                if (items != null && items.Any())
                {
                    var threatToDelete = context.Threats.Where(x => items.Contains(x.ThreatID)).ToList();
                    context.Threats.DeleteAllOnSubmit(threatToDelete);
                    TempData["Message"] = "Tất Cả dữ liệu của bạn đã được xóa thành công!!";
                    context.SubmitChanges();
                }
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }


        [HttpGet]
        public PartialViewResult Edit(Int32 ThreatID)  // Update PartialView  
        {
        string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            Threat threat = context.Threats.Where(x => x.ThreatID == ThreatID).FirstOrDefault();
            Threat threats = new Threat();

            threats.ThreatID = threat.ThreatID;
            threats.ThreatName = threat.ThreatName;
            threats.Age = threat.Age;
            threats.Sex = threat.Sex;

            return PartialView(threats);
        }

        [HttpPost]
        public JsonResult Edit(Threat threat)  // Record Update 
        {
        string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
                        PagodaDataContext context = new PagodaDataContext(connectionString);

            Threat threa1 = context.Threats.Where(x => x.ThreatID == threat.ThreatID).FirstOrDefault();

            threa1.ThreatName = threat.ThreatName;
            threa1.Age = threat.Age;
            threa1.Sex = threat.Sex;
            context.SubmitChanges();
            TempData["Message"] = "Dữ liệu của bạn đã được cập nhật thành công!!";
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}