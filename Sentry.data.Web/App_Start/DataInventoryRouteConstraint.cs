using Sentry.data.Core;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Sentry.data.Web
{
    public class DataInventoryRouteConstraint : IRouteConstraint
    {
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (string.Equals(DependencyResolver.Current.GetService<IDataFeatures>().CLA3707_DataInventorySource.GetValue(), "ELASTIC", StringComparison.OrdinalIgnoreCase))
            {
                values["controller"] = "DataInventory";
                values["action"] = "Search";
            }

            return true;
        }
    }
}