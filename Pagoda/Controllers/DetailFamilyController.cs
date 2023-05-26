using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.Excel;
using PagedList;
using Pagoda.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Linq;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.Xml;

namespace Pagoda.Controllers
{
    public class DetailFamilyController : Controller
    {
        // GET: DetailFamily
        [CheckLogin]

        public ActionResult Index(string keywords, Family family, int? page, string sortOrder, string searchString, string currentFilter)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            IEnumerable<DetailFamily> dsDetailFamily = context.DetailFamilies.Where(x => x.FamilyID == family.FamilyID).ToList();

            TempData["FamilyID"] = family.FamilyID.ToString();
            TempData["FamilyName"] = family.FamilyName;
            TempData["FamilyAddress"] = family.Address;

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
                dsDetailFamily = dsDetailFamily.Where(s => s.FullName.ToLower().Contains(searchString.ToLower()));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    dsDetailFamily = dsDetailFamily.OrderByDescending(s => s.FullName.Substring(s.FullName.LastIndexOf(" ") + 1));
                    break;
                case "Age":
                    dsDetailFamily = dsDetailFamily.OrderBy(s => s.Age);
                    break;
                case "age_desc":
                    dsDetailFamily = dsDetailFamily.OrderByDescending(s => s.Age);
                    break;
                default:  // Name ascending 
                    dsDetailFamily = dsDetailFamily.OrderBy(s => s.FullName.Substring(s.FullName.LastIndexOf(" ") + 1));
                    break;
            }

            int pageSize = 999999;
            int pageNumber = (page ?? 1);
            return View(dsDetailFamily.ToPagedList(pageNumber, pageSize));
        }


        [HttpGet]
        public PartialViewResult Create()   //Insert PartialView  
        {
            return PartialView(new Pagoda.Models.DetailFamily());
        }


        [HttpPost]
        public JsonResult Create(DetailFamily detailFamily, Family family, string fullFamilyName) // Record Insert  
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            var star = context.Stars.FirstOrDefault(x => x.Age == detailFamily.Age && x.Sex == detailFamily.Sex);
            if (star == null)
            {
                detailFamily.Star = star ?? null;
            }
            else
            {
                detailFamily.Star = star;
            }

            var threat = context.Threats.FirstOrDefault(x => x.Age == detailFamily.Age && x.Sex == detailFamily.Sex);
            if (threat == null)
            {
                detailFamily.Threat = threat ?? null;
            }
            else
            {
                detailFamily.Threat = threat;
            }

            fullFamilyName = family.FamilyID.ToString();

            ViewBag.fullFamilyName = fullFamilyName;

            //detailFamily.YearUser = (DateTime.Now.Year - detailFamily.Age) + 1;

            context.DetailFamilies.InsertOnSubmit(detailFamily);
            context.SubmitChanges();

            TempData["Message"] = "Dữ liệu của bạn đã được tạo thành công!!";

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Delete(string DetailFamilyID)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);
            DetailFamily detailFamily = context.DetailFamilies.FirstOrDefault(x => x.DetailFamilyID.ToString() == DetailFamilyID);

            context.DetailFamilies.DeleteOnSubmit(detailFamily);
            context.SubmitChanges();
            TempData["Message"] = "Dữ liệu của bạn đã được xóa thành công!!";

            Family family = new Family();

            return RedirectToAction("Index", new { FamilyID = detailFamily.FamilyID.ToString() });
        }


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
                    var DetailFamilyToDelete = context.DetailFamilies.Where(x => items.Contains(x.DetailFamilyID)).ToList();
                    context.DetailFamilies.DeleteAllOnSubmit(DetailFamilyToDelete);
                    TempData["Message"] = "Tất Cả dữ liệu của bạn đã được xóa thành công!!";
                    context.SubmitChanges();
                }
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }


        [HttpGet]
        public PartialViewResult Edit(Int32 DetailFamilyID)  // Update PartialView  
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);
            DetailFamily detailFamily = context.DetailFamilies.Where(x => x.DetailFamilyID == DetailFamilyID).FirstOrDefault();
            DetailFamily detailFamilys = new DetailFamily();

            detailFamilys.DetailFamilyID = detailFamily.DetailFamilyID;
            detailFamilys.FullName = detailFamily.FullName;
            detailFamilys.Age = detailFamily.Age;
            detailFamilys.Sex = detailFamily.Sex;

            return PartialView(detailFamilys);
        }


        [HttpPost]
        public JsonResult Edit(DetailFamily detailFamily, Family family, string fullFamilyName)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);
            DetailFamily df = context.DetailFamilies.Where(x => x.DetailFamilyID == detailFamily.DetailFamilyID).FirstOrDefault();

            fullFamilyName = family.FamilyID.ToString();

            ViewBag.fullFamilyName = fullFamilyName;

            df.FullName = detailFamily.FullName;
            df.Age = detailFamily.Age;
            df.Sex = detailFamily.Sex;

            var star = context.Stars.FirstOrDefault(x => x.Age == detailFamily.Age && x.Sex == detailFamily.Sex);
            if (star == null)
            {
                df.Star = star ?? null;
            }
            else
            {
                df.Star = star;
            }

            var threat = context.Threats.FirstOrDefault(x => x.Age == detailFamily.Age && x.Sex == detailFamily.Sex);
            if (threat == null)
            {
                df.Threat = threat ?? null;
            }
            else
            {
                df.Threat = threat;
            }

            context.SubmitChanges();
            TempData["Message"] = "Dữ liệu của bạn đã được cập nhật thành công!!";
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Details(string DetailFamilyID, string fullname, Family family)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);
            DetailFamily detailFamily = context.DetailFamilies.FirstOrDefault(x => x.DetailFamilyID.ToString() == DetailFamilyID);
            fullname = family.FamilyID.ToString();
            ViewBag.fullname = fullname;
            return View(detailFamily);
        }

        public ActionResult List(int? familyID, Family family, string sortOrder)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            IEnumerable<DetailFamily> dsDetailFamily = context.DetailFamilies.Where(x => x.FamilyID == familyID).ToList();

            if (familyID == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.FamilyName = context.Families.FirstOrDefault(x => x.FamilyID == familyID)?.FamilyName;

            ViewBag.Address = context.Families.FirstOrDefault(x => x.FamilyID == familyID)?.Address;

            switch (sortOrder)
            {
                default:  // Name ascending 
                    dsDetailFamily = dsDetailFamily.OrderBy(s => s.FullName.Substring(s.FullName.LastIndexOf(" ") + 1));
                    break;
            }

            ViewBag.fullname = familyID;

            return View(dsDetailFamily);
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

