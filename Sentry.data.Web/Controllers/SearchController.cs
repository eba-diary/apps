using Sentry.data.Core;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.SessionState;

namespace Sentry.data.Web.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class SearchController : BaseController
    {
        const int RECORDS_PER_PAGE = 2;

        private IDataAssetContext _domainContext;
        private UserService _userService;

        public SearchController(IDataAssetContext context, UserService userService)
        {
            _domainContext = context;
            _userService = userService;
        }

        // GET: Search
        [HttpGet()]
        [Route("Search/{SearchText?}")]
        [Route("Asset/")]
        public ActionResult SearchResults(SearchModel s)
        {
            if (s.SearchCategory.HasValue)
            {
                Category cat = _domainContext.GetById<Category>(s.SearchCategory.Value);
                s.Category = new FullCategoryModel(cat);
            }
            else
            {
                s.Category = new FullCategoryModel() { Name = "", SubCategories = _domainContext.Categories.WhereIsRoot().Select((c) => new FullCategoryModel(c, true)).ToList() };
            }
            return View(s);
        }

        [HttpGet()]
        [Route("Search/ResultList/{SearchText?}")]
        public ActionResult ResultList(SearchModel s)
        {
            System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(500));

            s.PageSize = RECORDS_PER_PAGE;
            s.StartRecordNumber = ((s.SearchPage - 1) * RECORDS_PER_PAGE) + 1;
            s.EndRecordNumber = s.StartRecordNumber + s.PageSize - 1;

            IQueryable<Asset> assets = _domainContext.Assets;
            if (!string.IsNullOrEmpty(s.SearchText))
            {
                assets = assets.Where((i) => (i.Name.ToLower().Contains(s.SearchText.ToLower()) || i.Description.ToLower().Contains(s.SearchText.ToLower())));
            }
            if (s.SearchCategory.HasValue)
            {
                Category cat = _domainContext.GetById<Category>(s.SearchCategory.Value);
                assets = assets.Where((i) => i.Categories.Contains(cat));
            }
            assets = assets.Where((i) => i.DynamicDetails.State == s.SearchState);

            s.AssetCount = assets.Count();
            s.EndRecordNumber = Math.Min(s.AssetCount, s.EndRecordNumber);

            assets = assets.Skip(s.StartRecordNumber - 1).Take(RECORDS_PER_PAGE);
            s.Assets = assets.Select((i) => new BaseAssetModel(i)).ToList();
            return PartialView("_SearchResultsList", s);
        }
    }
}
