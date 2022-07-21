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

            FilterSearchModel model = new FilterSearchModel();

            return GetFilterSearchView(model);
        }

        [ChildActionOnly]
        public override ActionResult Results()
        {
            DatasetSearchModel datasetSearchModel = new DatasetSearchModel()
            {
                PageNumber = 1,
                PageSize = 10,
                SortBy = 1
            };

            TileResultsModel tileResultsModel = new TileResultsModel()
            {
                PageSizeOptions = Utility.BuildTilePageSizeOptions(),
                SortByOptions = Utility.BuildDatasetSortByOptions(),
                Tiles = GetTileModels(datasetSearchModel)
            };

            return PartialView("~/Views/Search/TileResults.cshtml", tileResultsModel);
        }

        protected override FilterSearchConfigModel GetFilterSearchConfigModel(FilterSearchModel searchModel)
        {
            return new FilterSearchConfigModel()
            {
                PageTitle = "Dataset",
                SearchType = SearchType.DATASET_SEARCH,
                IconPath = "~/Images/Icons/DatasetsBlue.svg",
                DefaultSearch = searchModel
            };
        }

        private List<TileModel> GetTileModels(DatasetSearchModel searchModel)
        {
            DatasetSearchDto datasetSearchDto = searchModel.ToDto();
            List<DatasetTileDto> dtos = _datasetService.SearchDatasets(datasetSearchDto);
            List<TileModel> models = dtos.ToModel();
            return models;
        }
    }
}