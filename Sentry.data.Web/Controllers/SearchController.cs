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
using Sentry.data.Core.Entities.Metadata;
using static Sentry.data.Core.RetrieverJobOptions;

namespace Sentry.data.Web.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class SearchController : BaseController
    {
        public IAssociateInfoProvider _associateInfoProvider;
        public IDatasetContext _datasetContext;
        private UserService _userService;
        private S3ServiceProvider _s3Service;
        private ISASService _sasService;
        private IAppCache _cache;
        private IRequestContext _requestContext;

        public SearchController(IDatasetContext dsCtxt, S3ServiceProvider dsSvc, UserService userService, ISASService sasService, IAssociateInfoProvider associateInfoService, IRequestContext requestContext)
        {
            _cache = new CachingService();
            _datasetContext = dsCtxt;
            _s3Service = dsSvc;
            _userService = userService;
            _sasService = sasService;
            _associateInfoProvider = associateInfoService;
            _requestContext = requestContext;
        }

        // GET: Search
        public ActionResult Index()
        {
            return View();
        }

        // GET: Search/Datasets/searchParms
        [Route("Search/Datasets/Index")]
        [Route("Search/Datasets/")]
        [AuthorizeByPermission(PermissionNames.DatasetView)]
        public ActionResult Index(string category, string searchPhrase, string ids)
        {
            return View();
        }

        public class SearchTerms
        {
            public List<string> Category_Filters { get; set; }
            public List<string> Sentry_Owners { get; set; }
            public List<string> Extensions { get; set; }
            public String Search_Term { get; set; }
            public int Results_Returned { get; set; }
        }

        [HttpPost]
        [Route("Search/SearchEvent")]
        public JsonResult SearchEvent(string categoryFilters, string sentryOwners, string extensions, String searchTerm, int resultsReturned)
        {
            Event e = new Event();
            e.EventType = _datasetContext.EventTypes.Where(w => w.Description == "Search").FirstOrDefault();
            e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
            e.TimeCreated = DateTime.Now;
            e.TimeNotified = DateTime.Now;
            e.IsProcessed = false;
            e.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;

            e.Search = JsonConvert.SerializeObject(new SearchTerms()
            {
                Category_Filters = categoryFilters.Split(',').Where(x => !String.IsNullOrWhiteSpace(x)).ToList(),
                Sentry_Owners = sentryOwners.Split('|').Where(x => !String.IsNullOrWhiteSpace(x)).ToList(),
                Extensions = extensions.Split(',').Where(x => !String.IsNullOrWhiteSpace(x)).ToList(),
                Search_Term = searchTerm,
                Results_Returned = resultsReturned
            });

            e.Reason = "Searched Datasets";
            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);


            return Json(true, JsonRequestBehavior.AllowGet);
        }


        [Route("Search/DatasetList")]
        public JsonResult DatasetList()
        {
            List<SearchModel> models = new List<SearchModel>();

            foreach (Dataset ds in _datasetContext.Datasets.ToList())
            {
                SearchModel sm = new SearchModel(ds, _associateInfoProvider);
                models.Add(sm);
            }

            return Json(models, JsonRequestBehavior.AllowGet);
        }
    }
}