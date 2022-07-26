using Nest;
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
            TileResultsModel tileResultsModel = new TileResultsModel()
            {
                PageSizeOptions = Utility.BuildTilePageSizeOptions("10"),
                SortByOptions = Utility.BuildDatasetSortByOptions(),
                Tiles = new List<TileModel>(),
                PageItems = new List<PageItemModel>()
            };

            return PartialView("~/Views/Search/TileResults.cshtml", tileResultsModel);
        }

        [HttpPost]
        public JsonResult GetTileResultsModel(DatasetSearchModel datasetSearchModel)
        {
            DatasetSearchDto datasetSearchDto = datasetSearchModel.ToDto();
            DatasetSearchResultDto resultDto = _datasetService.SearchDatasets(datasetSearchDto);
            TileResultsModel tileModels = resultDto.ToModel(datasetSearchModel.SortBy);

            return Json(tileModels);
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
                PageTitle = "Dataset",
                SearchType = SearchType.DATASET_SEARCH,
                IconPath = "~/Images/Icons/DatasetsBlue.svg",
                DefaultSearch = searchModel
            };
        }
    }
}