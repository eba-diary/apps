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

namespace Sentry.data.Web.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class HomeController : BaseController
    {
        private IDataFeedContext _feedContext;
        private IDataAssetProvider _dataAssetProvider;
        private IAppCache cache;
        private List<DataAsset> das;
        private List<DataFeedItem> dfisAll;
        private List<DataFeedItem> dfisSentry;

        public HomeController() { }

        public HomeController(IDataAssetProvider dap, IDataFeedContext feedContext)
        {
            _dataAssetProvider = dap;
            _feedContext = feedContext;
            das = new List<DataAsset>(_dataAssetProvider.GetDataAssets());
            cache = new CachingService();
        }

        public ActionResult Index()
        {
            ViewData["fluid"] = true;
            return View(das);
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
            dfisAll = await Task.Factory.StartNew(() => cache.GetOrAdd("feedAll", () => _feedContext.GetAllFeedItems().ToList()));
            return PartialView("_Feed", dfisAll.Take(10).ToList());
        }
        
        public ActionResult GetSentryFeed()
        {
            dfisSentry = cache.GetOrAdd("feedSentry", () => _feedContext.GetSentryFeedItems().ToList());
            return PartialView("_Feed", dfisSentry.Take(10).ToList());
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
    }

    //private IDataAssetContext _domainContext;

    //public HomeController(IDataAssetContext domainContext, IDataFeedContext feedContext)
    //{
    //    _domainContext = domainContext;
    //    _feedContext = feedContext;
    //}

    //public JsonResult LongRunningProcess()
    //{
    //    //THIS COULD BE SOME LIST OF DATA
    //    int itemsCount = 100;

    //    for (int i = 0; i <= itemsCount; i++)
    //    {
    //        //SIMULATING SOME TASK
    //        Thread.Sleep(50);

    //        //CALLING A FUNCTION THAT CALCULATES PERCENTAGE AND SENDS THE DATA TO THE CLIENT
    //        Functions.SendProgress("Process in progress...", i, itemsCount);
    //    }

    //    return Json("", JsonRequestBehavior.AllowGet);
    //}

    //###  BEGIN Sentry.Data  A### - Code below is Sentry.Data-specific
    //public PartialViewResult AssetOverview()
    //{
    //    return PartialView("_AssetOverview", _domainContext.Assets.OrderBy((i) => i.Name).Select((i) => new BaseAssetModel(i)).ToList());
    //}

    //public PartialViewResult HotTopicsFeed()
    //{
    //    return PartialView("_HotTopicsFeed", _feedContext.HotTopicsFeed.OrderBy((i) => i.PublishDate).Take(3).Select((i) => new DataFeedItemModel(i)).ToList());
    //}

    //public PartialViewResult NewsFeed()
    //{
    //    return PartialView("_NewsFeed", _feedContext.NewsFeed.OrderBy((i) => i.PublishDate).Take(3).Select((i) => new DataFeedItemModel(i)).ToList());
    //}
    //###  END Sentry.Data  ### - Code above is Sentry.Data-specific

    //FROM INDEX ACTION METHOD
    //HomeModel model = new HomeModel();
    //###  BEGIN Sentry.Data  A### - Code below is Sentry.Data-specific
    //model.RootCategories = _domainContext.Categories.WhereIsRoot().Select((c) => new BaseCategoryModel(c)).ToList();
    //model.RootAssets = _domainContext.Assets.Select((c) => new BaseAssetModel(c)).ToList();
    //model.RootNewsFeedItems = _feedContext.NewsFeed.Select((c) => new DataFeedItemModel(c)).ToList();
    //model.RootHotTopicFeedItems = _feedContext.HotTopicsFeed.Select((c) => new DataFeedItemModel(c)).ToList();
    //###  END Sentry.Data  ### - Code above is Sentry.Data-specific
    //return View(model);
}
