using StackExchange.Profiling;
using System.Collections.Generic;
using Sentry.Profiling.AspNet;
using StackExchange.Profiling.Mvc;

namespace Sentry.data.Web
{
    public static class MiniProfilerConfig
    {
        ///<summary>
        ///Customize MiniProfiler
        ///</summary>
        public static void RegisterMiniProfilerSettings()
        {

            //Paths that shouldn't be profiled(e.g.css, js, images)
            HashSet<string> ignored = MiniProfiler.DefaultOptions.IgnoredPaths;
            ignored.Add("WebResource.axd");
            ignored.Add("/Content/");
            ignored.Add("/Images/");
            ignored.Add("/Scripts/");
            ignored.Add("/js/");
            ignored.Add("/fonts/");
            ignored.Add("/_status/");

            StackExchange.Profiling.MiniProfiler.Configure(new MiniProfilerOptions
            {
                RouteBasePath = "~/profiler",
                PopupRenderPosition = RenderPosition.Left,
                PopupMaxTracesToShow = 10,
                ResultsAuthorize = request => request.IsLocal,
                StackMaxLength = 256,
                SqlFormatter = new StackExchange.Profiling.SqlFormatters.SqlServerFormatter(),
                ShowControls = true,
                ProfilerProvider = new MiniProfilerRequestProvider()

            })
                .IgnorePath("WebResource.axd")
                .IgnorePath("/Content/")
                .IgnorePath("/Images/")
                .IgnorePath("/Scripts/")
                .IgnorePath("/js/")
                .IgnorePath("/fonts/")
                .IgnorePath("/_status/")
                .ExcludeAssembly("NHibernate")
                .ExcludeMethod("Flush")
#if DEBUG
                .AddViewProfiling()
#endif
            ;

        }

    }
}
