using Sentry.data.Core;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.Controllers
{
    public class DatasetSearchController : TileSearchController
    {
        private readonly ITileSearchService<DatasetTileDto> _tileSearchService;

        public DatasetSearchController(ITileSearchService<DatasetTileDto> tileSearchService, IFilterSearchService filterSearchService) : base(filterSearchService)
        {
            _tileSearchService = tileSearchService;
        }

        [HttpPost]
        public JsonResult SearchableDatasets(TileSearchModel datasetSearchModel)
        {
            TileSearchDto<DatasetTileDto> datasetSearchDto = datasetSearchModel.ToDto();
            List<DatasetTileDto> datasetTileDtos = _tileSearchService.SearchDatasetTileDtos(datasetSearchDto).ToList();
            List<TileModel> tileModels = datasetTileDtos.ToModels();
            return Json(tileModels);
        }

        [HttpPost]
        public JsonResult TileResultsModel(TileSearchModel datasetSearchModel)
        {
            TileSearchDto<DatasetTileDto> datasetSearchDto = datasetSearchModel.ToDto();
            TileSearchResultDto<DatasetTileDto> resultDto = _tileSearchService.SearchDatasets(datasetSearchDto);
            TileResultsModel tileResultsModel = resultDto.ToModel(datasetSearchModel.SortBy, datasetSearchModel.PageNumber, datasetSearchModel.Layout);
            return Json(tileResultsModel);
        }

        [HttpPost]
        public JsonResult TileFilters(TileSearchModel datasetSearchModel)
        {
            TileSearchDto<DatasetTileDto> datasetSearchDto = datasetSearchModel.ToDto();
            List<FilterCategoryDto> filterCategoryDtos = datasetSearchDto.SearchableTiles.CreateFilters(datasetSearchDto.FilterCategories);
            List<FilterCategoryModel> filterCategoryModels = filterCategoryDtos.ToModels();

            return Json(filterCategoryModels);
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