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

        public DataInventoryController(IDataInventoryService dataInventoryService, IFilterSearchService filterSearchService) : base(filterSearchService)
        {
            _dataInventoryService = dataInventoryService;
        }
        
        public ActionResult Search(string target = null, string search = null, string savedSearch = null)
        {
            if (TryGetSavedSearch(SearchType.DATA_INVENTORY, savedSearch, out SavedSearchDto savedSearchDto))
            {
                return GetFilterSearchView(savedSearchDto.ToModel(), null);
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
            
            return GetFilterSearchView(model, null);
        }

        [ChildActionOnly]
        public override ActionResult Results(Dictionary<string, string> parameters)
        {
            return PartialView("SearchResult");
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

            JsonResult jsonResult = new JsonResult
            {
                Data = new
                {
                    data = resultDto.DataInventoryResults.ToWeb(),
                    searchTotal = resultDto.SearchTotal,
                    visibleColumns
                },
                ContentType = "application/json",
                JsonRequestBehavior = JsonRequestBehavior.DenyGet,
                MaxJsonLength = 10485760 //10MB
            };

            return jsonResult;
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
        protected override FilterSearchConfigModel GetFilterSearchConfigModel(FilterSearchModel searchModel)
        {
            return new FilterSearchConfigModel()
            {
                PageTitle = "Data Inventory",
                SearchType = SearchType.DATA_INVENTORY,
                DefaultSearch = searchModel
            };
        }

        private FilterSearchModel BuildBaseSearchModel()
        {
            return new FilterSearchModel()
            {
                FilterCategories = new List<FilterCategoryModel>()
                {
                    new FilterCategoryModel()
                    {
                        CategoryName = FilterCategoryNames.DataInventory.ENVIRONMENT,
                        CategoryOptions = new List<FilterCategoryOptionModel>()
                        {
                            new FilterCategoryOptionModel()
                            {
                                OptionValue = FilterCategoryOptions.ENVIRONMENT_PROD,
                                ParentCategoryName = FilterCategoryNames.DataInventory.ENVIRONMENT,
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

                FilterCategoryModel category = new FilterCategoryModel() { CategoryName = FilterCategoryNames.DataInventory.SENSITIVE };
                category.CategoryOptions.Add(new FilterCategoryOptionModel() { OptionValue = "false", Selected = true, ParentCategoryName = category.CategoryName });

                searchModel.FilterCategories.Add(category);
            }
        }

        private void RemoveSensitive(FilterSearchModel searchModel)
        {
            searchModel.FilterCategories.RemoveAll(x => x.CategoryName == FilterCategoryNames.DataInventory.SENSITIVE);
        }

        protected bool CanViewSensitive()
        {
            return SharedContext.CurrentUser.CanViewSensitiveDataInventory;
        }
        #endregion
    }
}