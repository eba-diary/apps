using Sentry.data.Core;
using Sentry.data.Infrastructure;
using Sentry.data.Web;
using StackExchange.Profiling;
using System;
using System.Linq;
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
            AreaRegistration.RegisterAllAreas();

            GlobalConfiguration.Configure(WebApiConfig.Register);
            //WebApiConfig.Register(GlobalConfiguration.Configuration);

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            MiniProfilerConfig.RegisterMiniProfilerSettings();

            //Application Initialization
            LoggerHelper.ConfigureLogger();
            Bootstrapper.Init();

            //MVC dependency resolver
            _structureMapDependencyResolver = new StructureMapMvcDependencyResolver(Bootstrapper.Container);


            var registry = new StructureMap.Registry();
            registry.Scan((scanner) =>
            {
                scanner.TheCallingAssembly();
                scanner.WithDefaultConventions();
                scanner.With(new ControllerConvention());
            });
            Bootstrapper.Container.Configure((x) =>
            {
                x.AddRegistry(registry);
                x.For<ICurrentUserIdProvider>().Use<WebCurrentUserIdProvider>();
            });

            DependencyResolver.SetResolver(_structureMapDependencyResolver);

            //WebApi dependency resolver
            GlobalConfiguration.Configuration.DependencyResolver = new StructureMapWebApiDependencyResolver(Bootstrapper.Container);

            //###  BEGIN Sentry.Data  A### - Code below is Sentry.Data-specific
            //Create demo data, if needed
            //var dataAssetContext = _structureMapDependencyResolver.CurrentNestedContainer.GetInstance<IDataAssetContext>();
            //var datasetContext = _structureMapDependencyResolver.CurrentNestedContainer.GetInstance<IDatasetContext>();
            //var dataFeedContext = _structureMapDependencyResolver.CurrentNestedContainer.GetInstance<IDataFeedContext>();
            //var dynamicDetailsService = _structureMapDependencyResolver.CurrentNestedContainer.GetInstance<AssetDynamicDetailsService>();
            //if (dataAssetContext.Users.Count() == 0)
            //{
            //    var userSvc = _structureMapDependencyResolver.CurrentNestedContainer.GetInstance<UserService>();
            //    var demoData = new Controllers.DemoDataController(dataAssetContext, dataFeedContext, dynamicDetailsService, userSvc);
            //    demoData.Refresh();
            //}
            //###  END Sentry.Data  ### - Code above is Sentry.Data-specific

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
                MiniProfiler.Start();
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
                MiniProfiler.Stop();
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
