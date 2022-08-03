using Sentry.data.Core;
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
        public abstract ActionResult Results();
        protected abstract FilterSearchConfigModel GetFilterSearchConfigModel(FilterSearchModel searchModel);

        protected bool TryGetSavedSearch(string searchType, string savedSearchName, out ActionResult actionResult)
{
            if (!string.IsNullOrEmpty(savedSearchName))
{
                SavedSearchDto savedSearchDto = _filterSearchService.GetSavedSearch(searchType, savedSearchName, SharedContext.CurrentUser.AssociateId);
                if (savedSearchDto != null)
                {
                    actionResult = GetFilterSearchView(savedSearchDto.ToModel());
                    return true;
                }
            }

            actionResult = null;
            return false;
        }

        protected ActionResult GetFilterSearchView(FilterSearchModel searchModel)
        {
            FilterSearchConfigModel configModel = GetFilterSearchConfigModel(searchModel);
            return View("~/Views/Search/FilterSearch.cshtml", configModel);
        }
    }
}