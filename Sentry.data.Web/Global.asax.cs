using AutoMapper;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.data.Infrastructure;
using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Sentry.data.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        
        private static StructureMapMvcDependencyResolver _structureMapDependencyResolver;

        protected void Application_Start()
        {
            //Configure Logging
            LoggerHelper.ConfigureLogger();

            //setup the ASP.NET thread pool
            InitThreadPool();

            AreaRegistration.RegisterAllAreas();

            GlobalConfiguration.Configure(WebApiConfig.Register);
            //WebApiConfig.Register(GlobalConfiguration.Configuration);

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            MiniProfilerConfig.RegisterMiniProfilerSettings();
            ModelBinders.Binders.Add(typeof(DataTables.Shared.DataTablesRequest), new DataTables.Mvc.DataTablesBinder());

            //Application Initialization
            Bootstrapper.Init();

            //MVC dependency resolver
            _structureMapDependencyResolver = new StructureMapMvcDependencyResolver(Bootstrapper.Container);

            var configuration = new MapperConfiguration(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));
            IMapper mapper = new Mapper(configuration);

            Bootstrapper.Container.Configure((x) =>
            {
                x.For<IList<SampleDto>>().Use(new List<SampleDto>()).Singleton();
                x.For<IMapper>().Use(mapper);
                x.Scan((s) =>
                {
                    s.TheCallingAssembly();
                    s.WithDefaultConventions();
                    s.With(new ControllerConvention());
                    s.AddAllTypesOf<IRequestModelValidator>(); //register all validators
                });
                x.For<ICurrentUserIdProvider>().Use<WebCurrentUserIdProvider>();
            });

            DependencyResolver.SetResolver(_structureMapDependencyResolver);

            //WebApi dependency resolver
            GlobalConfiguration.Configuration.DependencyResolver = new StructureMapWebApiDependencyResolver(Bootstrapper.Container);

            //###  BEGIN Sentry.Data  A### - Code below is Sentry.Data-specific

            //Remove this when CLA3819_EgressEdgeMigration is removed
            //Remove implemenation 
            SentryProxy.UseEdgeProxy = _structureMapDependencyResolver.CurrentNestedContainer.GetInstance<IDataFeatures>().CLA3819_EgressEdgeMigration.GetValue();
            //###  END Sentry.Data  ### - Code above is Sentry.Data-specific
        }



        /// <summary>
        /// Update thread pool to custom settings
        /// See https://support.microsoft.com/en-us/help/821268/contention-poor-performance-and-deadlocks-when-you-make-calls-to-web-s
        /// </summary>
        private void InitThreadPool()
        {

            //Get the defaults (based on the # of cores)
            int minWorker = 0;
            int minIo = 0;
            int maxWorker = 0;
            int maxIo = 0;
            System.Threading.ThreadPool.GetMinThreads(out minWorker, out minIo);
            System.Threading.ThreadPool.GetMaxThreads(out maxWorker, out maxIo);
            Logger.Debug($"Default thread settings were: minWorker={minWorker}, minIO={minIo}, maxWorker={maxWorker}, maxIO={maxIo}");

            //Attempt to get the overridden setting from Sentry.Configuration.  If not specified for the environment, use the default
            try
            {
                var minWorkerString = Sentry.Configuration.Config.GetHostSetting("MinWorkerThreads");
                minWorker = int.Parse(minWorkerString);
                var minIoString = Sentry.Configuration.Config.GetHostSetting("MinIOThreads");
                minIo = int.Parse(minIoString);
                var maxWorkerString = Sentry.Configuration.Config.GetHostSetting("MaxWorkerThreads");
                maxWorker = int.Parse(maxWorkerString);
                var maxIoString = Sentry.Configuration.Config.GetHostSetting("MaxIOThreads");
                maxIo = int.Parse(maxIoString);
            }
            catch (Exception)
            {
            }

            //Now update the minimum thread count settings
            System.Threading.ThreadPool.SetMaxThreads(maxWorker, maxIo);
            System.Threading.ThreadPool.SetMinThreads(minWorker, minIo);
            Logger.Info($"Adjusted thread settings to: minWorker={minWorker}, minIO={minIo}, maxWorker={maxWorker}, maxIO={maxIo}");

        }


        private void Application_Error(Object sender, EventArgs e)
        {
            LoggerHelper.LogWebException(System.Web.HttpContext.Current.Error, System.Web.HttpContext.Current);
        }

        private void Application_BeginRequest(Object sender, EventArgs e)
        {
            // Fires at the beginning of each request
            if (Request.IsLocal)
            {
                MiniProfiler.StartNew();
            }

            //skip this if the _status page is being accessed...
            if (SkipGlobalEvents())
            {
                return;
            }

        }

        private void Application_EndRequest(Object sender, EventArgs e)
        {
            if (SkipGlobalEvents())
            {
                return;
            }
            if (_structureMapDependencyResolver != null)
            {
                _structureMapDependencyResolver.DisposeNestedContainer();
            }
            if (Request.IsLocal)
            {
                MiniProfiler.Current?.Stop();
            }
        }

        private Boolean SkipGlobalEvents()
        {
            if (HttpContext.Current == null)
            {
                return true;
            }
            return HttpContext.Current.Request.Url.AbsoluteUri.ToUpper().Contains("/_STATUS/");
        }
    }
}
