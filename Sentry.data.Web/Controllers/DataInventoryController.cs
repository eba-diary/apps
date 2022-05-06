using Sentry.Common.Logging;
using Sentry.data.Core;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.Controllers
{
    public class DataInventoryController : BaseController
    {
        private readonly IDataInventoryService _dataInventoryService;
        private readonly IFilterSearchService _filterSearchService;
        private readonly IDataFeatures _featureFlags;

        public DataInventoryController(IDataInventoryService dataInventoryService, IFilterSearchService filterSearchService, IDataFeatures featureFlags)
        {
            _dataInventoryService = dataInventoryService;
            _filterSearchService = filterSearchService;
            _featureFlags = featureFlags;
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
        public ActionResult Update(List<DataInventorySensitiveUpdateModel> models)
        {
            bool result = _dataInventoryService.UpdateIsSensitive(models.ToDto());
            return Json(new { success = result });
        }

        [HttpGet]
        public JsonResult GetCanDaleSensitive()
        {
            return Json(new
            {
                canDaleSensitiveEdit = SharedContext.CurrentUser.CanDaleSensitiveEdit || SharedContext.CurrentUser.IsAdmin,
                canDaleOwnerVerifiedEdit = (_featureFlags.Dale_Expose_EditOwnerVerified_CLA_1911.GetValue() && SharedContext.CurrentUser.CanDaleOwnerVerifiedEdit) || SharedContext.CurrentUser.IsAdmin,
                canDaleSensitiveView = CanViewSensitive()
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

        protected bool CanViewSensitive()
        {
            return SharedContext.CurrentUser.CanDaleSensitiveView;
        }
        #endregion
    }
}