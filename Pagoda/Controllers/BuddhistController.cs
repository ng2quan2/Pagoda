using PagedList;
using Pagoda.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Text.RegularExpressions;
using System.Configuration;

namespace Pagoda.Controllers
{
    public class BuddhistController : Controller
    {
        [CheckLogin]

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

            var buddhists = from s in context.Buddhists select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                buddhists = buddhists.Where(s => s.FullName.ToLower().Contains(searchString)
                                       || s.NickName.ToLower().Contains(searchString));
            }

            switch (sortOrder)
            {
                case "fullname_desc":
                    buddhists = buddhists.OrderByDescending(s => s.FullName.Substring(s.FullName.LastIndexOf(" ") + 1));
                    break;
                case "nickname_desc":
                    buddhists = buddhists.OrderByDescending(s => s.NickName.Substring(s.NickName.LastIndexOf(" ") + 1));
                    break;
                case "NickName":
                    buddhists = buddhists.OrderBy(s => s.NickName.Substring(s.NickName.LastIndexOf(" ") + 1));
                    break;
                case "Age":
                    buddhists = buddhists.OrderBy(s => s.Age);
                    break;
                case "age_desc":
                    buddhists = buddhists.OrderByDescending(s => s.Age);
                    break;
                default:  // Name ascending 
                    buddhists = buddhists.OrderBy(s => s.FullName.Substring(s.FullName.LastIndexOf(" ") + 1));
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(buddhists.ToPagedList(pageNumber, pageSize));
        }


        [HttpGet]
        public PartialViewResult Create()   //Insert PartialView  
        {
            return PartialView(new Pagoda.Models.Buddhist());
        }

        //[CheckLogin]
        //[HttpPost]
        //public JsonResult Create(Buddhist buddhist) // Record Insert  
        //{
        //    PagodaDataContext context = new PagodaDataContext();

        //    var star = context.Stars.FirstOrDefault(x => x.Age == buddhist.Age && x.Sex == buddhist.Sex);
        //    if (star == null)
        //    {
        //        buddhist.Star = star ?? null;
        //    }
        //    else
        //    {
        //        buddhist.Star = star;
        //    }

        //    var threat = context.Threats.FirstOrDefault(x => x.Age == buddhist.Age && x.Sex == buddhist.Sex);
        //    if (threat == null)
        //    {
        //        buddhist.Threat = threat ?? null;
        //    }
        //    else
        //    {
        //        buddhist.Threat = threat;
        //    }

        //    var existingBuddhist = context.Buddhists.FirstOrDefault(m => m.Phone == buddhist.Phone);
        //    if (existingBuddhist != null)
        //    {
        //        ModelState.AddModelError("", "Số điện thoại đã được sử dụng trước đó");
        //        return Json(new { success = false, message = "Số điện thoại đã được sử dụng trước đó" }, JsonRequestBehavior.AllowGet);
        //    }

        //    buddhist.YearUser = (DateTime.Now.Year - buddhist.Age) + 1;

        //    buddhist.Password = Guid.NewGuid().ToString("N").Substring(0, 8); // Tạo mật khẩu ngẫu nhiên

        //    context.Buddhists.InsertOnSubmit(buddhist);
        //    context.SubmitChanges();

        //    TempData["Message"] = "Dữ liệu của bạn đã được tạo thành công!!";

        //    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        //}


