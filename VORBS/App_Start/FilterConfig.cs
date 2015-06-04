using System.Configuration;
using System.Web;
using System.Web.Mvc;

namespace VORBS
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new VersionNumberInjection());
        }
    }

    public class VersionNumberInjection : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            filterContext.Controller.ViewBag.CurrentVersion = ConfigurationManager.AppSettings["version"];
        }
    }
}
