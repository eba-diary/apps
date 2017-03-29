using System.Linq;
using System.Web.Mvc;
using StackExchange.Profiling;
using System.Collections.Generic;

namespace Sentry.data.Web
{
    public static class MiniProfilerConfig
    {
        ///<summary>
        ///Customize MiniProfiler
        ///</summary>
        public static void RegisterMiniProfilerSettings()
        {
#if DEBUG
            //Wire up profiling for view rendering - in debug mode only
            var copy = ViewEngines.Engines.ToList();
            ViewEngines.Engines.Clear();
            foreach (IViewEngine asset in copy)
            {
                ViewEngines.Engines.Add(new ProfilingViewEngine(asset));
            }
#endif

            //Wire up SQL Formatter
            MiniProfiler.Settings.SqlFormatter = new StackExchange.Profiling.SqlFormatters.SqlServerFormatter();

            //Paths that shouldn't be profiled(e.g.css, js, images)
            List<string> ignored = MiniProfiler.Settings.IgnoredPaths.ToList();
            ignored.Add("WebResource.axd");
            ignored.Add("/Content/");
            ignored.Add("/Images/");
            ignored.Add("/Scripts/");
            ignored.Add("/js/");
            ignored.Add("/fonts/");
            ignored.Add("/_status/");

            MiniProfiler.Settings.IgnoredPaths = ignored.ToArray();
            MiniProfiler.Settings.PopupRenderPosition = RenderPosition.Left;
            MiniProfiler.Settings.PopupMaxTracesToShow = 10;
            MiniProfiler.Settings.RouteBasePath = "~/profiler/";

            MiniProfiler.Settings.ExcludeAssembly("NHibernate");
            MiniProfiler.Settings.ExcludeMethod("Flush");
            MiniProfiler.Settings.StackMaxLength = 256;
            MiniProfiler.Settings.ShowControls = true;
        }

    }
}
