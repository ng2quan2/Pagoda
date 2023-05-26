using Pagoda.Models;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace Pagoda.Controllers
{
    public class HomeController : Controller
    {
        [CheckLogin]

        public ActionResult Index()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PagodaConnectionString1"].ConnectionString;
            PagodaDataContext context = new PagodaDataContext(connectionString);

            //var Monk = context.Monks.Count();
            //var Buddhist = context.Buddhists.Count();
            //var Family = context.Families.Count();

            //ViewData["Monk"] = Monk;
            //ViewData["Buddhist"] = Buddhist;
            //ViewData["Family"] = Family;

            return View();
        }
    }
}