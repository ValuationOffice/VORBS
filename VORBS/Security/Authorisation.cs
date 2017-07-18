using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Mvc;
using System.Web.Routing;
using VORBS.DAL;
using VORBS.Utils;
using static VORBS.Models.User;

namespace VORBS.Security
{
    public class VorbsAuthoriseAttribute : AuthorizeAttribute
    {
        private string _userPID = "";
        private int _level = 0;

        public VorbsAuthoriseAttribute(int level)
        {
            _level = level;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var authorized = base.AuthorizeCore(httpContext);
            if (!authorized)
            {
                // The user is not authorized => no need to go any further
                return false;
            }

            VORBSContext db = new VORBSContext();

            var adminUser = db.Admins.Where(x => x.PID == _userPID).SingleOrDefault();
            if (adminUser != null)
                return adminUser.PermissionLevel >= _level;
            else
                return false;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var controllerUsingThisAttribute = ((Controller)filterContext.Controller);

            string uName = controllerUsingThisAttribute.User.Identity.Name;
            _userPID = uName.Substring(uName.IndexOf("\\") + 1);

            base.OnAuthorization(filterContext);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(new
            RouteValueDictionary(new { controller = "Errors", action = "Unauthorised" }));
        }
    }

    public class VorbsApiAuthoriseAttribute : AuthorizationFilterAttribute
    {
        private int _level = 0;

        public VorbsApiAuthoriseAttribute(int level)
        {
            _level = level;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var controller = ((System.Web.Http.ApiController)actionContext.ControllerContext.Controller);

            string uname = controller.User.Identity.Name;

            if (!VorbsAuthorise.IsUserAuthorised(uname, _level))
                actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            
            base.OnAuthorization(actionContext);
        }
    }

    public static class VorbsAuthorise
    {
        public static bool IsUserAuthorised(Pid pid, int level)
        {
            VORBSContext db = new VORBSContext();

            var adminUser = db.Admins.Where(x => x.PID == pid.Identity).SingleOrDefault();
            if (adminUser != null)
                return adminUser.PermissionLevel >= level;
            else
                return false;
        }

        public static bool IsUserAuthorised(string userName, int level)
        {
            Pid _userPID = new Pid(userName.Substring(userName.IndexOf("\\") + 1));

            if (userName == null)
                return false;

            return IsUserAuthorised(_userPID, level);

        }
    }
}