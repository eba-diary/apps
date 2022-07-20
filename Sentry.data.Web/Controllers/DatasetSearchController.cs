using Sentry.data.Core;
using Sentry.data.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Mvc;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.Controllers
{
    public class DatasetSearchController : BaseSearchableController
    {
        private readonly IDatasetService _datasetService;

        public DatasetSearchController(IFilterSearchService filterSearchService, IDatasetService datasetService) : base(filterSearchService)
        {
            _datasetService = datasetService;
        }

        //[Route("Search/Dataset")]
        //[Route("Dataset/Search")]
        public ActionResult Search(string savedSearch = null)
        {
            //validate user has permissions
            if (!SharedContext.CurrentUser.CanViewDataset)
            {
                return View("Forbidden");
            }

            if (TryGetSavedSearch(SearchType.DATASET_SEARCH, savedSearch, out ActionResult view))
            {
                return view;
            }

            //need to build filter categories
            //return GetFilterSearchView(model);

            //code below will move to Results() when transitioned into FilterSearch page
            //only using for testing
            TileResultsModel tileResultsModel = new TileResultsModel()
            {
                PageSizeOptions = Utility.BuildTilePageSizeOptions(),
                SortByOptions = Utility.BuildDatasetSortByOptions()
            };

            DatasetSearchModel datasetSearchModel = new DatasetSearchModel()
            {
                PageNumber = 1,
                PageSize = 10,
                SortBy = 1
            };

            DatasetSearchDto datasetSearchDto = datasetSearchModel.ToDto();

            List<DatasetTileDto> dtos = _datasetService.SearchDatasets(datasetSearchDto);

            tileResultsModel.Tiles = dtos.ToModel();

            return View("~/Views/Search/TileResults.cshtml", tileResultsModel);
        }

        public override ActionResult Results()
        {
            //will return the TileResults partial view
            throw new NotImplementedException();
        }

        protected override FilterSearchConfigModel GetFilterSearchConfigModel(FilterSearchModel searchModel)
        {
            return new FilterSearchConfigModel()
            {
                PageTitle = "Dataset",
                SearchType = SearchType.DATASET_SEARCH,
                IconPath = "~/Images/Icons/DatasetsBlue.svg",
                ResultView = "~/Views/Search/TileResults.cshtml",
                DefaultSearch = searchModel
            };
        }
    }
}