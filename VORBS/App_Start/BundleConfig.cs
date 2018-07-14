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
                        "~/node_modules/angular/angular.js",
                        "~/node_modules/angular-resource/angular-resource.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/node_modules/modernizr/modernizr.js"));

            bundles.Add(new ScriptBundle("~/bundles/moment").Include(
                        "~/node_modules/moment/moment.js"));

            bundles.Add(new ScriptBundle("~/bundles/typeahead").Include(
                        "~/node_modules/typeahead.js/dist/bloodhound.js",
                        "~/node_modules/typeahead.js/dist/typeahead.bundle.js",
                        "~/node_modules/typeahead.js/dist/typeahead.query.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/node_modules/respond/dest/respond.src.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css"
                      , "~/Content/govuk.css"
                      , "~/Content/elements-page.css"
                      , "~/Content/custom.css"
                      ));
        }
    }
}
