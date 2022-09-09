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
            if (DependencyResolver.Current.GetService<IDataFeatures>().CLA3756_UpdateSearchPages.GetValue())
            {
                if (string.Equals(values["searchType"].ToString(), GlobalConstants.SearchType.DATASET_SEARCH, StringComparison.OrdinalIgnoreCase))
                {
                    values["controller"] = "DatasetSearch";
                    values["action"] = "Search";
                }
                //else if (string.Equals(values["searchType"].ToString(), GlobalConstants.SearchType.BUSINESS_INTELLIGENCE_SEARCH, StringComparison.OrdinalIgnoreCase))
                //{
                //    values["controller"] = "BusinessIntelligenceSearch";
                //    values["action"] = "Search";
                //}
            }

            return true;
        }
    }
}