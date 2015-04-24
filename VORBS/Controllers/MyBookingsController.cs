using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace VORBS.Controllers
{
    public class MyBookingsController : Controller
    {
        // GET: MyBooking
        public ActionResult Index()
        {
            return View();
        }
    }
}