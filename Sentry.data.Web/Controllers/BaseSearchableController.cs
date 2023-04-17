using Sentry.data.Core;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Sentry.data.Web.Controllers
{
    public abstract class BaseSearchableController : BaseController
    {
        protected readonly IFilterSearchService _filterSearchService;

        protected BaseSearchableController(IFilterSearchService filterSearchService)
        {
            _filterSearchService = filterSearchService;
        }

        [ChildActionOnly]
        public abstract ActionResult Results(Dictionary<string, string> parameters);
        protected abstract FilterSearchConfigModel GetFilterSearchConfigModel(FilterSearchModel searchModel);

        protected bool TryGetSavedSearch(string searchType, string savedSearchName, out SavedSearchDto savedSearchDto)
        {
            if (!string.IsNullOrEmpty(savedSearchName))
            {
                savedSearchDto = _filterSearchService.GetSavedSearch(searchType, savedSearchName, SharedContext.CurrentUser.AssociateId);
                return savedSearchDto != null;
            }

            savedSearchDto = null;
            return false;
        }

        protected ActionResult GetFilterSearchView(FilterSearchModel searchModel, Dictionary<string, string> resultParameters)
        {
            FilterSearchConfigModel configModel = GetFilterSearchConfigModel(searchModel);
            configModel.ResultParameters = resultParameters;
            return View("~/Views/Search/FilterSearch.cshtml", configModel);
        }
    }
}