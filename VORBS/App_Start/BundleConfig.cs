using System.Web;
using System.Web.Optimization;

namespace VORBS
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/vendor").Include(
                        "~/build/vendor.js"));

            bundles.Add(new ScriptBundle("~/bundles/angular").Include(
                        "~/node_modules/angular/angular.js",
                        "~/node_modules/angular-resource/angular-resource.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/node_modules/jquery/dist/jquery.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/node_modules/jquery-validation/dist/jquery.validate.js",
                        "~/node_modules/jquery-validation-unobtrusive/dist/jquery.validate.unobtrusive.js"));

            bundles.Add(new ScriptBundle("~/bundles/moment").Include(
                        "~/node_modules/moment/moment.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                        "~/node_modules/bootstrap-sass/assets/javascripts/bootstrap.js"));

            bundles.Add(new ScriptBundle("~/bundles/typeahead").Include(
                        "~/node_modules/typeahead.js/dist/bloodhound.js",
                        "~/node_modules/typeahead.js/dist/typeahead.bundle.js",
                        "~/node_modules/typeahead.js/dist/typeahead.query.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css"
                      , "~/Content/govuk.css"
                      , "~/Content/elements-page.css"
                      , "~/Content/custom.css"
                      ));
        }
    }
}