        [HttpPost]
        public JsonResult Create(Buddhist buddhist) // Record Insert  
        {
            if (string.IsNullOrWhiteSpace(buddhist.FullName))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ họ tên của bạn");
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ họ tên của bạn" }, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrWhiteSpace(buddhist.YearUser.ToString()))
            {
                ModelState.AddModelError("", "Vui lòng năm sinh của bạn");
                return Json(new { success = false, message = "Vui lòng nhập năm sinh của bạn" }, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrWhiteSpace(buddhist.Phone))
            {
                ModelState.AddModelError("", "Vui lòng nhập số điện thoại của bạn");
                return Json(new { success = false, message = "Vui lòng nhập số điện thoại của bạn" }, JsonRequestBehavior.AllowGet);
            }
            else if (!Regex.IsMatch(buddhist.Phone, @"^\d{10}$"))
            {
                ModelState.AddModelError("", "Số điện thoại phải không đúng định dạng");
                return Json(new { success = false, message = "Số điện thoại phải không đúng định dạng" }, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrWhiteSpace((buddhist.Sex).ToString()))
            {
                ModelState.AddModelError("", "Vui lòng chọn giới tính");
                return Json(new { success = false, message = "Vui lòng chọn giới tính" }, JsonRequestBehavior.AllowGet);
            }

            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            var existingBuddhist = context.Buddhists.FirstOrDefault(m => m.Phone == buddhist.Phone);
            if (existingBuddhist != null)
            {
                ModelState.AddModelError("", "Số điện thoại đã được sử dụng trước đó");
                return Json(new { success = false, message = "Số điện thoại đã được sử dụng trước đó" }, JsonRequestBehavior.AllowGet);
            }

            buddhist.Age = (DateTime.Now.Year - buddhist.YearUser) + 1;

            var star = context.Stars.FirstOrDefault(x => x.Age == buddhist.Age && x.Sex == buddhist.Sex);
            buddhist.Star = star;

            var threat = context.Threats.FirstOrDefault(x => x.Age == buddhist.Age && x.Sex == buddhist.Sex);
            buddhist.Threat = threat;

            buddhist.Password = Guid.NewGuid().ToString("N").Substring(0, 8); // Tạo mật khẩu ngẫu nhiên

            context.Buddhists.InsertOnSubmit(buddhist);
            context.SubmitChanges();

            TempData["Message"] = "Dữ liệu của bạn đã được tạo thành công!!";

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [CheckPermission]
        public ActionResult Delete(string BuddhistID)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);
            Buddhist buddhist = context.Buddhists.FirstOrDefault(x => x.BuddhistID.ToString() == BuddhistID);

            context.Buddhists.DeleteOnSubmit(buddhist);
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
                    var buddhistToDelete = context.Buddhists.Where(x => items.Contains(x.BuddhistID)).ToList();
                    context.Buddhists.DeleteAllOnSubmit(buddhistToDelete);
                    TempData["Message"] = "Tất Cả dữ liệu của bạn đã được xóa thành công!!";
                    context.SubmitChanges();
                }
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }



        [HttpGet]
        public PartialViewResult Edit(Int32 BuddhistID)  // Update PartialView  
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);
            Buddhist buddhist = context.Buddhists.Where(x => x.BuddhistID == BuddhistID).FirstOrDefault();
            Buddhist buddhists = new Buddhist();

            buddhists.BuddhistID = buddhist.BuddhistID;
            buddhists.FullName = buddhist.FullName;
            buddhists.NickName = buddhist.NickName;
            buddhists.Age = buddhist.Age;
            buddhists.Sex = buddhist.Sex;
            buddhists.Phone = buddhist.Phone;
            buddhists.Address = buddhist.Address;
            buddhists.Status = buddhist.Status;
            buddhists.StarID = buddhist.StarID;
            buddhists.ThreatID = buddhist.ThreatID;

            return PartialView(buddhists);
        }


        [HttpPost]
        public JsonResult Edit(Buddhist buddhist)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);
            Buddhist bh = context.Buddhists.Where(x => x.BuddhistID == buddhist.BuddhistID).FirstOrDefault();

            if (bh != null)
            {
                bh.FullName = buddhist.FullName;
                bh.NickName = buddhist.NickName;
                bh.Age = buddhist.Age;
                bh.Sex = buddhist.Sex;
                bh.Phone = buddhist.Phone;
                bh.Address = buddhist.Address;
                bh.Status = buddhist.Status;

                var star = context.Stars.FirstOrDefault(x => x.Age == buddhist.Age && x.Sex == buddhist.Sex);
                if (star == null)
                {
                    bh.Star = star ?? null;
                }
                else
                {
                    bh.Star = star;
                }

                var threat = context.Threats.FirstOrDefault(x => x.Age == buddhist.Age && x.Sex == buddhist.Sex);
                if (threat == null)
                {
                    bh.Threat = threat ?? null;
                }
                else
                {
                    bh.Threat = threat;
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

        public ActionResult List()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            IEnumerable<Buddhist> dsbuddhist = context.Buddhists.ToList();

            return View(dsbuddhist);
        }

        public ActionResult EachPerson(int BuddhistID)
        {

            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            Buddhist bh = context.Buddhists.FirstOrDefault(x => x.BuddhistID == BuddhistID);

            return View(bh);
        }

        public ActionResult PrintEP(int BuddhistID)
        {
            var report = new Rotativa.ActionAsPdf("EachPerson", new { BuddhistID = BuddhistID });
            return report;
        }

        public ActionResult PrintPDF()
        {
            var report = new Rotativa.ActionAsPdf("List");
            return report;
        }

        public ActionResult ExportWord()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            var data = context.Buddhists.ToList();
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
            var data = context.Buddhists.ToList();

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