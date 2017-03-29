using Sentry.data.Core;
using System.Linq;
using System.Web.Mvc;
using System.Web.SessionState;
using System.Threading;
using Microsoft.AspNet.SignalR;

namespace Sentry.data.Web.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class HomeController : BaseController
    {
        //private IDataAssetContext _domainContext;
        //private IDataFeedContext _feedContext;

        //public HomeController(IDataAssetContext domainContext, IDataFeedContext feedContext)
        //{
        //    _domainContext = domainContext;
        //    _feedContext = feedContext;
        //}

        public HomeController()
        {
        }

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

        public ActionResult Index()
        {
            HomeModel model = new HomeModel();
            //###  BEGIN Sentry.Data  A### - Code below is Sentry.Data-specific
            //model.RootCategories = _domainContext.Categories.WhereIsRoot().Select((c) => new BaseCategoryModel(c)).ToList();
            //model.RootAssets = _domainContext.Assets.Select((c) => new BaseAssetModel(c)).ToList();
            //model.RootNewsFeedItems = _feedContext.NewsFeed.Select((c) => new DataFeedItemModel(c)).ToList();
            //model.RootHotTopicFeedItems = _feedContext.HotTopicsFeed.Select((c) => new DataFeedItemModel(c)).ToList();
            //###  END Sentry.Data  ### - Code above is Sentry.Data-specific
            return View(model);
        }

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

    }
}
