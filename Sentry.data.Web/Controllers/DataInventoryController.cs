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

        public ActionResult Search(FilterSearchModel searchModel)
        {
            //this is the view setup and initial filters for default search
            searchModel.PageTitle = "Data Inventory";
            searchModel.IconPath = "~/Images/Dale/DataInventoryIcon.png";
            searchModel.ResultView = "SearchResult";
            searchModel.FilterCategories.Add(new FilterCategoryModel()
            {
                CategoryName = FilterCategoryNames.ENVIRONMENT,
                CategoryOptions = new List<FilterCategoryOptionModel>()
                    {
                        new FilterCategoryOptionModel()
                        {
                            OptionValue = "Prod",
                            Selected = true,
                            ParentCategoryName = FilterCategoryNames.ENVIRONMENT
                        }
                    }
            });

            return View("~/Views/Search/FilterSearch.cshtml", searchModel);
        }

        [HttpPost]
        public JsonResult SearchResult(FilterSearchModel searchModel)
        {
            List<DaleResultRowModel> results = new List<DaleResultRowModel>();

            if (searchModel.IsValid(CanViewSensitive()))
            {
                results = _service.GetSearchResults(searchModel.ToDto()).DaleResults.Select(x => x.ToWeb()).ToList();
            }

            JsonResult result = Json(new { data = results });
            result.MaxJsonLength = int.MaxValue;
            return result;
        }

        [HttpPost]
        public ActionResult FilterCategories(FilterSearchModel searchModel)
        {
            if (searchModel.IsValid(CanViewSensitive()))
            {
                searchModel.FilterCategories = _service.GetSearchFilters(searchModel.ToDto()).FilterCategories.Select(x => x.ToModel()).ToList();
            }

            return PartialView("~/Views/Search/FilterCategories.cshtml", searchModel.FilterCategories);
        }
    }
}