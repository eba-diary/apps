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
        static int r1, r2;
        
        private string[] colors = new string[] { "blue", "green", "gold", "plum", "orange", "blueGray" };

        public HomeController() { }

        public HomeController(IDataAssetProvider dap, IDataFeedContext feedContext)
        {
            Sentry.Common.Logging.Logger.Debug($"HomeController constructor called");
            _dataAssetProvider = dap;
            _feedContext = feedContext;
            das = new List<DataAsset>(_dataAssetProvider.GetDataAssets());
            cache = new CachingService();
        }

        public ActionResult Index()
        {
            do
            {
                r1 = new Random().Next(0, 6);
                r2 = new Random().Next(0, 6);
            }
            while (r1 == r2);

            ViewData["color"] = colors[r1];
            ViewData["color2"] = colors[r2];
            ViewData["fluid"] = true;

            return View(das);
        }

        public ActionResult GetFeed()
        {
            Sentry.Common.Logging.Logger.Debug($"Color before retrieval: {colors[r2]}");
            List<DataFeedItem> temp = cache.Get<List<DataFeedItem>>("feedAll");

            try { Sentry.Common.Logging.Logger.Debug($"Feed list count before cache: {temp.Count}"); }
            catch { Sentry.Common.Logging.Logger.Debug($"Feed list count before cache: null"); }

            //force to get feed items if failed to retrieve items previously due to storing a list of count 0 in failed try
            if (temp == null || temp.Count == 0)
            {
                Sentry.Common.Logging.Logger.Debug($"Feed list count was null or 0");
                dfisAll = _feedContext.GetAllFeedItems().ToList();
                cache.Add("feedAll", dfisAll);
            }
            else
            {
                dfisAll = cache.GetOrAdd("feedAll", () => _feedContext.GetAllFeedItems().ToList());
            }

            Sentry.Common.Logging.Logger.Debug($"Feed list count after cache: {dfisAll.Count}");
            Sentry.Common.Logging.Logger.Debug($"Color after retrieval: {colors[r2]}");

            ViewData["color2"] = colors[r2];
            
            return PartialView("_Feed", dfisAll.Take(10).ToList());
        }
        
        public ActionResult GetSentryFeed()
        {
            dfisSentry = cache.GetOrAdd("feedSentry", () => _feedContext.GetSentryFeedItems().ToList());
            ViewData["color2"] = colors[r2];

            return PartialView("_Feed", dfisSentry.Take(10).ToList());
        }

        public ActionResult GetMoreFeeds(int skip)
        {
            List<DataFeedItem> tempList = cache.GetOrAdd("feedAll", () => _feedContext.GetAllFeedItems().ToList()).Skip(skip).Take(5).ToList();
            ViewData["color2"] = colors[r2];
            return PartialView("_Feed", tempList);
        }

        public ActionResult GetMoreSentryFeeds(int skip)
        {
            List<DataFeedItem> tempList = cache.GetOrAdd("feedSentry", () => _feedContext.GetSentryFeedItems().ToList()).Skip(skip).Take(5).ToList();
            ViewData["color2"] = colors[r2];
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
