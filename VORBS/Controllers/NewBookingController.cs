using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace VORBS.Controllers
{
    public class NewBookingController : Controller
    {
        // GET: NewBooking
        public ActionResult Index()
        {
            return View("~/Views/Test/ui2.cshtml");
        }
    }
}