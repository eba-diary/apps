using Nest;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
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
                PageItems = new List<PageItemModel>(),
                LayoutOptions = Utility.BuildSelectListFromEnum<LayoutOption>(0)
            };

            return PartialView("~/Views/Search/TileResults.cshtml", tileResultsModel);
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
            List<FilterCategoryModel> filterCategoryModels = filterCategoryDtos.ToModels(new List<string>() { "*" });

            return Json(filterCategoryModels);
        }

        [HttpPost]
        public JsonResult RefreshFilters(FilterCategoriesRefreshModel refreshModel)
        {
            foreach (FilterCategoryModel filterCategory in refreshModel.CurrentFilterCategories)
            {
                FilterCategoryModel resultCategory = refreshModel.ResultFilterCategories.FirstOrDefault(x => x.CategoryName == filterCategory.CategoryName);
                bool hasSelectedOption = resultCategory?.CategoryOptions.Any(x => x.Selected) == true;

                foreach (FilterCategoryOptionModel categoryOption in filterCategory.CategoryOptions)
                {
                    FilterCategoryOptionModel resultCategoryOption = resultCategory?.CategoryOptions?.FirstOrDefault(x => x.OptionValue == categoryOption.OptionValue);
                    if (resultCategoryOption != null)
                    {
                        categoryOption.ResultCount = resultCategoryOption.ResultCount;
                        categoryOption.Selected = resultCategoryOption.Selected;
                    }
                    else if (!hasSelectedOption)
                    {
                        categoryOption.ResultCount = 0;
                    }
                }
            }

            return Json(refreshModel.CurrentFilterCategories);
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