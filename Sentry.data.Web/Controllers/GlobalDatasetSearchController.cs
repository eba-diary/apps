using AutoMapper;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.Controllers
{
    public class GlobalDatasetSearchController : BaseSearchableController
    {
        private readonly IGlobalDatasetSearchService _globalDatasetSearchService;
        private readonly IMapper _mapper;

        public GlobalDatasetSearchController(IGlobalDatasetSearchService globalDatasetSearchService, IMapper mapper, IFilterSearchService filterSearchService) : base(filterSearchService)
        {
            _globalDatasetSearchService = globalDatasetSearchService;
            _mapper = mapper;
        }

        public ActionResult Search(string searchText = null, int sortBy = -1, int pageNumber = 1, int pageSize = 12, int layout = 0, List<string> filters = null, string savedSearch = null)
        {
            if (TryGetSavedSearch(SearchType.GLOBAL_DATASET, savedSearch, out SavedSearchDto savedSearchDto))
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

            //only set sort if it wasn't specified on request
            if (sortBy == -1)
            {
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    //set to relevance because a text search will be executed
                    sortBy = (int)GlobalDatasetSortByOption.Relevance;
                }
                else
                {
                    //default to favorites otherwise
                    sortBy = (int)GlobalDatasetSortByOption.Favorites;
                }
            }

            FilterSearchModel model = new FilterSearchModel()
            {
                SearchName = savedSearch,
                SearchText = searchText,
                FilterCategories = _globalDatasetSearchService.GetInitialFilters(filters).ToModels()
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
            GlobalDatasetResultsViewModel resultsViewModel = new GlobalDatasetResultsViewModel()
            {
                GlobalDatasets = new List<GlobalDatasetViewModel>(),
                PageItems = new List<PageItemModel>()
                {
                    new PageItemModel()
                    {
                        IsActive = true,
                        PageNumber = parameters[TileResultParameters.PAGENUMBER]
                    }
                },
                PageSizeOptions = Utility.BuildTilePageSizeOptions(parameters[TileResultParameters.PAGESIZE]),
                SortByOptions = Utility.BuildSelectListFromEnum<GlobalDatasetSortByOption>(int.Parse(parameters[TileResultParameters.SORTBY])),
                LayoutOptions = Utility.BuildSelectListFromEnum<LayoutOption>(int.Parse(parameters[TileResultParameters.LAYOUT]))
            };

            return PartialView("GlobalDatasetResults", resultsViewModel);
        }

        [HttpPost]
        public ActionResult GlobalDatasetResults(GlobalDatasetPageRequestViewModel resultsRequestModel)
        {
            GlobalDatasetPageRequestDto requestDto = _mapper.Map<GlobalDatasetPageRequestDto>(resultsRequestModel);

            GlobalDatasetPageResultDto resultDto = _globalDatasetSearchService.SetGlobalDatasetPageResults(requestDto);

            GlobalDatasetResultsViewModel resultsViewModel = _mapper.Map<GlobalDatasetResultsViewModel>(resultDto);

            return PartialView(resultsViewModel);
        }

        protected override FilterSearchConfigModel GetFilterSearchConfigModel(FilterSearchModel searchModel)
        {
            return new FilterSearchConfigModel()
            {
                PageTitle = "Datasets",
                SearchType = SearchType.GLOBAL_DATASET,
                DefaultSearch = searchModel
            };
        }
    }
}