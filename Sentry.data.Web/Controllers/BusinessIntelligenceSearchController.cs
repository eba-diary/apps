using Sentry.data.Core;
using System;
using System.Collections.Generic;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.Controllers
{
    public class BusinessIntelligenceSearchController : TileSearchController<BusinessIntelligenceTileDto>
    {
        public BusinessIntelligenceSearchController(ITileSearchService<BusinessIntelligenceTileDto> tileSearchService, IEventService eventService, IFilterSearchService filterSearchService) : base(tileSearchService, eventService, filterSearchService) { }

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

        protected override bool HasPermission()
        {
            return SharedContext.CurrentUser.CanViewReports;
        }

        protected override string GetSearchType()
        {
            return SearchType.BUSINESS_INTELLIGENCE_SEARCH;
        }

        protected override List<BusinessIntelligenceTileDto> MapToTileDtos(List<TileModel> tileModels)
        {
            return tileModels.ToBusinessIntelligenceTileDtos();
        }

        protected override List<TileModel> MapToTileModels(List<BusinessIntelligenceTileDto> tileDtos)
        {
            return tileDtos.ToModels();
        }
    }
}