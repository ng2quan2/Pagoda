using Pagoda.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.PeerToPeer;
using System.Web;
using System.Web.Mvc;
using System.Web.Profile;
using System.Web.Security;

namespace Pagoda.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            if (Request.Form.Count > 0)
            {
                var userPhone = Request.Form["UserPhone"];
                var password = Request.Form["PassWord"];

                if (checkPassword(userPhone, password))
                {
                    var dsUser = getUserInfo(userPhone);

                    Session.Add("LoginSession", true);
                    Session.Add("MaNVSession", userPhone);
                    Session.Add("MaNhomSession", dsUser[0].RoleID);

                    var monk = context.Monks.FirstOrDefault(m => m.Phone == userPhone);
                    if (monk != null)
                    {
                        Session["MonkId"] = monk.MonkID;
                        Session["MonkFullName"] = monk.FullName;
                        Session["MonkNickName"] = monk.NickName;
                        Session["MonkAge"] = monk.Age;
                        Session["MonkSex"] = monk.Sex;
                        Session["MonkStar"] = monk.Star.StarName;
                        Session["MonkThreat"] = monk.Threat.ThreatName;
                        Session["MonkSex"] = monk.Sex;
                        Session["MonkPhone"] = monk.Phone;
                        Session["MonkAddress"] = monk.Address;
                        Session["MonkDisplayName"] = monk.DisplayName;
                        Session["MonkYearUser"] = monk.YearUser;
                        Session["MonkPassword"] = monk.Password;
                        // Lưu thêm các thông tin khác của Monk nếu cần thiết
                    }

                    var buddhist = context.Buddhists.FirstOrDefault(m => m.Phone == userPhone);
                    if (buddhist != null)
                    {
                        Session["BuddhistId"] = buddhist.BuddhistID;
                        Session["BuddhistFullName"] = buddhist.FullName;
                        Session["BuddhistNickName"] = buddhist.NickName;
                        Session["BuddhistAge"] = buddhist.Age;
                        Session["BuddhistSex"] = buddhist.Sex;
                        Session["BuddhistStar"] = buddhist.Star.StarName;
                        Session["BuddhistThreat"] = buddhist.Threat.ThreatName;
                        Session["BuddhistSex"] = buddhist.Sex;
                        Session["BuddhistPhone"] = buddhist.Phone;
                        Session["BuddhistAddress"] = buddhist.Address;
                        Session["BuddhistDisplayName"] = buddhist.DisplayName;
                        Session["BuddhistYearUser"] = buddhist.YearUser;
                        Session["BuddhistStatus"] = buddhist.Status;
                        Session["BuddhistPassword"] = buddhist.Password;
                        // Lưu thêm các thông tin khác của Monk nếu cần thiết
                    }

                    return Redirect("/");
                }
                else
                    ViewBag.ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng";
                    return RedirectToAction("Index");
            }
            return View();
        }

        public bool checkPassword(string userPhone, string password)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            var ul = context.UserLogins
                        .Where(x => x.UserPhone.ToString() == userPhone && x.Password == password)
                        .ToList().Count();
            if (ul != 0)
                return true;
            else
                return false;
        }

        public List<UserLogin> getUserInfo(string userPhone)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            var Login = context.UserLogins
                        .Where(x => x.UserPhone.ToString() == userPhone)
                        .ToList();
            return Login;
        }

        public ActionResult EditProfile()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            if (Session["MonkId"] == null)
            {
                return RedirectToAction("Index");
            }

            var monkId = (int)Session["MonkId"];
            var monk = context.Monks.FirstOrDefault(m => m.MonkID == monkId);

            if (monk == null)
            {
                return HttpNotFound();
            }

            var model = new Monk
            {
                FullName = monk.FullName,
                NickName = monk.NickName,
                Age = monk.Age,
                Sex = monk.Sex,
                Phone = monk.Phone,
                Address = monk.Address,
                DisplayName = monk.DisplayName,
                YearUser = DateTime.Now.Year - monk.Age + 1
            };

            var star = context.Stars.FirstOrDefault(x => x.Age == monk.Age && x.Sex == monk.Sex);
            if (star == null)
            {
                monk.Star = star ?? null;
            }
            else
            {
                monk.Star = star;
            }

            var threat = context.Threats.FirstOrDefault(x => x.Age == monk.Age && x.Sex == monk.Sex);
            if (threat == null)
            {
                monk.Threat = threat ?? null;
            }
            else
            {
                monk.Threat = threat;
            }

            return View(model);
        }


        [HttpPost]
        public ActionResult EditProfile(Monk model)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            if (Session["MonkId"] == null)
            {
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                var monkId = (int)Session["MonkId"];
                var monk = context.Monks.FirstOrDefault(m => m.MonkID == monkId);

                if (monk == null)
                {
                    return HttpNotFound();
                }

                monk.FullName = model.FullName;
                monk.NickName = model.NickName;
                monk.Age = model.Age;
                monk.Sex = model.Sex;
                monk.Phone = model.Phone;
                monk.Address = model.Address;
                monk.DisplayName = model.DisplayName;
                monk.YearUser = DateTime.Now.Year - model.Age + 1;

                var star = context.Stars.FirstOrDefault(x => x.Age == monk.Age && x.Sex == monk.Sex);
                if (star == null)
                {
                    monk.Star = star ?? null;
                }
                else
                {
                    monk.Star = star;
                }

                var threat = context.Threats.FirstOrDefault(x => x.Age == monk.Age && x.Sex == monk.Sex);
                if (threat == null)
                {
                    monk.Threat = threat ?? null;
                }
                else
                {
                    monk.Threat = threat;
                }

                context.SubmitChanges();

                Session["MonkFullName"] = monk.FullName;
                Session["MonkNickName"] = monk.NickName;
                Session["MonkAge"] = monk.Age;
                Session["MonkSex"] = monk.Sex;
                Session["MonkStar"] = monk.Star.StarName;
                Session["MonkThreat"] = monk.Threat.ThreatName;
                Session["MonkPhone"] = monk.Phone;
                Session["MonkAddress"] = monk.Address;
                Session["MonkDisplayName"] = monk.DisplayName;
                Session["MonkYearUser"] = monk.YearUser;

                TempData["Message"] = "Dữ liệu của bạn đã được cập nhật thành công!!";

                // Cập nhật các thông tin khác của Monk trong session nếu cần thiết

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        public ActionResult EditProfile_Buddhist()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            if (Session["BuddhistId"] == null)
            {
                return RedirectToAction("Index");
            }

            var buddhistId = (int)Session["BuddhistId"];
            var buddhist = context.Buddhists.FirstOrDefault(m => m.BuddhistID == buddhistId);

            if (buddhist == null)
            {
                return HttpNotFound();
            }

            var model = new Buddhist
            {
                FullName = buddhist.FullName,
                NickName = buddhist.NickName,
                Age = buddhist.Age,
                Sex = buddhist.Sex,
                Phone = buddhist.Phone,
                Address = buddhist.Address,
                DisplayName = buddhist.DisplayName,
                Status = buddhist.Status,
                YearUser = DateTime.Now.Year - buddhist.Age + 1
            };

            var star = context.Stars.FirstOrDefault(x => x.Age == buddhist.Age && x.Sex == buddhist.Sex);
            if (star == null)
            {
                buddhist.Star = star ?? null;
            }
            else
            {
                buddhist.Star = star;
            }

            var threat = context.Threats.FirstOrDefault(x => x.Age == buddhist.Age && x.Sex == buddhist.Sex);
            if (threat == null)
            {
                buddhist.Threat = threat ?? null;
            }
            else
            {
                buddhist.Threat = threat;
            }

            return View(model);
        }


        [HttpPost]
        public ActionResult EditProfile_Buddhist(Buddhist model)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            if (Session["BuddhistId"] == null)
            {
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                var buddhistId = (int)Session["BuddhistId"];
                var buddhist = context.Buddhists.FirstOrDefault(m => m.BuddhistID == buddhistId);

                if (buddhist == null)
                {
                    return HttpNotFound();
                }

                buddhist.FullName = model.FullName;
                buddhist.NickName = model.NickName;
                buddhist.Age = model.Age;
                buddhist.Sex = model.Sex;
                buddhist.Phone = model.Phone;
                buddhist.Address = model.Address;
                buddhist.DisplayName = model.DisplayName;
                buddhist.YearUser = DateTime.Now.Year - model.Age + 1;

                var star = context.Stars.FirstOrDefault(x => x.Age == buddhist.Age && x.Sex == buddhist.Sex);
                if (star == null)
                {
                    buddhist.Star = star ?? null;
                }
                else
                {
                    buddhist.Star = star;
                }

                var threat = context.Threats.FirstOrDefault(x => x.Age == buddhist.Age && x.Sex == buddhist.Sex);
                if (threat == null)
                {
                    buddhist.Threat = threat ?? null;
                }
                else
                {
                    buddhist.Threat = threat;
                }

                context.SubmitChanges();

                Session["BuddhistFullName"] = buddhist.FullName;
                Session["BuddhistNickName"] = buddhist.NickName;
                Session["BuddhistAge"] = buddhist.Age;
                Session["BuddhistSex"] = buddhist.Sex;
                Session["BuddhistStar"] = buddhist.Star.StarName;
                Session["BuddhistThreat"] = buddhist.Threat.ThreatName;
                Session["BuddhistPhone"] = buddhist.Phone;
                Session["BuddhistAddress"] = buddhist.Address;
                Session["BuddhistDisplayName"] = buddhist.DisplayName;
                Session["BuddhistYearUser"] = buddhist.YearUser;
                Session["BuddhistStatus"] = buddhist.Status;

                TempData["Message"] = "Dữ liệu của bạn đã được cập nhật thành công!!";

                // Cập nhật các thông tin khác của Monk trong session nếu cần thiết

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        public ActionResult ChangePassword()
        {
            if (Session["MonkId"] == null)
            {
                return RedirectToAction("Index");
            }

            var model = new ChangePassword();

            return View(model);
        }


        [HttpPost]
        public ActionResult ChangePassword(ChangePassword model)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            if (ModelState.IsValid)
            {
                var monkId = (int)Session["MonkId"];
                var monk = context.Monks.SingleOrDefault(m => m.MonkID == monkId);

                if (monk == null)
                {
                    return HttpNotFound();
                }

                if (monk.Password == model.CurrentPassword)
                {
                    monk.Password = model.NewPassword;

                    context.SubmitChanges();

                    TempData["Message"] = "Mật khẩu của bạn đã được cập nhật thành công!!";

                    Session["MonkPassword"] = monk.Password;

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Mật khẩu hiện tại không đúng.");
                }
            }

            return View(model);
        }

        public ActionResult ChangePassword_Buddhist()
        {
            if (Session["BuddhistId"] == null)
            {
                return RedirectToAction("Index");
            }

            var model = new ChangePassword();

            return View(model);
        }


        [HttpPost]
        public ActionResult ChangePassword_Buddhist(ChangePassword model)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            if (ModelState.IsValid)
            {
                var buddhistId = (int)Session["BuddhistId"];
                var buddhist = context.Buddhists.SingleOrDefault(m => m.BuddhistID == buddhistId);

                if (buddhist == null)
                {
                    return HttpNotFound();
                }

                if (buddhist.Password == model.CurrentPassword)
                {
                    buddhist.Password = model.NewPassword;

                    context.SubmitChanges();

                    TempData["Message"] = "Mật khẩu của bạn đã được cập nhật thành công!!";

                    Session["BuddhistPassword"] = buddhist.Password;

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Mật khẩu hiện tại không đúng.");
                }
            }

            return View(model);
        }
    }
}