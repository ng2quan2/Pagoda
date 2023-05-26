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
    public class DonationController : Controller
    {
        [CheckLogin]
        // GET: Donation
        public ViewResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            ViewBag.CurrentSort = sortOrder;
            ViewBag.FullNameSort = String.IsNullOrEmpty(sortOrder) ? "fullname_desc" : "";
            ViewBag.NickNameSort = String.IsNullOrEmpty(sortOrder) ? "nickname_desc" : "NickName";
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

            var donations = from s in context.Donations select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                donations = donations.Where(s => s.FullName.ToLower().Contains(searchString));
            }

            switch (sortOrder)
            {
                case "fullname_desc":
                    donations = donations.OrderByDescending(s => s.FullName.Substring(s.FullName.LastIndexOf(" ") + 1));
                    break;
                case "Age":
                    donations = donations.OrderBy(s => s.Age);
                    break;
                case "age_desc":
                    donations = donations.OrderByDescending(s => s.Age);
                    break;
                default:  // Name ascending 
                    donations = donations.OrderBy(s => s.FullName.Substring(s.FullName.LastIndexOf(" ") + 1));
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(donations.ToPagedList(pageNumber, pageSize));
        }


        public ActionResult Create()
        {
            if (Request.Form.Count > 0)
            {
                string fullName = Request.Form["FullName"];
                int age = int.Parse(Request.Form["Age"]);
                string address = Request.Form["Address"];
                int asset = int.Parse(Request.Form["Asset"]);
                string thing = Request.Form["Thing"];

                string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
                PagodaDataContext context = new PagodaDataContext(connectionString);
                Donation donation = new Donation();

                donation.FullName = fullName;
                donation.Age = age;
                donation.Address = address;
                donation.Asset = asset;
                donation.Thing = thing;

                donation.YearUser = DateTime.Now.Year - age;

                context.Donations.InsertOnSubmit(donation);
                context.SubmitChanges();

                TempData["Message"] = "Dữ liệu của bạn đã được tạo thành công!!";

                return RedirectToAction("Index");
            }
            return View();
        }

        [CheckPermission]
        public ActionResult Delete(string DonationID)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);
            Donation donation = context.Donations.FirstOrDefault(x => x.DonationID.ToString() == DonationID);

            context.Donations.DeleteOnSubmit(donation);
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
                    var donationToDelete = context.Donations.Where(x => items.Contains(x.DonationID)).ToList();
                    context.Donations.DeleteAllOnSubmit(donationToDelete);
                    TempData["Message"] = "Tất Cả dữ liệu của bạn đã được xóa thành công!!";
                    context.SubmitChanges();
                }
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }


        public ActionResult Edit(string DonationID)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);
            Donation donation = context.Donations.FirstOrDefault(x => x.DonationID.ToString() == DonationID);

            if (Request.Form.Count == 0)
            {
                return View(donation);
            }

            donation.FullName = Request.Form["FullName"];
            donation.Age = int.Parse(Request.Form["Age"]);
            donation.Asset = int.Parse(Request.Form["Asset"]);
            donation.Address = Request.Form["Address"];
            donation.Thing = Request.Form["Thing"];

            donation.YearUser = DateTime.Now.Year - donation.Age;

            context.SubmitChanges();
            TempData["Message"] = "Dữ liệu của bạn đã được cập nhật thành công!!";

            return RedirectToAction("Index");
        }
    }
}