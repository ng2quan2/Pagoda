using PagedList;
using Pagoda.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.Web.UI;
using BoldReports.Linq;
using Pagoda;
using System.Configuration;

namespace Pagoda.Controllers
{
    public class FamilyController : Controller
    {
        [CheckLogin]

        public ViewResult Index(string sortOrder, string currentFilter, string searchString, int? page, string familyID)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            ViewBag.CurrentSort = sortOrder;
            ViewBag.FullNameSort = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var families = from s in context.Families select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                families = families.Where(s => s.FamilyName.ToLower().Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    families = families.OrderByDescending(s => s.FamilyName.Substring(s.FamilyName.LastIndexOf(" ") + 1));
                    break;
                default:  // Name ascending 
                    families = families.OrderBy(s => s.FamilyName.Substring(s.FamilyName.LastIndexOf(" ") + 1));
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(families.ToPagedList(pageNumber, pageSize));
        }


        public ActionResult Create()
        {
            if (Request.Form.Count > 0)
            {
                string familyName = Request.Form["FamilyName"];
                string address = Request.Form["Address"];

                string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
                PagodaDataContext context = new PagodaDataContext(connectionString);
                Family family = new Family();

                family.FamilyName = familyName;
                family.Address = address;

                context.Families.InsertOnSubmit(family);
                context.SubmitChanges();
                TempData["Message"] = "Dữ liệu của bạn đã được tạo thành công!!";

                return RedirectToAction("Index");
            }
            return View();
        }

        [CheckPermission]
        public ActionResult Delete(string FamilyID)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);
            Family family = context.Families.FirstOrDefault(x => x.FamilyID.ToString() == FamilyID);

            context.Families.DeleteOnSubmit(family);
            context.SubmitChanges();
            TempData["Message"] = "Dữ liệu của bạn đã được xóa thành công!!";

            return RedirectToAction("Index");
        }


        public ActionResult Edit(string FamilyID)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);
            Family family = context.Families.FirstOrDefault(x => x.FamilyID.ToString() == FamilyID);

            if (Request.Form.Count == 0)
            {
                return View(family);
            }

            family.FamilyName = Request.Form["FamilyName"];
            family.Address = Request.Form["Address"];

            context.SubmitChanges();

            TempData["Message"] = "Dữ liệu của bạn đã được cập nhật thành công!!";

            return RedirectToAction("Index");
        }

        public ActionResult List(string sortOrder)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            ViewBag.FullNameSort = String.IsNullOrEmpty(sortOrder) ? "fullname_desc" : "";

            IEnumerable<Family> dsFamily = context.Families.ToList();

            switch (sortOrder)
            {
                case "fullname_desc":
                    dsFamily = dsFamily.OrderByDescending(s => (s.FamilyName).Split().Last());
                    break;
                default:
                    dsFamily = dsFamily.OrderBy(s => (s.FamilyName).Split().Last());
                    break;
            }

            return View(dsFamily);
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

            var data = context.Families.ToList();
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
            var data = context.Families.ToList();

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