using System;
using System.Configuration;
using System.Web.Mvc;

namespace MSWeekRaffle.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Validated()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Error()
        {
            return View();
        }

        [HttpGet]
        public string Validate(string password)
        {
            if (password != ConfigurationManager.AppSettings["MSWEEK_SECRET"])
                return "error";

            Response.Cookies["MSWeekRaffle"]["MSWEEK_SECRET"] = ConfigurationManager.AppSettings["MSWEEK_SECRET"];
            Response.Cookies["MSWeekRaffle"].Expires = DateTime.Now.AddDays(8d);

            return "success";
        }
    }
}