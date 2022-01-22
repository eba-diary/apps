using Sentry.data.Core;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

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
            searchModel.FilterCategories = new List<FilterCategoryModel>()
            {
                new FilterCategoryModel()
                {
                    CategoryName = "Environment",
                    CategoryOptions = new List<FilterCategoryOptionModel>()
                    {
                        new FilterCategoryOptionModel()
                        {
                            OptionValue = "Prod",
                            Selected = true,
                            ParentCategoryName = "Environment"
                        }
                    }
                }
            };

            if (!CanViewSensitive())
            {
                searchModel.FilterCategories.Add(new FilterCategoryModel()
                {
                    CategoryName = "Sensitivity",
                    CategoryOptions = new List<FilterCategoryOptionModel>()
                    {
                        new FilterCategoryOptionModel()
                        {
                            OptionValue = "Public",
                            Selected = true,
                            ParentCategoryName = "Sensitivity"
                        }
                    }
                });
            }

            return View("~/Views/Search/FilterSearch.cshtml", searchModel);
        }

        [HttpPost]
        public JsonResult SearchResult(FilterSearchModel searchModel)
        {
            List<DaleResultRowModel> results = new List<DaleResultRowModel>();

            CleanFilters(searchModel.FilterCategories);

            //there is search text or a filter to search on
            if (!string.IsNullOrWhiteSpace(searchModel.SearchText) || searchModel.FilterCategories?.Any(x => x.CategoryOptions?.Any(o => o.Selected) == true) == true)
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
            //aggregation still needs to be run based on

            searchModel.FilterCategories = new List<FilterCategoryModel>()
            {
                new FilterCategoryModel()
                {
                    CategoryName = "Sensitivity",
                    CategoryOptions = new List<FilterCategoryOptionModel>()
                    {
                        new FilterCategoryOptionModel()
                        {
                            OptionValue = "Sensitive",
                            ResultCount = 1,
                            ParentCategoryName = "Sensitivity"
                        },
                        new FilterCategoryOptionModel()
                        {
                            OptionValue = "Public",
                            ResultCount = 12,
                            ParentCategoryName = "Sensitivity"
                        }
                    }
                },
                new FilterCategoryModel()
                {
                    CategoryName = "Environment",
                    CategoryOptions = new List<FilterCategoryOptionModel>()
                    {
                        new FilterCategoryOptionModel()
                        {
                            OptionValue = "Prod",
                            ResultCount = 3,
                            Selected = true,
                            ParentCategoryName = "Environment"
                        },
                        new FilterCategoryOptionModel()
                        {
                            OptionValue = "NonProd",
                            ResultCount = 6,
                            Selected = false,
                            ParentCategoryName = "Environment"
                        }
                    }
                }
            };

            CleanFilters(searchModel.FilterCategories);

            return PartialView("~/Views/Search/FilterCategories.cshtml", searchModel.FilterCategories);
        }

        private void CleanFilters(List<FilterCategoryModel> categories)
        {
            if (!CanViewSensitive() && categories?.Any(c => c.CategoryName == "Sensitivity") == true)
            {
                categories.RemoveAll(c => c.CategoryName == "Sensitivity");
            }
        }
    }
}