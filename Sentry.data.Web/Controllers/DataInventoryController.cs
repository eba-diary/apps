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
            searchModel.Validate(CanViewSensitive());

            DaleResultDto resultDto = _service.GetSearchResults(searchModel.ToDto());

            return Json(new { 
                data = resultDto.DaleResults.Select(x => x.ToWeb()).ToList(),
                searchTotal = resultDto.SearchTotal
            });
        }

        [HttpPost]
        public ActionResult FilterCategories(FilterSearchModel searchModel)
        {
            searchModel.Validate(CanViewSensitive());
            return PartialView("~/Views/Search/FilterCategories.cshtml", _service.GetSearchFilters(searchModel.ToDto()).ToModel().FilterCategories);
        }
    }
}