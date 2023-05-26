using PagedList;
using Pagoda.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pagoda.Controllers
{
    public class RoleController : Controller
    {
        [CheckLogin]
        [CheckPermission]
        // GET: Role
        public ViewResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            ViewBag.CurrentSort = sortOrder;
            ViewBag.RoleName = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var roles = from s in context.Roles select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                roles = roles.Where(s => s.RoleName.ToLower().Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    roles = roles.OrderByDescending(s => s.RoleName.Substring(s.RoleName.LastIndexOf(" ") + 1));
                    break;
                default:  // Name ascending 
                    roles = roles.OrderBy(s => s.RoleName.Substring(s.RoleName.LastIndexOf(" ") + 1));
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(roles.ToPagedList(pageNumber, pageSize));
        }


        [HttpGet]
        public PartialViewResult Create()   //Insert PartialView  
        {
            return PartialView(new Pagoda.Models.Role());
        }


        [HttpPost]
        public JsonResult Create(Role role) // Record Insert  
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);
            context.Roles.InsertOnSubmit(role);
            context.SubmitChanges();
            TempData["Message"] = "Dữ liệu của bạn đã được tạo thành công!!";

            return Json(role, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Delete(string RoleID)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);
            Role role = context.Roles.FirstOrDefault(x => x.RoleID.ToString() == RoleID);

            context.Roles.DeleteOnSubmit(role);

            context.SubmitChanges();

            TempData["Message"] = "Dữ liệu của bạn đã được xóa thành công!!";

            return RedirectToAction("Index");
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
                    var rolesToDelete = context.Roles.Where(x => items.Contains(x.RoleID)).ToList();
                    context.Roles.DeleteAllOnSubmit(rolesToDelete);
                    TempData["Message"] = "Tất Cả dữ liệu của bạn đã được xóa thành công!!";
                    context.SubmitChanges();
                }
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }


        [HttpGet]
        public PartialViewResult Edit(Int32 RoleID)  // Update PartialView  
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);
            Role role = context.Roles.Where(x => x.RoleID == RoleID).FirstOrDefault();
            Role roles = new Role();

            roles.RoleID = role.RoleID;
            roles.RoleName = role.RoleName;
            roles.Description = role.Description;

            return PartialView(roles);
        }


        [HttpPost]
        public JsonResult Edit(Role role)  // Record Update 
        {

            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);
            Role rl = context.Roles.Where(x => x.RoleID == role.RoleID).FirstOrDefault();

            rl.RoleID = role.RoleID;
            rl.RoleName = role.RoleName;
            rl.Description = role.Description;
            context.SubmitChanges();
            TempData["Message"] = "Dữ liệu của bạn đã được cập nhật thành công!!";
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}