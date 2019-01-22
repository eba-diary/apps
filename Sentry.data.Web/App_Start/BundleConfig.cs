using System.Web.Optimization;

namespace Sentry.data.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            string dataTablesScriptsDirectory = "~/Scripts/DataTables";

            //This bundle contains basic jquery stuff and some common plugins
            bundles.Add(new ScriptBundle("~/bundles/jquery").
                        Include("~/Scripts/jquery-{version}.js").
                        Include("~/Scripts/jquery-ui.js").
                        Include("~/Scripts/jquery.unobtrusive-ajax.js").
                        Include("~/Scripts/jquery.placeholder.js").
                        Include("~/Scripts/select2.js").
                        Include("~/Scripts/jquery.ba-throttle-debounce.js").
                        Include("~/Scripts/bootbox.js").
                        Include("~/Scripts/spin.js").
                        Include("~/Scripts/ladda.js").
                        Include("~/Scripts/knockout-3.4.2.js").
                        Include("~/Scripts/knockout-sortable.min.js").
                        Include("~/Scripts/jQuery.extendext.min.js").
                        Include("~/Scripts/jquery.validate.js").
                        Include("~/Scripts/jquery.validate.unobtrusive.js").
                        Include("~/Scripts/jquery.validate.unobtrusive.sentry.js"));

            //This bundle contains basic bootstrap stuff and some common plugins specific to bootstrap
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").
                        Include("~/Scripts/bootstrap.js").
                        Include("~/Scripts/respond.js").
                        Include(dataTablesScriptsDirectory + "/moment.js").
                        Include("~/Scripts/bootstrap-datepicker.js").
                        Include("~/Scripts/bootstrap-datetimepicker.js").
                        Include("~/Scripts/bootstrap-datetimepicker.min.js").
                        Include("~/Scripts/bootstrap-submenu.js").
                        Include("~/Scripts/typeahead.bundle.js").
                        Include("~/Scripts/sentry.associates.js").
                        Include("~/Scripts/sentry.common.js"));

            //This bundle contains all of the custom javascript for your application -
            //it automatically picks up everything in the "js" folder
            bundles.Add(new ScriptBundle("~/bundles/js").
                        Include("~/js/_Shared.js").
                        IncludeDirectory("~/js", "*.js", true));

            //This bundle contains scripts needed for DataTables and related plugins
            bundles.Add(new ScriptBundle("~/bundles/dataTables").
                        Include(dataTablesScriptsDirectory + "/jquery.dataTables.js").
                        Include(dataTablesScriptsDirectory + "/dataTables.bootstrap3.js").
                        Include(dataTablesScriptsDirectory + "/dataTables.colVis.js").
                        Include(dataTablesScriptsDirectory + "/dataTables.columnFilter.js").
                        Include(dataTablesScriptsDirectory + "/dataTables.responsive.js").
                        Include(dataTablesScriptsDirectory + "/jquery.dataTables.odata.js").
                        Include(dataTablesScriptsDirectory + "/moment-with-locales.js").
                        Include(dataTablesScriptsDirectory + "/moment-with-locales.min.js").
                        Include(dataTablesScriptsDirectory + "/moment.js").
                        Include(dataTablesScriptsDirectory + "/moment.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/prettyCron").
                        Include(dataTablesScriptsDirectory + "/moment.js").
                        Include(dataTablesScriptsDirectory + "/moment.min.js").
                        Include("~/Scripts/later.js").
                        Include("~/Scripts/prettycron.js"));


            //This bundle contains styles that are used commonly across the site, including bootstrap and jquery plugins
            bundles.Add(new StyleBundle("~/bundles/css/main").
                        Include("~/Content/bootstrap.min.css", new CssRewriteUrlTransform()).
                       
                        Include("~/Content/font-awesome.min.css", new CssRewriteUrlTransform()).
                        Include("~/Content/css/select2.css", new CssRewriteUrlTransform()).
                        Include("~/Content/ladda-themeless.css"));

            string dataTablesStylesDirectory = "~/Content/DataTables/css";

            //This bundle contains styles specific to DataTables and related plugins
            bundles.Add(new StyleBundle("~/bundles/css/dataTables").
                        Include(dataTablesStylesDirectory + "/dataTables.bootstrap3.css").
                        Include(dataTablesStylesDirectory + "/dataTables.fontAwesome.css").
                        Include(dataTablesStylesDirectory + "/dataTables.colVis.css").
                        Include(dataTablesStylesDirectory + "/dataTables.responsive.css"));

            //This bundle contains styles that override everything else, and must come after all other css includes
            bundles.Add(new StyleBundle("~/bundles/css/site").
                 Include("~/Content/bootstrap-datetimepicker.css", new CssRewriteUrlTransform()).
                        Include("~/Content/sentry-internal.min.css", new CssRewriteUrlTransform()).
                        Include("~/Content/query-builder.default.min.css").
                        Include("~/Content/query-tool.css").
                        Include("~/Content/dataset-detail.css").
                        Include("~/Content/favorites.css").
                        Include("~/Content/site.css"));

            /* If you want to see content bundled/minimized when running locally, uncomment the EnableOptimizations 
               line below.  Otherwise, bundling/optimization will be driven by the compilation debug property.
               For more information, visit http://go.microsoft.com/fwlink/?LinkId=301862
            https://stackoverflow.com/questions/21270834/asp-net-mvc-bundle-not-rendering-script-files-on-staging-server-it-works-on-dev
            BundleTable.EnableOptimizations = false;
            */

        }
    }
}
