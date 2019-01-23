using Sentry.data.Core;
using System.Linq;
using System.Web.Mvc;
using System.Web.SessionState;
using System.Threading;
using Microsoft.AspNet.SignalR;
using System.Collections.Generic;
using System;
using System.Xml;
using System.ServiceModel.Syndication;
using LazyCache;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web.Script.Serialization;
using System.Text;
using System.Net.Http.Headers;
using System.Net;
using Newtonsoft.Json.Linq;
using System.IO;
using Sentry.Configuration;
using Sentry.data.Common;

namespace Sentry.data.Web.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class HomeController : BaseController
    {
        private readonly IDataFeedContext _feedContext;
        private readonly IDatasetContext _dsContext;
        private readonly IAppCache cache;
        private List<DataFeedItem> dfisAll;

        public HomeController(IDataFeedContext feedContext, IDatasetContext datasetContext, IDataAssetContext dataAssetContext)
        {
            _feedContext = feedContext;
            _dsContext = datasetContext;
            cache = new CachingService();
        }

        public ActionResult Index()
        {
            ViewData["fluid"] = true;

            HomeModel hm = new HomeModel();

            List<Dataset> dsList = _dsContext.Datasets.ToList();

            hm.DatasetCount = dsList.Count(w => w.DatasetType == GlobalConstants.DataEntityTypes.DATASET);
            hm.Categories = _dsContext.Categories.Where(w => w.ObjectType == GlobalConstants.DataEntityTypes.DATASET).ToList();
            hm.CanEditDataset = SharedContext.CurrentUser.CanEditDataset;
            hm.CanUpload = SharedContext.CurrentUser.CanUpload;

            Event e = new Event();
            e.EventType = _dsContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault();
            e.Status = _dsContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
            e.TimeCreated = DateTime.Now;
            e.TimeNotified = DateTime.Now;
            e.IsProcessed = false;
            e.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;
            e.Reason = "Viewed Home Page";
            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

            return View(hm);
        }

        //TODO Refactor into table (https://jira.sentry.com/browse/DSC-476)
        private readonly int[] sprintsIds = new int[2] { 2230, 2346 };

        public async Task<ActionResult> ReleaseNotes()
        {
            return View();
        }

        public class SprintModel
        {
            public List<Sprint> sprints { get; set; }
        }

        //TODO Refactor Jira calls into Infrastructure Layer (https://jira.sentry.com/browse/DSC-475)
        public JsonResult GetSprint()
        {


            string mergedCredentials = string.Format("{0}:{1}", Config.GetHostSetting("JiraApiUser"), Config.GetHostSetting("JiraApiPass"));
            byte[] byteCredentials = UTF8Encoding.UTF8.GetBytes(mergedCredentials);
            var base64Credentials = Convert.ToBase64String(byteCredentials);

            List<Sprint> sprints = new List<Sprint>();

            foreach (int sprintId in sprintsIds)
            {
                WebRequest wr = WebRequest.Create(String.Format("{0}rest/agile/1.0/sprint/{1}", Config.GetHostSetting("JiraApiUrl"), sprintId));

                wr.ContentType = "application/json;charset=UTF-8";
                wr.Method = "GET";
                wr.Headers.Add("Authorization", "Basic " + base64Credentials);
                wr.PreAuthenticate = true;

                WebResponse response = wr.GetResponse();

                StreamReader sr = new StreamReader(response.GetResponseStream());

                JObject joResponse = JObject.Parse(sr.ReadToEnd());


                Sprint sprint = joResponse.ToObject<Sprint>();

                sprint.issues = GetIssues(sprintId);

                sprints.Add(sprint);
            }

            SprintModel sm = new SprintModel { sprints = sprints };


            return Json(sm, JsonRequestBehavior.AllowGet);
        }

        //TODO Refactor Jira calls into Infrastructure Layer (https://jira.sentry.com/browse/DSC-475)
        public List<Issue> GetIssues(int sprintId)
        {
            string mergedCredentials = string.Format("{0}:{1}", Config.GetHostSetting("JiraApiUser"), Config.GetHostSetting("JiraApiPass"));
            byte[] byteCredentials = UTF8Encoding.UTF8.GetBytes(mergedCredentials);
            var base64Credentials = Convert.ToBase64String(byteCredentials);

            var wr = WebRequest.Create(String.Format("{0}rest/agile/1.0/sprint/{1}/issue ", Config.GetHostSetting("JiraApiUrl"), sprintId));

            wr.ContentType = "application/json;charset=UTF-8";
            wr.Method = "GET";
            wr.Headers.Add("Authorization", "Basic " + base64Credentials);
            wr.PreAuthenticate = true;

            var response = wr.GetResponse();

            var sr = new StreamReader(response.GetResponseStream());

            var joResponse = JObject.Parse(sr.ReadToEnd());         

            List<Issue> issues = joResponse["issues"].ToObject<List<Issue>>();

            return issues;


        }

        public async Task<ActionResult> GetFeed()
        {
            dfisAll = await Task.Factory.StartNew(() => cache.GetOrAdd("feedAll", () => _feedContext.GetAllFeedItems().ToList(), TimeSpan.FromHours(1)));
            return PartialView("_Feed", dfisAll.Take(10).ToList());
        }

        public ActionResult GetMoreFeeds(int skip)
        {
            List<DataFeedItem> tempList = cache.GetOrAdd("feedAll", () => _feedContext.GetAllFeedItems().ToList()).Skip(skip).Take(5).ToList();
            return PartialView("_Feed", tempList);
        }

        public ActionResult GetMoreSentryFeeds(int skip)
        {
            List<DataFeedItem> tempList = cache.GetOrAdd("feedSentry", () => _feedContext.GetSentryFeedItems().ToList()).Skip(skip).Take(5).ToList();
            return PartialView("_Feed", tempList);
        }


        public ActionResult GetFavorites()
        {
            List<DataFeedItem> favList = _feedContext.GetAllFavorites(SharedContext.CurrentUser.AssociateId).OrderBy(o => o.Title).ToList();           
            ViewBag.CanEditDataset = SharedContext.CurrentUser.CanEditDataset;
            return PartialView("_Favorites", favList);
        }
    }
}
