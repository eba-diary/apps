using Sentry.data.Core;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.Controllers
{
    public class DatasetSearchController : TileSearchController
    {
        private readonly ITileSearchService _tileSearchService;

        public DatasetSearchController(ITileSearchService tileSearchService, IFilterSearchService filterSearchService) : base(tileSearchService, filterSearchService)
        {
            _tileSearchService = tileSearchService;
        }

        [HttpPost]
        public ActionResult TileResults(TileResultsModel tileResultsModel)
        {
            return PartialView("~/Views/Search/TileResults.cshtml", tileResultsModel);
        }

        protected override FilterSearchConfigModel GetFilterSearchConfigModel(FilterSearchModel searchModel)
        {
            return new FilterSearchConfigModel()
            {
                PageTitle = "Datasets",
                SearchType = SearchType.DATASET_SEARCH,
                IconPath = "~/Images/Icons/DatasetsBlue.svg",
                DefaultSearch = searchModel
            };
        }

        protected override bool HasPermission()
        {
            return SharedContext.CurrentUser.CanViewDataset;
        }

        protected override string GetSearchType()
        {
            return SearchType.DATASET_SEARCH;
        }
    }
}