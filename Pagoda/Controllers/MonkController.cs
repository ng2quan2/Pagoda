using PagedList;
using Pagoda.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.IO;
using System.Web.UI.WebControls;
using System.Web.UI;
using BoldReports.Linq;
using System.Configuration;
using System.Text.RegularExpressions;

namespace Pagoda.Controllers
{

    public class MonkController : Controller
    {
        [CheckLogin]

        public ViewResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            ViewBag.CurrentSort = sortOrder;
            ViewBag.FullNameSort = String.IsNullOrEmpty(sortOrder) ? "fullName_desc" : "";
            ViewBag.NickNameSort = String.IsNullOrEmpty(sortOrder) ? "nickName_desc" : "nickName";
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

            var monks = from s in context.Monks select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                monks = monks.Where(s => s.FullName.ToLower().Contains(searchString)
                                       || s.NickName.ToLower().Contains(searchString));
            }

            switch (sortOrder)
            {
                case "fullName_desc":
                    monks = monks.OrderByDescending(s => s.FullName.Substring(s.FullName.LastIndexOf(" ") + 1));
                    break;
                case "nickName_desc":
                    monks = monks.OrderByDescending(s => s.NickName.Substring(s.NickName.LastIndexOf(" ") + 1));
                    break;
                case "nickName":
                    monks = monks.OrderBy(s => s.NickName.Substring(s.NickName.LastIndexOf(" ") + 1));
                    break;
                case "Age":
                    monks = monks.OrderBy(s => s.Age);
                    break;
                case "age_desc":
                    monks = monks.OrderByDescending(s => s.Age);
                    break;
                default:  // Name ascending 
                    monks = monks.OrderBy(s => s.FullName.Substring(s.FullName.LastIndexOf(" ") + 1));
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(monks.ToPagedList(pageNumber, pageSize));
        }


        [HttpGet]
        public PartialViewResult Create()   //Insert PartialView  
        {
            return PartialView(new Pagoda.Models.Monk());
        }

        //[CheckLogin]
        //[HttpPost]
        //public JsonResult Create(Monk monk) // Record Insert  
        //{
        //    PagodaDataContext context = new PagodaDataContext();

        //    var star = context.Stars.FirstOrDefault(x => x.Age == monk.Age && x.Sex == monk.Sex);
        //    if (star == null)
        //    {
        //        monk.Star = star ?? null;
        //    }
        //    else
        //    {
        //        monk.Star = star;
        //    }

        //    var threat = context.Threats.FirstOrDefault(x => x.Age == monk.Age && x.Sex == monk.Sex);
        //    if (threat == null)
        //    {
        //        monk.Threat = threat ?? null;
        //    }
        //    else
        //    {
        //        monk.Threat = threat;
        //    }

        //    monk.YearUser = (DateTime.Now.Year - monk.Age) + 1;

        //    monk.Password = Guid.NewGuid().ToString("N").Substring(0, 8); // Tạo mật khẩu ngẫu nhiên

        //    var existingMonk = context.Monks.FirstOrDefault(m => m.Phone == monk.Phone);
        //    if (existingMonk != null)
        //    {
        //        ModelState.AddModelError("", "Số điện thoại đã được sử dụng trước đó");
        //        return Json(new { success = false, message = "Số điện thoại đã được sử dụng trước đó" }, JsonRequestBehavior.AllowGet);
        //    }

        //    context.Monks.InsertOnSubmit(monk);
        //    context.SubmitChanges();

        //    TempData["Message"] = "Dữ liệu của bạn đã được tạo thành công!!";
        //    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        //}

        public JsonResult Create(Monk monk) // Record Insert  
        {
            if (string.IsNullOrWhiteSpace(monk.FullName))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ họ tên của bạn");
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ họ tên của bạn" }, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrWhiteSpace(monk.YearUser.ToString()))
            {
                ModelState.AddModelError("", "Vui lòng năm sinh của bạn");
                return Json(new { success = false, message = "Vui lòng nhập năm sinh của bạn" }, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrWhiteSpace(monk.Phone))
            {
                ModelState.AddModelError("", "Vui lòng nhập số điện thoại của bạn");
                return Json(new { success = false, message = "Vui lòng nhập số điện thoại của bạn" }, JsonRequestBehavior.AllowGet);
            }
            else if (!Regex.IsMatch(monk.Phone, @"^\d{10}$"))
            {
                ModelState.AddModelError("", "Số điện thoại phải không đúng định dạng");
                return Json(new { success = false, message = "Số điện thoại phải không đúng định dạng" }, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrWhiteSpace((monk.Sex).ToString()))
            {
                ModelState.AddModelError("", "Vui lòng chọn giới tính");
                return Json(new { success = false, message = "Vui lòng chọn giới tính" }, JsonRequestBehavior.AllowGet);
            }

            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            var existingBuddhist = context.Monks.FirstOrDefault(m => m.Phone == monk.Phone);
            if (existingBuddhist != null)
            {
                ModelState.AddModelError("", "Số điện thoại đã được sử dụng trước đó");
                return Json(new { success = false, message = "Số điện thoại đã được sử dụng trước đó" }, JsonRequestBehavior.AllowGet);
            }

            monk.Age = (DateTime.Now.Year - monk.YearUser) + 1;

            var star = context.Stars.FirstOrDefault(x => x.Age == monk.Age && x.Sex == monk.Sex);
            monk.Star = star;

            var threat = context.Threats.FirstOrDefault(x => x.Age == monk.Age && x.Sex == monk.Sex);
            monk.Threat = threat;

            monk.Password = Guid.NewGuid().ToString("N").Substring(0, 8); // Tạo mật khẩu ngẫu nhiên

            context.Monks.InsertOnSubmit(monk);

            context.SubmitChanges();

            TempData["Message"] = "Dữ liệu của bạn đã được tạo thành công!!";

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [CheckPermission]
        public ActionResult Delete(string MonkID)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);
            Monk monk = context.Monks.FirstOrDefault(x => x.MonkID.ToString() == MonkID);

            context.Monks.DeleteOnSubmit(monk);
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
                    var monkToDelete = context.Monks.Where(x => items.Contains(x.MonkID)).ToList();
                    context.Monks.DeleteAllOnSubmit(monkToDelete);
                    TempData["Message"] = "Tất Cả dữ liệu của bạn đã được xóa thành công!!";
                    context.SubmitChanges();
                }
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }


