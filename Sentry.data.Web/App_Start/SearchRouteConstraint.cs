using Sentry.data.Core;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Sentry.data.Web
{
    public class SearchRouteConstraint : IRouteConstraint
    {
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            IDataFeatures dataFeatures = DependencyResolver.Current.GetService<IDataFeatures>();

            if (string.Equals(values["searchType"].ToString(), GlobalConstants.SearchType.DATASET_SEARCH, StringComparison.OrdinalIgnoreCase))
            {
                if (dataFeatures.CLA4789_ImprovedSearchCapability.GetValue())
                {
                    values["controller"] = "GlobalDatasetSearch";
                    values["action"] = "Search";
                }
                else if (dataFeatures.CLA3756_UpdateSearchPages.GetValue())
                {
                    values["controller"] = "DatasetSearch";
                    values["action"] = "Search";
                }
            }                

            return true;
        }
    }
}