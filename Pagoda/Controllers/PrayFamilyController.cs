using PagedList;
using Pagoda.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Configuration;

namespace Pagoda.Controllers
{
    public class PrayFamilyController : Controller
    {
        [CheckLogin]

        // GET: PrayFamily
        public ActionResult Index(string keywords, Family family, int? page, string sortOrder, string searchString, string currentFilter)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            IEnumerable<PrayFamily> dsprayFamilies = context.PrayFamilies.Where(x => x.FamilyID == family.FamilyID).ToList();

            string fullname = family.FamilyID.ToString();

            TempData["FamilyID"] = family.FamilyID.ToString();

            ViewBag.fullname = fullname;
            ViewBag.familyName = family.FamilyName;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.FullNameSort = String.IsNullOrEmpty(sortOrder) ? "fullname_desc" : "";
            ViewBag.AgeSort = sortOrder == "Age" ? "age_desc" : "Age";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            if (!String.IsNullOrEmpty(searchString))
            {
                dsprayFamilies = dsprayFamilies.Where(s => s.FullName.ToLower().Contains(searchString.ToLower()));

            };

            switch (sortOrder)
            {
                default:  // Name ascending 
                    dsprayFamilies = dsprayFamilies.OrderBy(s => s.FullName.Substring(s.FullName.LastIndexOf(" ") + 1));
                    break;
            }

            int pageSize = 99999;
            int pageNumber = (page ?? 1);
            return View(dsprayFamilies.ToPagedList(pageNumber, pageSize));
        }


        [HttpGet]
        public PartialViewResult Create()   //Insert PartialView  
        {
            return PartialView(new Pagoda.Models.PrayFamily());
        }


        [HttpPost]
        public JsonResult Create(PrayFamily prayFamily, Family family, string fullFamilyName) // Record Insert  
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            fullFamilyName = family.FamilyID.ToString();

            prayFamily.AgeLived = prayFamily.DieDate.Value.Year - prayFamily.BornDate.Value.Year;

            ViewBag.fullFamilyName = fullFamilyName;

            context.PrayFamilies.InsertOnSubmit(prayFamily);
            context.SubmitChanges();

            TempData["Message"] = "Dữ liệu của bạn đã được tạo thành công!!";

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [CheckPermission]
        public ActionResult Delete(string prayFamilyID)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);
            PrayFamily prayFamily = context.PrayFamilies.FirstOrDefault(x => x.PrayFamilyID.ToString() == prayFamilyID);

            context.PrayFamilies.DeleteOnSubmit(prayFamily);
            context.SubmitChanges();
            TempData["Message"] = "Dữ liệu của bạn đã được xóa thành công!!";

            Family family = new Family();

            return RedirectToAction("Index", new { FamilyID = prayFamily.FamilyID.ToString() });
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
                    var PrayFamilyToDelete = context.PrayFamilies.Where(x => items.Contains(x.PrayFamilyID)).ToList();
                    context.PrayFamilies.DeleteAllOnSubmit(PrayFamilyToDelete);
                    TempData["Message"] = "Tất Cả dữ liệu của bạn đã được xóa thành công!!";
                    context.SubmitChanges();
                }
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }


        [HttpGet]
        public PartialViewResult Edit(Int32 prayFamilyID)  // Update PartialView  
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);
            PrayFamily prayFamily = context.PrayFamilies.Where(x => x.PrayFamilyID == prayFamilyID).FirstOrDefault();
            PrayFamily prayFamilys = new PrayFamily();

            prayFamilys.PrayFamilyID = prayFamily.PrayFamilyID;
            prayFamilys.FullName = prayFamily.FullName;
            prayFamilys.Sex = prayFamily.Sex;
            prayFamilys.AgeLived = prayFamily.AgeLived;
            prayFamilys.DieDate = prayFamily.DieDate;
            prayFamilys.BornDate = prayFamily.BornDate;
            prayFamilys.WhereDie = prayFamily.WhereDie;
            prayFamilys.WhereBorn = prayFamily.WhereBorn;

            return PartialView(prayFamilys);
        }


        [HttpPost]
        public JsonResult Edit(PrayFamily prayFamily, Family family, string fullFamilyName)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);
            PrayFamily pf = context.PrayFamilies.Where(x => x.PrayFamilyID == prayFamily.PrayFamilyID).FirstOrDefault();

            fullFamilyName = family.FamilyID.ToString();

            ViewBag.fullFamilyName = fullFamilyName;

            pf.FullName = prayFamily.FullName;
            pf.Sex = prayFamily.Sex;
            pf.DieDate = prayFamily.DieDate;
            pf.BornDate = prayFamily.BornDate;
            pf.WhereBorn = prayFamily.WhereBorn;
            pf.WhereDie = prayFamily.WhereDie;

            if (pf.BornDate.HasValue && pf.DieDate.HasValue)
            {
                pf.AgeLived = pf.DieDate.Value.Year - pf.BornDate.Value.Year;
            }
            else
            {
                pf.AgeLived = 0;
            }
            context.SubmitChanges();
            TempData["Message"] = "Dữ liệu của bạn đã được cập nhật thành công!!";
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult List(int? familyID, Family family, string sortOrder)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            IEnumerable<PrayFamily> dsPrayFamilies = context.PrayFamilies.Where(x => x.FamilyID == familyID).ToList();

            if (familyID == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.FamilyName = context.Families.FirstOrDefault(x => x.FamilyID == familyID)?.FamilyName;

            ViewBag.Address = context.Families.FirstOrDefault(x => x.FamilyID == familyID)?.Address;

            switch (sortOrder)
            {
                default:  // Name ascending 
                    dsPrayFamilies = dsPrayFamilies.OrderBy(s => s.FullName.Substring(s.FullName.LastIndexOf(" ") + 1));
                    break;
            }

            ViewBag.fullname = familyID;

            return View(dsPrayFamilies);
        }

        public ActionResult PrintPDF(int? familyID, Family family)
        {
            if (familyID == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var report = new Rotativa.ActionAsPdf("List", new { familyID = familyID, familyName = ViewBag.FamilyName })
            {
                PageMargins = { Left = 20, Bottom = 18, Right = 20, Top = 18 },
                PageSize = Rotativa.Options.Size.A4,
            };
            return report;
        }

    }
}