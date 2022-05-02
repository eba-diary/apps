using Sentry.data.Core;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.Controllers
{
    public class DataInventoryController : BaseDataInventoryController
    {
        private readonly IDaleService _dataInventoryService;
        private readonly IFilterSearchService _filterSearchService;

        public DataInventoryController(IDaleService dataInventoryService, IFilterSearchService filterSearchService, IDataFeatures featureFlags) : base(featureFlags)
        {
            _dataInventoryService = dataInventoryService;
            _filterSearchService = filterSearchService;
        }
        
        public ActionResult Search(string target = null, string search = null, string savedSearch = null)
        {
            if (!string.IsNullOrEmpty(savedSearch))
            {
                SavedSearchDto savedSearchDto = _filterSearchService.GetSavedSearch(SearchType.DATA_INVENTORY, savedSearch, SharedContext.CurrentUser.AssociateId);
                if (savedSearchDto != null)
                {
                    return GetView(savedSearchDto.ToModel());
                }
            }
            
            FilterSearchModel model = BuildBaseSearchModel();

            //ability to deep link with a filter, link from SAID passes in asset and code
            if (!string.IsNullOrWhiteSpace(target) && !string.IsNullOrWhiteSpace(search) && _dataInventoryService.TryGetCategoryName(target, out string categoryName))
            {
                model.FilterCategories.Add(new FilterCategoryModel()
                {
                    CategoryName = categoryName,
                    CategoryOptions = new List<FilterCategoryOptionModel>()
                    {
                        new FilterCategoryOptionModel()
                        {
                            OptionValue = search,
                            ParentCategoryName = categoryName,
                            Selected = true
                        }
                    }
                });
            }
            
            return GetView(model);
        }

        [HttpPost]
        public JsonResult SearchResult(FilterSearchModel searchModel)
        {
            //FOR COLUMN SAVE: add SearchName parameter as optional

            ValidateSearchModel(searchModel);

            //FOR COLUMN SAVE: update GetSearchResults to be async
            //Will add additional call to get saved search ResultConfiguration using SearchName if populated async
            //pass back the ResultConfiguration as visible columns

            DaleResultDto resultDto = _dataInventoryService.GetSearchResults(searchModel.ToDaleDto());

            return Json(new { 
                data = resultDto.DaleResults.Select(x => x.ToWeb()).ToList(),
                searchTotal = resultDto.SearchTotal
            });
        }

        [HttpPost]
        public JsonResult SearchFilters(FilterSearchModel searchModel)
        {
            ValidateSearchModel(searchModel);
            FilterSearchModel filterResult = _dataInventoryService.GetSearchFilters(searchModel.ToDaleDto()).ToModel();
            
            if (!CanViewSensitive())
            {
                RemoveSensitive(filterResult);
            }

            return Json(filterResult.FilterCategories);
        }

        [HttpPost]
        public ActionResult Update(List<DaleSensitiveModel> models)
        {
            return Json(new { success = _dataInventoryService.UpdateIsSensitive(models.ToDto()) });
        }

        #region Methods
        private ActionResult GetView(FilterSearchModel searchModel)
        {
            FilterSearchConfigModel model = new FilterSearchConfigModel()
            {
                PageTitle = "Data Inventory",
                SearchType = SearchType.DATA_INVENTORY,
                IconPath = "~/Images/DataInventory/DataInventoryIconBlue.svg",
                ResultView = "SearchResult",
                InfoLink = "https://confluence.sentry.com/display/CLA/Data+Inventory+-+Elastic",
                DefaultSearch = searchModel
            };

            return View("~/Views/Search/FilterSearch.cshtml", model);
        }

        private FilterSearchModel BuildBaseSearchModel()
        {
            return new FilterSearchModel()
            {
                FilterCategories = new List<FilterCategoryModel>()
                {
                    new FilterCategoryModel()
                    {
                        CategoryName = FilterCategoryNames.ENVIRONMENT,
                        CategoryOptions = new List<FilterCategoryOptionModel>()
                        {
                            new FilterCategoryOptionModel()
                            {
                                OptionValue = FilterCategoryOptions.ENVIRONMENT_PROD,
                                ParentCategoryName = FilterCategoryNames.ENVIRONMENT,
                                Selected = true
                            }
                        }
                    }
                }
            };
        }
        
        private void ValidateSearchModel(FilterSearchModel searchModel)
        {
            if (!CanViewSensitive())
            {
                RemoveSensitive(searchModel);

                FilterCategoryModel category = new FilterCategoryModel() { CategoryName = FilterCategoryNames.SENSITIVE };
                category.CategoryOptions.Add(new FilterCategoryOptionModel() { OptionValue = "false", Selected = true, ParentCategoryName = category.CategoryName });

                searchModel.FilterCategories.Add(category);
            }
        }

        private void RemoveSensitive(FilterSearchModel searchModel)
        {
            searchModel.FilterCategories.RemoveAll(x => x.CategoryName == FilterCategoryNames.SENSITIVE);
        }
        #endregion
    }
}