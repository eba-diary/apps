using Newtonsoft.Json.Linq;
using Sentry.Configuration;
using Sentry.data.Core;
using Sentry.data.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.SessionState;

namespace Sentry.data.Web.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class HomeController : BaseController
    {
        private readonly IDataFeedContext _feedContext;
        private readonly IDatasetContext _dsContext;
        private readonly IDataFeatures _featureFlags;
        private readonly IUserFavoriteService _userFavoriteService;

        public HomeController(IDataFeedContext feedContext, IDatasetContext datasetContext, IDataFeatures featureFlags, IUserFavoriteService userFavoriteService)
        {
            _feedContext = feedContext;
            _dsContext = datasetContext;
            _featureFlags = featureFlags;
            _userFavoriteService = userFavoriteService;
        }

        public ActionResult Index()
        {
            ViewData["fluid"] = true;

            /*List<Dataset> dsList = _dsContext.Datasets.Where(x=> x.CanDisplay).ToList();*/

            HomeModel hm = new HomeModel()
            {
                /*DatasetCount = dsList.Count(w => w.DatasetType == GlobalConstants.DataEntityCodes.DATASET),*/
                Categories = _dsContext.Categories.Where(w => w.ObjectType == GlobalConstants.DataEntityCodes.DATASET).ToList(),
                CanEditDataset = SharedContext.CurrentUser.CanModifyDataset,
                DisplayDataflowMetadata = _featureFlags.Expose_Dataflow_Metadata_CLA_2146.GetValue(),
                CLA2838_DSC_ANOUNCEMENTS = _featureFlags.CLA2838_DSC_ANOUNCEMENTS.GetValue(),
                DirectToSearchPages = _featureFlags.CLA3756_UpdateSearchPages.GetValue()
            };

            Event e = new Event()
            {
                EventType = _dsContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault(),
                Status = _dsContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault(),
                UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId,
                Reason = "Viewed Home Page"
            };

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
            List<DataFeedItem> allDatafeedItems = _feedContext.GetAllFeedItems().ToList();
            if(allDatafeedItems.Count > 0)
            {
                return PartialView("_Feed", allDatafeedItems.Take(10).ToList());
            }
            else
            {
                return PartialView("_FeedEmpty");
            }
        }

        public ActionResult GetMoreFeed(int skip)
        {
            List<DataFeedItem> tempList = _feedContext.GetAllFeedItems().Skip(skip).Take(5).ToList();
            return PartialView("_Feed", tempList);
        }

        public ActionResult GetFavorites()
        {
            IList<FavoriteItem> favList = _userFavoriteService.GetUserFavoriteItems(SharedContext.CurrentUser.AssociateId);
            List<FavoriteItemModel> favItems = favList.Select(x => x.ToModel()).OrderBy(x => x.Sequence).ThenBy(y => y.Title).ToList();
            ViewBag.CanEditDataset = SharedContext.CurrentUser.CanModifyDataset;
            return PartialView("_Favorites", favItems);
        }
    }
}