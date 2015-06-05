using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using VORBS.DAL;

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

    public static class VorbsAuthorise
    {
        public static bool IsUserAuthorised(string userName, int level)
        {
            VORBSContext db = new VORBSContext();

            string uName = userName;
            string _userPID = uName.Substring(uName.IndexOf("\\") + 1);

            if (userName == null)
                return false;

            var adminUser = db.Admins.Where(x => x.PID == _userPID).SingleOrDefault();
            if (adminUser != null)
                return adminUser.PermissionLevel >= level;
            else
                return false;
        }
    }
}