﻿using Sentry.Common.Logging;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
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
            SavedSearchesModel model = new SavedSearchesModel()
            {
                SearchType = searchType,
                SavedSearchNames = _filterSearchService.GetSavedSearchNames(searchType, SharedContext.CurrentUser.AssociateId),
                ActiveSearchName = activeSearchName
            };

            return PartialView("~/Views/Search/SavedSearches.cshtml", model);
        }

        [HttpPost]
        public JsonResult SaveSearch(SaveSearchModel searchModel)
        {
            try
            {
                SavedSearchDto savedSearchDto = searchModel.ToDto();
                savedSearchDto.AssociateId = SharedContext.CurrentUser.AssociateId;
                
                _filterSearchService.SaveSearch(savedSearchDto);

                return Json(new { Success = true });
            }
            catch (Exception ex)
            {
                Logger.Error("Error saving search", ex);
                return Json(new { Success = false });
            }
        }
    }
}