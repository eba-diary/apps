using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Web.Helpers;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Sentry.data.Web.Controllers
{
    public abstract class TileSearchController : BaseSearchableController
    {
        protected TileSearchController(IFilterSearchService filterSearchService) : base(filterSearchService) { }

        [ChildActionOnly]
        public override ActionResult Results()
        {
            TileResultsModel tileResultsModel = new TileResultsModel()
            {
                PageSizeOptions = Utility.BuildTilePageSizeOptions("10"),
                SortByOptions = Utility.BuildDatasetSortByOptions(),
                Tiles = new List<TileModel>(),
                PageItems = new List<PageItemModel>(),
                LayoutOptions = Utility.BuildSelectListFromEnum<LayoutOption>(0)
            };

            return PartialView("~/Views/Search/TileResults.cshtml", tileResultsModel);
        }

        protected ActionResult GetBaseTileSearch(string searchType, bool permission, string savedSearch)
        {
            //validate user has permissions
            if (!permission)
            {
                return View("Forbidden");
            }

            if (TryGetSavedSearch(searchType, savedSearch, out ActionResult view))
            {
                return view;
            }

            return GetFilterSearchView(new FilterSearchModel());
        }
    }
}