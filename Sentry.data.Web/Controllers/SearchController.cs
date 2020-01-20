using Sentry.Core;
using Sentry.data.Core;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.SessionState;
using System.Linq.Dynamic;
using System.Web;
using Sentry.data.Web.Helpers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using Sentry.data.Infrastructure;
using Sentry.DataTables.Shared;
using Sentry.DataTables.Mvc;
using Sentry.DataTables.QueryableAdapter;
using Sentry.data.Common;
using System.Diagnostics;
using LazyCache;
using StackExchange.Profiling;
using Sentry.Common.Logging;
using static Sentry.data.Core.RetrieverJobOptions;

namespace Sentry.data.Web.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class SearchController : BaseController
    {
        public IAssociateInfoProvider _associateInfoProvider;
        public IDatasetContext _datasetContext;
        private UserService _userService;
        private readonly IEventService _eventService;

        private string Title { get; set; }

        public SearchController(IDatasetContext dsCtxt, UserService userService, IAssociateInfoProvider associateInfoService, IEventService eventService)
        {
            _datasetContext = dsCtxt;
            _userService = userService;
            _associateInfoProvider = associateInfoService;
            _eventService = eventService;
        }

        // GET: Search
        public ActionResult Index()
        {
            return View();
        }

        // GET: Search/Datasets/searchParms
        [Route("Search/{searchType?}/Index")]
        [Route("Search/{searchType?}/")]
        public ActionResult Index(string searchType, string category, string searchPhrase, string ids)
        {
            SearchIndexModel model = new SearchIndexModel();
            IApplicationUser user = _userService.GetCurrentUser();

            //validate user has permissions
            if ((searchType == GlobalConstants.SearchType.BUSINESS_INTELLIGENCE_SEARCH && !user.CanViewReports) ||
                (searchType == GlobalConstants.SearchType.DATASET_SEARCH && !user.CanViewDataset))
            {
                return View("Forbidden");
            }

            switch (searchType)
            {
                case GlobalConstants.SearchType.BUSINESS_INTELLIGENCE_SEARCH:
                    ViewBag.Title = "Business Intelligence";
                    model.SearchType = searchType;
                    break;
                case GlobalConstants.SearchType.DATASET_SEARCH:
                    ViewBag.Title = "Dataset";
                    model.SearchType = searchType;
                    break;
                default:
                    if (user.IsAdmin)
                    {
                        ViewBag.Title = "Search";
                        break;
                    }
                    else { return View("Forbidden"); }
            }            

            return View(model);
        }

        public class SearchTerms
        {
            public List<string> Category_Filters { get; set; }
            public List<string> Sentry_Owners { get; set; }
            public List<string> Extensions { get; set; }
            public List<string> BusinessUnits { get; set; }
            public List<string> DatasetFunctions { get; set; }
            public List<string> Tags { get; set; }
            public String Search_Term { get; set; }
            public int Results_Returned { get; set; }
        }

        [HttpPost]
        [Route("Search/SearchEvent/{searchType}")]
        public JsonResult SearchEvent(string searchType, string categoryFilters, string sentryOwners, string extensions, string businessUnits, string datasetFunctions, string tags, String searchTerm, int resultsReturned)
        {
            string reason = null;
            switch (searchType)
            {
                case GlobalConstants.SearchType.BUSINESS_INTELLIGENCE_SEARCH:
                    reason = "Searched Exhibits";
                    break;
                case GlobalConstants.SearchType.DATASET_SEARCH:
                    reason = "Searched Datasets";
                    break;
                default:
                    break;
            }

            string searchValue = JsonConvert.SerializeObject(new SearchTerms()
            {
                Category_Filters = categoryFilters.Split(',').Where(x => !String.IsNullOrWhiteSpace(x)).ToList(),
                Sentry_Owners = (!String.IsNullOrWhiteSpace(sentryOwners)) ? sentryOwners.Split('|').Where(x => !String.IsNullOrWhiteSpace(x)).ToList() : null,
                Extensions = extensions.Split(',').Where(x => !String.IsNullOrWhiteSpace(x)).ToList(),
                BusinessUnits = businessUnits.Split(',').Where(x => !String.IsNullOrWhiteSpace(x)).ToList(),
                DatasetFunctions = datasetFunctions.Split(',').Where(x => !String.IsNullOrWhiteSpace(x)).ToList(),
                Tags = tags.Split(',').Where(x => !String.IsNullOrWhiteSpace(x)).ToList(),
                Search_Term = searchTerm,
                Results_Returned = resultsReturned
            });

            _eventService.PublishSuccessEvent(_datasetContext.EventTypes.Where(w => w.Description == "Search").FirstOrDefault().Description, SharedContext.CurrentUser.AssociateId,
                                            reason, null, searchValue);

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        //[Route("Search/List")]
        [Route("Search/{searchType?}/List")]
        public JsonResult List(string searchType)
        {

            List<SearchModel> models = new List<SearchModel>();

            //validate user has permissions
            // return empty list if user does not have permissions
            IApplicationUser user = _userService.GetCurrentUser();
            if ((searchType == GlobalConstants.SearchType.BUSINESS_INTELLIGENCE_SEARCH && !user.CanViewReports) ||
                (searchType == GlobalConstants.SearchType.DATASET_SEARCH && !user.CanViewDataset))
            {
                return Json(models, JsonRequestBehavior.AllowGet);
            }

            IQueryable<Dataset> dsQuery;

            switch (searchType)
            {
                case "BusinessIntelligence":
                    dsQuery = _datasetContext.Datasets.Where(x => x.DatasetType == GlobalConstants.DataEntityCodes.REPORT && x.CanDisplay);
                    break;
                case "Datasets":
                    dsQuery = _datasetContext.Datasets.Where(w => w.DatasetType == GlobalConstants.DataEntityCodes.DATASET && w.CanDisplay);
                    break;
                default:
                    if (user.IsAdmin)
                    {
                        dsQuery = _datasetContext.Datasets.Where(x => x.CanDisplay);
                        break;
                    }
                    else { return Json(models, JsonRequestBehavior.AllowGet); }
            }

            var dsList = dsQuery.FetchAllChildren(_datasetContext);
            var dsIds = dsList.Select(x => x.DatasetId.ToString()).ToList();

            var events = new List<Event>();
            foreach (var group in dsIds.Split(1000))
            {
                events.AddRange(_datasetContext.Events.Where(x => x.EventType.Description == GlobalConstants.EventType.VIEWED && x.Dataset.HasValue && dsIds.Contains(x.Dataset.Value.ToString())).ToList());
            }

            foreach (Dataset ds in dsList.OrderBy(x => x.DatasetName).ToList())
            {
                SearchModel sm = new SearchModel(ds, _associateInfoProvider)
                {
                    IsFavorite = ds.Favorities.Any(w => w.UserId == SharedContext.CurrentUser.AssociateId),
                    PageViews = events.Count(x => x.Dataset == ds.DatasetId),
                    CanEditDataset = (searchType == GlobalConstants.SearchType.BUSINESS_INTELLIGENCE_SEARCH) ? SharedContext.CurrentUser.CanManageReports : false
                };
                models.Add(sm);
            }

            return Json(models, JsonRequestBehavior.AllowGet);
        }
    }
}