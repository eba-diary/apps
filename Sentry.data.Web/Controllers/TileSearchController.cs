using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Web.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text.Json;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.Controllers
{
    public abstract class TileSearchController<T> : BaseSearchableController where T : DatasetTileDto
    {
        private readonly ITileSearchService<T> _tileSearchService;

        protected TileSearchController(ITileSearchService<T> tileSearchService, IFilterSearchService filterSearchService) : base(filterSearchService)
        {
            _tileSearchService = tileSearchService;
        }

        public ActionResult Search(string searchText = null, int sortBy = 0, int pageNumber = 1, int pageSize = 15, int layout = 0, List<string> filters = null, string savedSearch = null)
        {
            if (HasPermission())
            {
                if (TryGetSavedSearch(GetSearchType(), savedSearch, out SavedSearchDto savedSearchDto))
                {
                    try
                    {
                        Dictionary<string, string> savedParameters = null;
                        if (savedSearchDto.ResultConfiguration != null)
                        {
                            savedParameters = savedSearchDto.ResultConfiguration["ResultParameters"].ToObject<Dictionary<string, string>>();
                            savedParameters.Add(TileResultParameters.PAGENUMBER, "1");
                        }

                        return GetFilterSearchView(savedSearchDto.ToModel(), savedParameters);
                    }
                    catch (System.Exception ex)
                    {
                        Logger.Error("Error getting result parameters for saved search", ex);
                    }
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
        public JsonResult SearchableDatasets(TileSearchModel searchModel)
        {
            TileSearchDto<T> searchDto = MapToTileSearchDto(searchModel);
            List<T> tileDtos = _tileSearchService.SearchTileDtos(searchDto).ToList();
            List<TileModel> tileModels = MapToTileModels(tileDtos);
            return Json(tileModels);
        }

        [HttpPost]
        public JsonResult TileResultsModel(TileSearchModel searchModel)
        {
            TileSearchDto<T> searchDto = MapToTileSearchDto(searchModel);
            TileSearchResultDto<T> resultDto = _tileSearchService.SearchTiles(searchDto);
            TileResultsModel tileResultsModel = resultDto.ToModel(searchModel.SortBy, searchModel.PageNumber, searchModel.Layout);
            tileResultsModel.Tiles = MapToTileModels(resultDto.Tiles);

            _tileSearchService.PublishSearchEventAsync(searchModel.ToEventDto(tileResultsModel.TotalResults));

            return Json(tileResultsModel);
        }

        [HttpPost]
        public JsonResult TileFilters(TileSearchModel searchModel)
        {
            TileSearchDto<T> searchDto = MapToTileSearchDto(searchModel);
            List<FilterCategoryDto> filterCategoryDtos = searchDto.SearchableTiles.CreateFilters(searchDto.FilterCategories);
            List<FilterCategoryModel> filterCategoryModels = filterCategoryDtos.ToModels();

            return Json(filterCategoryModels);
        }

        [HttpPost]
        public ActionResult TileResults(TileResultsModel tileResultsModel)
        {
            return PartialView("~/Views/Search/TileResults.cshtml", tileResultsModel);
        }

        #region Abstract
        protected abstract bool HasPermission();
        protected abstract string GetSearchType();
        protected abstract List<T> MapToTileDtos(List<TileModel> tileModels);
        protected abstract List<TileModel> MapToTileModels(List<T> tileDtos);
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

        private TileSearchDto<T> MapToTileSearchDto(TileSearchModel searchModel)
        {
            TileSearchDto<T> searchDto = searchModel.ToDto<T>();
            searchDto.SearchableTiles = MapToTileDtos(searchModel.SearchableTiles);
            return searchDto;
        }
        #endregion
    }
}