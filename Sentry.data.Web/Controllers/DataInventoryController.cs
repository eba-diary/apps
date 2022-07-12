using Sentry.Common.Logging;
using Sentry.data.Core;
using System.Collections.Generic;
using System.Web.Mvc;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.Controllers
{
    public class DataInventoryController : BaseSearchableController
    {
        private readonly IDataInventoryService _dataInventoryService;
        private readonly IFilterSearchService _filterSearchService;

        public DataInventoryController(IDataInventoryService dataInventoryService, IFilterSearchService filterSearchService)
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
            ValidateSearchModel(searchModel);

            DataInventorySearchResultDto resultDto = _dataInventoryService.GetSearchResults(searchModel.ToDto());

            List<int> visibleColumns = null;
            
            if (!string.IsNullOrEmpty(searchModel.SearchName))
            {
                try
                {
                    SavedSearchDto savedSearchDto = _filterSearchService.GetSavedSearch(SearchType.DATA_INVENTORY, searchModel.SearchName, SharedContext.CurrentUser.AssociateId);
                    if (savedSearchDto.ResultConfiguration != null)
                    {
                        visibleColumns = savedSearchDto.ResultConfiguration["VisibleColumns"].ToObject<List<int>>();
                    }
                }
                catch (System.Exception ex)
                {
                    Logger.Error("Error getting visible columns for saved search", ex);
                }
            }

            return Json(new { 
                data = resultDto.DataInventoryResults.ToWeb(),
                searchTotal = resultDto.SearchTotal,
                visibleColumns
            });
        }

        [HttpPost]
        public JsonResult SearchFilters(FilterSearchModel searchModel)
        {
            ValidateSearchModel(searchModel);
            FilterSearchModel filterResult = _dataInventoryService.GetSearchFilters(searchModel.ToDto()).ToModel();
            
            if (!CanViewSensitive())
            {
                RemoveSensitive(filterResult);
            }

            return Json(filterResult.FilterCategories);
        }

        [HttpPost]
        public ActionResult Update(List<DataInventoryUpdateModel> models)
        {
            bool result = _dataInventoryService.UpdateIsSensitive(models.ToDto());
            return Json(new { success = result });
        }

        [HttpGet]
        public JsonResult GetDataInventoryAccess()
        {
            return Json(new
            {
                canEditSensitive = SharedContext.CurrentUser.CanEditSensitiveDataInventory || SharedContext.CurrentUser.IsAdmin,
                canEditOwnerVerified = SharedContext.CurrentUser.CanEditOwnerVerifiedDataInventory || SharedContext.CurrentUser.IsAdmin,
                canViewSensitive = CanViewSensitive()
            }, JsonRequestBehavior.AllowGet);
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

            return GetFilterSearchView(model);
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

        protected bool CanViewSensitive()
        {
            return SharedContext.CurrentUser.CanViewSensitiveDataInventory;
        }
        #endregion
    }
}