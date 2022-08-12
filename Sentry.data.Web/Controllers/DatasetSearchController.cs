using Sentry.data.Core;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.Controllers
{
    public class DatasetSearchController : TileSearchController
    {
        private readonly IDatasetService _datasetService;

        public DatasetSearchController(IFilterSearchService filterSearchService, IDatasetService datasetService) : base(filterSearchService)
        {
            _datasetService = datasetService;
        }

        public ActionResult Search(string savedSearch = null)
        {
            return GetBaseTileSearch(SearchType.DATASET_SEARCH, SharedContext.CurrentUser.CanViewDataset, savedSearch);
        }

        [HttpPost]
        public JsonResult SearchableDatasets(DatasetSearchModel datasetSearchModel)
        {
            DatasetSearchDto datasetSearchDto = datasetSearchModel.ToDto();
            List<DatasetTileDto> datasetTileDtos = _datasetService.SearchDatasetTileDtos(datasetSearchDto).ToList();
            List<TileModel> tileModels = datasetTileDtos.ToModels();
            return Json(tileModels);
        }

        [HttpPost]
        public JsonResult TileResultsModel(DatasetSearchModel datasetSearchModel)
        {
            DatasetSearchDto datasetSearchDto = datasetSearchModel.ToDto();
            DatasetSearchResultDto resultDto = _datasetService.SearchDatasets(datasetSearchDto);
            TileResultsModel tileResultsModel = resultDto.ToModel(datasetSearchModel.SortBy, datasetSearchModel.PageNumber, datasetSearchModel.Layout);
            return Json(tileResultsModel);
        }

        [HttpPost]
        public JsonResult TileFilters(DatasetSearchModel datasetSearchModel)
        {
            DatasetSearchDto datasetSearchDto = datasetSearchModel.ToDto();
            List<FilterCategoryDto> filterCategoryDtos = datasetSearchDto.SearchableTiles.CreateFilters(datasetSearchDto.FilterCategories);
            List<FilterCategoryModel> filterCategoryModels = filterCategoryDtos.ToModels();

            return Json(filterCategoryModels);
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
    }
}