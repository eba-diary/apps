using DoddleReport.Web;
using Sentry.data.Core;
using System.Web.Mvc;
using System.Web.Routing;

namespace Sentry.data.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapReportingRoute();

            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("_status/{*pathInfo}");

            //enable attribute-based routing - see http://blogs.msdn.com/b/webdev/archive/2013/10/17/attribute-routing-in-asp-net-mvc-5.aspx
            routes.MapMvcAttributeRoutes();

            routes.MapRoute(
                name: "DatasetSearchFeature",
                url: "Search/{searchType}/{indexSuffix}",
                defaults: new { controller = "Search", action = "Index", searchType = GlobalConstants.SearchType.DATASET_SEARCH, indexSuffix = UrlParameter.Optional },
                constraints: new { feature = new SearchRouteConstraint() }
            );

            routes.MapRoute(
                name: "DataAssetIndex",
                url: "DataAsset/Index",
                defaults: new { controller = "DataAsset", action = "Index", id = 0 }            
            );

            routes.MapRoute(
                name: "DataAssetGetEditAssetNotificationPartialView",
                url: "DataAsset/GetEditAssetNotificationPartialView",
                defaults: new { controller = "DataAsset", action = "GetEditAssetNotificationPartialView" }
            );

            routes.MapRoute(
                name: "DataAssetName",
                url: "DataAsset/{assetName}",
                defaults: new { controller = "DataAsset", action = "DataAsset" }
            );

            routes.MapRoute(
                name: "DataModel",
                url: "DataModel/{dataModelName}/{*filename}",
                defaults: new { controller = "ExternalFile", action = "DataModel" }
            );

            routes.MapRoute(
               name: "Retrieval Job Creation",
               url: "Config/{configId}/Job/Create",
               defaults: new { controller = "Config", action = "CreateRetrievalJob", configId = 251 }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                //defaults: new { controller = "DataAsset", action = "Index", id = 1 }
                //defaults: new { controller = "Dataset", action = "Index", id = UrlParameter.Optional }
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }            
            );

            routes.MapRoute(
                name: "ODCFile",
                url: "DataAsset/ODCFile/{assetId}/{cubeName}",
                defaults: new { controller = "ExternalFile", action = "ODCFile" }
            );

            routes.MapRoute(
                name: "Lineage",
                url: "Lineage/{line}/{assetName}/{businessObject}/{sourceElement}",
                defaults: new { controller = "DataAsset", action = "Lineage", assetName = "", businessObject = UrlParameter.Optional, sourceElement = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Dataset Configuration",
                url: "Dataset/Detail/{id}/Configuration",
                defaults: new { controller = "Dataset", action = "DatasetConfiguration", id= 0 }
            );

            routes.MapRoute(
               name: "New Configuration",
               url: "Dataset/Detail/{id}/Configuration/Create",
               defaults: new { controller = "Dataset", action = "CreateDataFileConfig", id = 0 }
           );
        }
    } 
}