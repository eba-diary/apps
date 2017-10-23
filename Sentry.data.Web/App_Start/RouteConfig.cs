using DoddleReport.Web;
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
                name: "DataAssetIndex",
                url: "DataAsset/Index",
                defaults: new { controller = "DataAsset", action = "Index", id = 0 }            
            );

            routes.MapRoute(
                name: "EditAssetNotification",
                url: "DataAsset/EditAssetNotification",
                defaults: new { controller = "DataAsset", action = "EditAssetNotification" }
            );

            routes.MapRoute(
                name: "DataAssetGetEditAssetNotificationPartialView",
                url: "DataAsset/GetEditAssetNotificationPartialView",
                defaults: new { controller = "DataAsset", action = "GetEditAssetNotificationPartialView" }
            );

            routes.MapRoute(
                name: "DataAssetManageAssetNotification",
                url: "DataAsset/ManageAssetNotification",
                defaults: new { controller = "DataAsset", action = "ManageAssetNotification" }
            );

            routes.MapRoute(
                name: "DataAssetGetAssetNotificationInfoForGrid",
                url: "DataAsset/GetAssetNotificationInfoForGrid",
                defaults: new { controller = "DataAsset", action = "GetAssetNotificationInfoForGrid" }
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
        }
    } 
}