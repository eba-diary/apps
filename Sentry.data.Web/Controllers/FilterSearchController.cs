using Sentry.Common.Logging;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Sentry.data.Web.Controllers
{
    public class FilterSearchController : BaseController
    {
        private readonly IFilterSearchService _filterSearchService;

        public FilterSearchController(IFilterSearchService filterSearchService)
        {
            _filterSearchService = filterSearchService;
        }

        [HttpPost]
        public ActionResult FilterCategories(List<FilterCategoryModel> filterCategories)
        {
            return PartialView("~/Views/Search/FilterCategories.cshtml", filterCategories ?? new List<FilterCategoryModel>());
        }

        [HttpPost]
        public ActionResult FilterShowAll(List<FilterCategoryModel> filterCategories)
        {
            return PartialView("~/Views/Search/FilterShowAll.cshtml", filterCategories ?? new List<FilterCategoryModel>());
        }

        [HttpGet]
        public ActionResult SavedSearches(string searchType, string activeSearchName)
        {
            List<SavedSearchOptionDto> savedSearchOptions = _filterSearchService.GetSavedSearchOptions(searchType, SharedContext.CurrentUser.AssociateId);
            
            SavedSearchDropdownModel model = new SavedSearchDropdownModel()
            {
                SearchType = searchType,
                SavedSearchOptions = savedSearchOptions.Select(x => x.ToModel()).ToList(),
                ActiveSearchName = activeSearchName
            };

            return PartialView("~/Views/Search/SavedSearches.cshtml", model);
        }

        [HttpPost]
        public JsonResult SaveSearch(SaveSearchModel searchModel)
        {
            SavedSearchDto savedSearchDto = searchModel.ToDto();
            savedSearchDto.AssociateId = SharedContext.CurrentUser.AssociateId;

            string result = _filterSearchService.SaveSearch(savedSearchDto);

            return Json(new { Result = result });
        }

        [HttpDelete]
        public JsonResult RemoveSearch(int savedSearchId)
        {
            _filterSearchService.RemoveSavedSearch(savedSearchId, SharedContext.CurrentUser.AssociateId);
            return Json(new { Success = true });
        }
    }
}