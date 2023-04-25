using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Infrastructure.FeatureFlags;
using Sentry.data.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.Controllers
{
    public class GlobalDatasetSearchController : BaseSearchableController
    {
        private readonly IGlobalDatasetService _globalDatasetService;

        public GlobalDatasetSearchController(IGlobalDatasetService globalDatasetService, IFilterSearchService filterSearchService) : base(filterSearchService)
        {
            _globalDatasetService = globalDatasetService;
        }

        public ActionResult Search(string searchText = null, int sortBy = 0, int pageNumber = 1, int pageSize = 15, int layout = 0, List<string> filters = null, string savedSearch = null)
        {
            if (TryGetSavedSearch(SearchType.DATASET_SEARCH, savedSearch, out SavedSearchDto savedSearchDto))
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
                catch (Exception ex)
                {
                    Logger.Error("Error getting result parameters for saved search", ex);
                }
            }

            FilterSearchModel model = new FilterSearchModel()
            {
                SearchName = savedSearch,
                SearchText = searchText,
                FilterCategories = _globalDatasetService.GetInitialFilters(filters).ToModels()
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

            return PartialView("GlobalDatasetResults.cshtml", tileResultsModel);
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