using System.Web.Optimization;

namespace Sentry.data.Web
{
    public static class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            //This bundle contains basic jquery stuff and some common plugins
            //bundles.Add(new ScriptBundle("~/bundles/jquery").
            //            Include("~/Scripts/jquery-{version}.js").
            //            Include("~/Scripts/jquery-ui.js").
            //            Include("~/Scripts/jquery.unobtrusive-ajax.js").
            //            Include("~/Scripts/jquery.placeholder.js").
            //            Include("~/Scripts/jquery.ba-throttle-debounce.js").
            //            Include("~/Scripts/jquery.validate.js").
            //            Include("~/Scripts/jquery.validate.unobtrusive.js").
            //            Include("~/Scripts/jquery.validate.unobtrusive.sentry.js").
            //            Include("~/Scripts/jquery.json-viewer.js").
            //            Include("~/Scripts/jQuery.extendext.min.js").
            //            Include("~/Scripts/select2.js").
            //            Include("~/Scripts/bootbox.js").
            //            Include("~/Scripts/spin.js").
            //            Include("~/Scripts/ladda.js").
            //            Include("~/Scripts/toastr.min.js").
            //            Include("~/Scripts/thenBy.min.js"));

            ////This bundle contains basic bootstrap stuff and some common plugins specific to bootstrap
            //bundles.Add(new Bundle("~/bundles/bootstrap").
            //            Include("~/Scripts/bootstrap.bundle.js").
            //            Include("~/Scripts/MDB/mdb.js").
            //            Include("~/Scripts/respond.js").
            //            Include("~/Scripts/typeahead.bundle.js").
            //            Include("~/Scripts/sentry.associates.js").
            //            Include("~/Scripts/sentry.common.js"));asdf

            //bundles.Add(new ScriptBundle("~/bundles/knockout").
            //            Include("~/Scripts/knockout-3.4.2.js").
            //            Include("~/Scripts/knockout-sortable.min.js").
            //            Include("~/Scripts/knockout-paging.js"));

            //This bundle contains all of the custom javascript for your application -
            //it automatically picks up everything in the "js" folder
            //bundles.Add(new ScriptBundle("~/bundles/js").
            //            Include("~/js/_Shared.js").
            //            IncludeDirectory("~/js", "*.js", true));

            //string dataTablesScriptsDirectory = "~/Scripts/DataTables";
            //This bundle contains scripts needed for DataTables and related plugins
            //bundles.Add(new ScriptBundle("~/bundles/dataTables").
            //            Include(dataTablesScriptsDirectory + "/jquery.dataTables.js").
            //            Include(dataTablesScriptsDirectory + "/dataTables.bootstrap4.js").
            //            Include(dataTablesScriptsDirectory + "/dataTables.columnFilter.js").
            //            Include(dataTablesScriptsDirectory + "/dataTables.responsive.js").
            //            Include(dataTablesScriptsDirectory + "/jquery.dataTables.odata.js").
            //            Include(dataTablesScriptsDirectory + "/dataTables.buttons.js").
            //            Include(dataTablesScriptsDirectory + "/buttons.colVis.js").
            //            Include(dataTablesScriptsDirectory + "/buttons.bootstrap4.js").
            //            Include(dataTablesScriptsDirectory + "/buttons.print.min.js").
            //            Include(dataTablesScriptsDirectory + "/dataTables.rowGroup.js").
            //            Include(dataTablesScriptsDirectory + "/dataTables.fixedHeader.js").
            //            Include(dataTablesScriptsDirectory + "/dataTables.select.js").
            //            Include(dataTablesScriptsDirectory + "/moment.js").
            //            Include("~/Scripts/jquery.dataTables.yadcf.js"));

            //bundles.Add(new ScriptBundle("~/bundles/prettyCron").
            //            Include(dataTablesScriptsDirectory + "/moment.js").
            //            Include(dataTablesScriptsDirectory + "/moment.min.js").
            //            Include("~/Scripts/later.js").
            //            Include("~/Scripts/prettycron.js"));

            //This bundle contains quill
            //bundles.Add(new ScriptBundle("~/bundles/quill").
            //            Include("~/Scripts/quill/quill.js"));

            //This bundle contains styles that are used commonly across the site, including bootstrap and jquery plugins
            //bundles.Add(new StyleBundle("~/bundles/css/main").
            //            Include("~/Content/bootstrap.css", new CssRewriteUrlTransform()).
            //            Include("~/Content/sentry-styles.css", new CssRewriteUrlTransform()).
            //            Include("~/Content/sentry-icons.css", new CssRewriteUrlTransform()).
            //            Include("~/Content/font-awesome.css", new CssRewriteUrlTransform()).
            //            Include("~/Content/css/select2.css", new CssRewriteUrlTransform()).
            //            Include("~/Content/ladda-themeless.css"));

            //This bundle contains styles specific to DataTables and related plugins
            //string dataTablesStylesDirectory = "~/Content/DataTables/css";
            //bundles.Add(new StyleBundle("~/bundles/css/dataTables").
            //        Include(dataTablesStylesDirectory + "/dataTables.bootstrap4.css").
            //        Include(dataTablesStylesDirectory + "/dataTables.responsive.css").
            //        Include(dataTablesStylesDirectory + "/buttons.bootstrap4.css").
            //        Include(dataTablesStylesDirectory + "/buttons.dataTables.css"));

            bundles.Add(new StyleBundle("~/bundles/css/fontawesome").
                        Include("~/Content/font-awesome.css", new CssRewriteUrlTransform()).
                        Include("~/Content/all.min.css", new CssRewriteUrlTransform()));

            //This bundle contains styles that override everything else, and must come after all other css includes
            bundles.Add(new StyleBundle("~/bundles/css/site").
                Include("~/Content/bootstrap-datetimepicker.css", new CssRewriteUrlTransform()).
                Include("~/Content/toastr.min.css").
                Include("~/Content/query-builder.default.min.css").
                Include("~/Content/query-tool.css").
                Include("~/Content/datasets.css").
                Include("~/Content/dataset-detail.css").
                Include("~/Content/favorites.css").
                Include("~/Content/fields.css").
                Include("~/Content/home.css").
                Include("~/Content/search.css").
                Include("~/Content/business-area.css").
                Include("~/Content/system-notifications.css").
                Include("~/Content/checkbox.css").
                Include("~/Content/business-intelligence.css").
                Include("~/Content/dataflow.css").
                Include("~/Content/site.css").
                Include("~/Content/filter-search.css").
                Include("~/Content/data-inventory.css").
                Include("~/Content/jquery.json-viewer.css").
                Include("~/scripts/quill/quill.snow.css").
                Include("~/Content/admin.css")
            );

            /* If you want to see content bundled/minimized when running locally, uncomment the EnableOptimizations 
               line below.  Otherwise, bundling/optimization will be driven by the compilation debug property.
               For more information, visit http://go.microsoft.com/fwlink/?LinkId=301862
            https://stackoverflow.com/questions/21270834/asp-net-mvc-bundle-not-rendering-script-files-on-staging-server-it-works-on-dev
            BundleTable.EnableOptimizations = false;
            */
        }
    }
}