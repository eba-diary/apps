using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Web.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.Controllers
{
    public abstract class TileSearchController<T> : BaseSearchableController where T : IFilterSearchable
    {
        private readonly ITileSearchService _tileSearchService;

        protected TileSearchController(ITileSearchService tileSearchService, IFilterSearchService filterSearchService) : base(filterSearchService) 
        {
            _tileSearchService = tileSearchService;
        }

        public ActionResult Search(string searchText = null, int sortBy = 0, int pageNumber = 1, int pageSize = 15, int layout = 0, List<string> filters = null, string savedSearch = null)
        {
            if (HasPermission())
            {
                if (TryGetSavedSearch(GetSearchType(), savedSearch, out ActionResult view))
                {
                    return view;
                }

                FilterSearchModel model = new FilterSearchModel()
                {
                    SearchName = savedSearch,
                    SearchText = searchText,
                    FilterCategories = BuildInitialFilters(filters)
                };

                Dictionary<string, string> resultParameters = new Dictionary<string, string>()
                {
                    { TileResultParameters.SORTBY, sortBy.ToString() },
                    { TileResultParameters.PAGENUMBER, pageNumber.ToString() },
                    { TileResultParameters.PAGESIZE, pageSize.ToString() },
                    { TileResultParameters.LAYOUT, layout.ToString() }
                };

                return GetFilterSearchView(model, resultParameters);
            }

            return View("Forbidden");
        }

        [ChildActionOnly]
        public override ActionResult Results(Dictionary<string, string> parameters)
        {
            TileResultsModel tileResultsModel = new TileResultsModel()
            {
                Tiles = new List<TileModel>(),
                PageItems = new List<PageItemModel>() 
                { 
                    new PageItemModel() 
                    { 
                        IsActive = true,
                        PageNumber = parameters[TileResultParameters.PAGENUMBER]
                    }
                },
                PageSizeOptions = Utility.BuildTilePageSizeOptions(parameters[TileResultParameters.PAGESIZE]),
                SortByOptions = Utility.BuildSelectListFromEnum<TileSearchSortByOption>(int.Parse(parameters[TileResultParameters.SORTBY])),
                LayoutOptions = Utility.BuildSelectListFromEnum<LayoutOption>(int.Parse(parameters[TileResultParameters.LAYOUT]))
            };



            return PartialView("~/Views/Search/TileResults.cshtml", tileResultsModel);
        }

        [HttpPost]
        public JsonResult SearchableDatasets(TileSearchModel datasetSearchModel)
        {
            DatasetSearchDto datasetSearchDto = datasetSearchModel.ToDto();
            List<DatasetTileDto> datasetTileDtos = _tileSearchService.SearchDatasetTileDtos(datasetSearchDto).ToList();
            List<TileModel> tileModels = datasetTileDtos.ToModels();
            return Json(tileModels);
        }

        [HttpPost]
        public JsonResult TileResultsModel(TileSearchModel datasetSearchModel)
        {
            DatasetSearchDto datasetSearchDto = datasetSearchModel.ToDto();
            DatasetSearchResultDto resultDto = _tileSearchService.SearchDatasets(datasetSearchDto);
            TileResultsModel tileResultsModel = resultDto.ToModel(datasetSearchModel.SortBy, datasetSearchModel.PageNumber, datasetSearchModel.Layout);
            return Json(tileResultsModel);
        }

        [HttpPost]
        public JsonResult TileFilters(TileSearchModel datasetSearchModel)
        {
            DatasetSearchDto datasetSearchDto = datasetSearchModel.ToDto();
            List<FilterCategoryDto> filterCategoryDtos = datasetSearchDto.SearchableTiles.CreateFilters(datasetSearchDto.FilterCategories);
            List<FilterCategoryModel> filterCategoryModels = filterCategoryDtos.ToModels();

            return Json(filterCategoryModels);
        }

        #region Abstract
        protected abstract bool HasPermission();
        protected abstract string GetSearchType();
        #endregion

        #region Private
        private List<FilterCategoryModel> BuildInitialFilters(List<string> filters)
        {
            List<FilterCategoryModel> categories = new List<FilterCategoryModel>();

            if (filters != null)
            {
                foreach (string filter in filters)
                {
                    List<string> parts = filter.Split('_').ToList();
                    string category = parts.First();

                    FilterCategoryOptionModel optionModel = new FilterCategoryOptionModel()
                    {
                        OptionValue = HttpUtility.UrlDecode(parts.Last()),
                        ParentCategoryName = category,
                        Selected = true
                    };

                    FilterCategoryModel existingCategory = categories.FirstOrDefault(x => x.CategoryName == category);

                    if (existingCategory != null)
                    {
                        existingCategory.CategoryOptions.Add(optionModel);
                    }
                    else
                    {
                        FilterCategoryModel newCategory = new FilterCategoryModel() { CategoryName = category };
                        newCategory.CategoryOptions.Add(optionModel);
                        categories.Add(newCategory);
                    }
                }
            }

            return categories;
        }
        #endregion
    }
}