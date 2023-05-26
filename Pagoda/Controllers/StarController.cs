using Pagoda.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using PagedList;
using System.Configuration;

namespace Pagoda.Controllers
{
    public class StarController : Controller
    {
        [CheckLogin]

        public ViewResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            ViewBag.CurrentSort = sortOrder;
            ViewBag.StarName = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.StarAge = sortOrder == "Age" ? "age_desc" : "Age";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var stars = from s in context.Stars select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                stars = stars.Where(s => s.StarName.ToLower().Contains(searchString)
                                       || s.Age.ToString().Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    stars = stars.OrderByDescending(s => s.StarName.Substring(s.StarName.LastIndexOf(" ") + 1));
                    break;
                case "Age":
                    stars = stars.OrderBy(s => s.Age);
                    break;
                case "age_desc":
                    stars = stars.OrderByDescending(s => s.Age);
                    break;
                default:  // Name ascending 
                    stars = stars.OrderBy(s => s.StarName.Substring(s.StarName.LastIndexOf(" ") + 1));
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(stars.ToPagedList(pageNumber, pageSize));
        }

        [CheckLogin]
        [HttpGet]
        public PartialViewResult Create()   //Insert PartialView  
        {
            return PartialView(new Pagoda.Models.Star());
        }

        [CheckLogin]
        [HttpPost]
        public JsonResult Create(Star st) // Record Insert  
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);
            context.Stars.InsertOnSubmit(st);
            context.SubmitChanges();
            TempData["Message"] = "Dữ liệu của bạn đã được tạo thành công!!";

            return Json(st, JsonRequestBehavior.AllowGet);
        }

        [CheckLogin]
        [CheckPermission]
        public ActionResult Delete(string StarID)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);
            Star star = context.Stars.FirstOrDefault(x => x.StarID.ToString() == StarID);

            context.Stars.DeleteOnSubmit(star);

            context.SubmitChanges();

            TempData["Message"] = "Dữ liệu của bạn đã được xóa thành công!!";

            return RedirectToAction("Index");
        }

        [CheckLogin]
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
                    var starToDelete = context.Stars.Where(x => items.Contains(x.StarID)).ToList();
                    context.Stars.DeleteAllOnSubmit(starToDelete);
                    TempData["Message"] = "Tất Cả dữ liệu của bạn đã được xóa thành công!!";
                    context.SubmitChanges();
                }
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [CheckLogin]
        [HttpGet]
        public PartialViewResult Edit(Int32 StarID)  // Update PartialView  
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);
            Star star = context.Stars.Where(x => x.StarID == StarID).FirstOrDefault();
            Star stars = new Star();

            stars.StarID = star.StarID;
            stars.StarName = star.StarName;
            stars.Age = star.Age;
            stars.Sex = star.Sex;

            return PartialView(stars);
        }

        [CheckLogin]
        [HttpPost]
        public JsonResult Edit(Star star)  // Record Update 
        {

            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);
            Star st = context.Stars.Where(x => x.StarID == star.StarID).FirstOrDefault();

            st.StarName = star.StarName;
            st.Age = star.Age;
            st.Sex = star.Sex;
            context.SubmitChanges();
            TempData["Message"] = "Dữ liệu của bạn đã được cập nhật thành công!!";
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}