using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.Controllers
{
    public class BusinessIntelligenceSearchController : TileSearchController
    {
        public BusinessIntelligenceSearchController(IFilterSearchService filterSearchService) : base(filterSearchService)
        {
            
        }

        public ActionResult Search(string savedSearch = null)
        {
            return GetBaseTileSearch(SearchType.BUSINESS_INTELLIGENCE_SEARCH, SharedContext.CurrentUser.CanViewReports, savedSearch);
        }

        protected override FilterSearchConfigModel GetFilterSearchConfigModel(FilterSearchModel searchModel)
        {
            return new FilterSearchConfigModel()
            {
                PageTitle = "Business Intelligence",
                SearchType = SearchType.BUSINESS_INTELLIGENCE_SEARCH,
                IconPath = "~/Images/Icons/Business IntelligenceBlue.svg",
                DefaultSearch = searchModel
            };
        }
    }
}