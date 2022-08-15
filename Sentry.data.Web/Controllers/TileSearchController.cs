using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Web.Helpers;
using System.Collections.Generic;
using System.Web.Mvc;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.Controllers
{
    public abstract class TileSearchController : BaseSearchableController
    {
        protected TileSearchController(IFilterSearchService filterSearchService) : base(filterSearchService) { }

        public ActionResult Search(string searchText = null, int sortBy = 0, int pageNumber = 1, int pageSize = 15, int layout = 0, List<string> filters = null, string savedSearch = null)
        {
            if (HasPermission())
            {
                if (TryGetSavedSearch(GetSearchType(), savedSearch, out ActionResult view))
                {
                    return view;
                }

                FilterSearchModel model = new FilterSearchModel()
                {
                    SearchText = searchText
                };

                Dictionary<string, string> resultParameters = new Dictionary<string, string>()
                {
                    { TileResultParameters.SORTBY, sortBy.ToString() },
                    { TileResultParameters.PAGENUMBER, pageNumber.ToString() },
                    { TileResultParameters.PAGESIZE, pageSize.ToString() },
                    { TileResultParameters.LAYOUT, layout.ToString() }
                };

                return GetFilterSearchView(model, resultParameters);
            }

            return View("Forbidden");
        }

        [ChildActionOnly]
        public override ActionResult Results(Dictionary<string, string> parameters)
        {
            TileResultsModel tileResultsModel = new TileResultsModel()
            {
                Tiles = new List<TileModel>(),
                PageItems = new List<PageItemModel>() 
                { 
                    new PageItemModel() 
                    { 
                        IsActive = true,
                        PageNumber = parameters[TileResultParameters.PAGENUMBER]
                    }
                },
                PageSizeOptions = Utility.BuildTilePageSizeOptions(parameters[TileResultParameters.PAGESIZE]),
                SortByOptions = Utility.BuildSelectListFromEnum<TileSearchSortByOption>(int.Parse(parameters[TileResultParameters.SORTBY])),
                LayoutOptions = Utility.BuildSelectListFromEnum<LayoutOption>(int.Parse(parameters[TileResultParameters.LAYOUT]))
            };

            return PartialView("~/Views/Search/TileResults.cshtml", tileResultsModel);
        }

        protected abstract bool HasPermission();
        protected abstract string GetSearchType();
    }
}