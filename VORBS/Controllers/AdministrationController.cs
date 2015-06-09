﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace VORBS.Controllers
{
    [VORBS.Security.VorbsAuthorise(1)]
    public class AdministrationController : Controller
    {
        //
        // GET: /Administration/
        public ActionResult Index()
        {
            return View();
        }
	}
}