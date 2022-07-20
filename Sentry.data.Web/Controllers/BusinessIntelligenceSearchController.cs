using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.Controllers
{
    public class BusinessIntelligenceSearchController : BaseSearchableController
    {
        public BusinessIntelligenceSearchController(IFilterSearchService filterSearchService) : base(filterSearchService)
        {
            
        }

        //[Route("Search/BusinessIntelligence")]
        //[Route("BusinessIntelligence/Search")]
        //public ActionResult Search()
        //{
        //    //validate user has permissions
        //    if (!SharedContext.CurrentUser.CanViewReports)
        //    {
        //        return View("Forbidden");
        //    }

        //    ViewBag.Title = "Business Intelligence";
        //    SearchIndexModel model = new SearchIndexModel()
        //    {
        //        SearchType = GlobalConstants.SearchType.BUSINESS_INTELLIGENCE_SEARCH
        //    };

        //    return View(model);
        //}

        public override ActionResult Results()
        {
            throw new NotImplementedException();
        }

        protected override FilterSearchConfigModel GetFilterSearchConfigModel(FilterSearchModel searchModel)
        {
            return new FilterSearchConfigModel()
            {
                PageTitle = "Business Intelligence",
                SearchType = SearchType.BUSINESS_INTELLIGENCE_SEARCH,
                IconPath = "~/Images/Icons/Business IntelligenceBlue.svg",
                ResultView = "~/Views/Search/TileResults.cshtml",
                DefaultSearch = searchModel
            };
        }
    }
}