        [HttpGet]
        public PartialViewResult Edit(Int32 MonkID)  // Update PartialView  
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);
            Monk monk = context.Monks.Where(x => x.MonkID == MonkID).FirstOrDefault();
            Monk monks = new Monk();

            monks.MonkID = monk.MonkID;
            monks.FullName = monk.FullName;
            monks.NickName = monk.NickName;
            monks.Age = monk.Age;
            monks.Sex = monk.Sex;
            monks.Phone = monk.Phone;
            monks.Address = monk.Address;
            monks.StarID = monk.StarID;
            monks.ThreatID = monk.ThreatID;

            return PartialView(monks);
        }


        [HttpPost]
        public JsonResult Edit(Monk monk)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);
            Monk mk = context.Monks.Where(x => x.MonkID == monk.MonkID).FirstOrDefault();

            if (mk != null)
            {
                mk.FullName = monk.FullName;
                mk.NickName = monk.NickName;
                mk.Age = monk.Age;
                mk.Sex = monk.Sex;
                mk.Phone = monk.Phone;
                mk.Address = monk.Address;

                var star = context.Stars.FirstOrDefault(x => x.Age == monk.Age && x.Sex == monk.Sex);
                if (star == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy sao phù hợp");
                    return Json(new { success = false, message = "Không tìm thấy sao phù hợp" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    mk.Star = star;
                }

                var threat = context.Threats.FirstOrDefault(x => x.Age == monk.Age && x.Sex == monk.Sex);
                if (threat == null)
                {
                    mk.Threat = null;
                }
                else
                {
                    mk.Threat = threat;
                }

                context.SubmitChanges();
                TempData["Message"] = "Dữ liệu của bạn đã được cập nhật thành công!!";
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                ModelState.AddModelError("", "Không tìm thấy thông tin tăng sĩ");
                return Json(new { success = false, message = "Không tìm thấy thông tin tăng sĩ" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult EachPerson(int MonkID)
        {

            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            Monk monk = context.Monks.FirstOrDefault(x => x.MonkID == MonkID);

            return View(monk);
        }

        public ActionResult PrintEP(int MonkID)
        {
            var report = new Rotativa.ActionAsPdf("EachPerson", new { MonkID = MonkID });
            return report;
        }

        public ActionResult List(string sortOrder)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            ViewBag.FullNameSort = String.IsNullOrEmpty(sortOrder) ? "fullname_desc" : "";

            IEnumerable<Monk> dsMonk = context.Monks.ToList();

            switch (sortOrder)
            {
                case "fullname_desc":
                    dsMonk = dsMonk.OrderByDescending(s => s.FullName.Substring(s.FullName.LastIndexOf(" ") + 1));
                    break;
                default:
                    dsMonk = dsMonk.OrderBy(s => (s.FullName).Split().Last());
                    break;
            }

            return View(dsMonk);
        }

        public ActionResult PrintPDF()
        {
            var report = new Rotativa.ActionAsPdf("List")
            {
                PageMargins = { Left = 20, Bottom = 20, Right = 20, Top = 20 },
                PageSize = Rotativa.Options.Size.A4,
            };
            return report;
        }

        public ActionResult ExportWord()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            var data = context.Monks.ToList();
            GridView gridview = new GridView();
            gridview.DataSource = data;
            gridview.DataBind();

            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment;filename = word.doc");
            Response.ContentType = "application/ms-word";
            Response.Charset = "";

            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter htw = new HtmlTextWriter(sw))
                {
                    gridview.RenderControl(htw);
                    Response.Output.Write(sw.ToString());
                    Response.Flush();
                    Response.End();
                }
            }
            return View();
        }

        public ActionResult ExportToExcel()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            var gv = new GridView();
            var data = context.Monks.ToList();

            gv.DataSource = data;
            gv.DataBind();

            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=excel.xls");
            Response.ContentType = "application/ms-excel";

            Response.Charset = "";
            StringWriter objStringWriter = new StringWriter();
            HtmlTextWriter objHtmlTextWriter = new HtmlTextWriter(objStringWriter);

            gv.RenderControl(objHtmlTextWriter);

            Response.Output.Write(objStringWriter.ToString());
            Response.Flush();
            Response.End();

            return View("Index");
        }
    }
}