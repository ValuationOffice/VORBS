using System.Web;
using System.Web.Optimization;

namespace VORBS
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/angular").Include(
                        "~/Scripts/lib/angular/angular.js",
                        "~/Scripts/lib/angular-resource/angular-resource.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/lib/modernizr/modernizr.js"));

            bundles.Add(new ScriptBundle("~/bundles/moment").Include(
                        "~/Scripts/lib/moment/moment.js"));

            bundles.Add(new ScriptBundle("~/bundles/typeahead").Include(
                        "~/Scripts/lib/typeahead.js/dist/bloodhound.js",
                        "~/Scripts/lib/typeahead.js/dist/typeahead.bundle.js",
                        "~/Scripts/lib/typeahead.js/dist/typeahead.query.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/lib/respond/dest/respond.src.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css"
                      , "~/Content/govuk.css"
                      , "~/Content/elements-page.css"
                      , "~/Content/custom.css"
                      ));
        }
    }
}
