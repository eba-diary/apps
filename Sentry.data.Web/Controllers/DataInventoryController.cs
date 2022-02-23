using Sentry.data.Core;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.Controllers
{
    public class DataInventoryController : BaseDataInventoryController
    {
        private readonly IDaleService _service;

        public DataInventoryController(IDaleService service, IDataFeatures featureFlags) : base(featureFlags)
        {
            _service = service;
        }

        public ActionResult Search()
        {
            FilterSearchConfigModel model = new FilterSearchConfigModel()
            {
                PageTitle = "Data Inventory",
                IconPath = "~/Images/Dale/DataInventoryIcon.png",
                ResultView = "SearchResult",
                InfoLink = "https://confluence.sentry.com/display/CLA/Data+Inventory+-+Elastic",
                DefaultSearch = new FilterSearchModel()
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
                                    OptionValue = "P",
                                    ParentCategoryName = FilterCategoryNames.ENVIRONMENT,
                                    Selected = true
                                }
                            }
                        }
                    }
                }
            };

            return View("~/Views/Search/FilterSearch.cshtml", model);
        }

        [HttpPost]
        public JsonResult SearchResult(FilterSearchModel searchModel)
        {
            ValidateSearchModel(searchModel);

            DaleResultDto resultDto = _service.GetSearchResults(searchModel.ToDto());

            return Json(new { 
                data = resultDto.DaleResults.Select(x => x.ToWeb()).ToList(),
                searchTotal = resultDto.SearchTotal
            });
        }

        [HttpPost]
        public JsonResult SearchFilters(FilterSearchModel searchModel)
        {
            ValidateSearchModel(searchModel);
            FilterSearchModel filterResult = _service.GetSearchFilters(searchModel.ToDto()).ToModel();
            
            if (!CanViewSensitive())
            {
                RemoveSensitive(filterResult);
            }

            return Json(filterResult.FilterCategories);
        }

        [HttpPost]
        public ActionResult Update(List<DaleSensitiveModel> models)
        {
            return Json(new { success = _service.UpdateIsSensitive(models.ToDto()) });
        }

        #region Methods
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