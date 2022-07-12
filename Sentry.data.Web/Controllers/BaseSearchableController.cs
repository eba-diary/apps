using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sentry.data.Web.Controllers
{
    public abstract class BaseSearchableController : BaseController
    {
        private readonly IFilterSearchService _filterSearchService;

        protected BaseSearchableController(IFilterSearchService filterSearchService)
        {
            _filterSearchService = filterSearchService;
        }

        protected abstract ActionResult GetView(FilterSearchModel searchModel);

        protected bool TrySavedSearch(string searchType, string savedSearchName, out ActionResult actionResult)
{
            if (!string.IsNullOrEmpty(savedSearchName))
{
                SavedSearchDto savedSearchDto = _filterSearchService.GetSavedSearch(searchType, savedSearchName, SharedContext.CurrentUser.AssociateId);
                if (savedSearchDto != null)
                {
                    actionResult = GetView(savedSearchDto.ToModel());
                    return true;
                }
            }

            actionResult = null;
            return false;
        }

        protected ActionResult GetFilterSearchView(FilterSearchConfigModel configModel)
        {
            return View("~/Views/Search/FilterSearch.cshtml", configModel);
        }
    }
}