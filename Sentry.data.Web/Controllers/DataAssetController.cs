using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sentry.data.Core;
using System.Text;
using Sentry.DataTables.Mvc;
using Sentry.DataTables.Shared;
using Sentry.DataTables.QueryableAdapter;
using Sentry.data.Infrastructure;
using System.Threading.Tasks;
using Sentry.data.Common;

namespace Sentry.data.Web.Controllers
{
    [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATA_ASSET_VIEW)]
    public class DataAssetController : BaseController
    {
        public readonly MetadataRepositoryService _metadataRepositoryService;

        public readonly IDataAssetContext _dataAssetContext;
        public readonly IDatasetContext _dataSetContext;

        public readonly IAssociateInfoProvider _associateInfoService;
        public readonly UserService _userService;

        public DataAssetController(MetadataRepositoryService metadataRepositoryService, IDataAssetContext dataAssetContext, IDatasetContext datasetContext,  IAssociateInfoProvider associateInfoService, UserService userService)
        {
            _metadataRepositoryService = metadataRepositoryService;
            _dataAssetContext = dataAssetContext;
            _dataSetContext = datasetContext;
            _associateInfoService = associateInfoService;
            _userService = userService;
        }

        public ActionResult Index(int id)
        {
            var das = new List<DataAsset>(_dataAssetContext.GetDataAssets());
            ViewBag.DataAssets = das.Select(x => new Models.AssetUIModel(x)).ToList();
            ViewBag.CanUserSwitch = SharedContext.CurrentUser.CanUserSwitch;

            id = (id == 0) ? das.FirstOrDefault(x => x.DisplayName.ToLower().Contains("sera pl")).Id : id;

            DataAsset da = _dataAssetContext.GetDataAsset(id);

            if (da != null)
            {
                da.AssetNotifications = _dataAssetContext.GetAssetNotificationsByDataAssetId(da.Id).Where(w => w.StartTime < DateTime.Now && w.ExpirationTime > DateTime.Now).ToList();
                da.LastUpdated = DateTime.Now;
                da.Status = 1;

                Event e = new Event();
                e.EventType = _dataSetContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault();
                e.Status = _dataSetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
                e.TimeCreated = DateTime.Now;
                e.TimeNotified = DateTime.Now;
                e.IsProcessed = false;
                e.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;
                e.DataAsset = da.Id;
                e.Reason = "Viewed Data Asset";
                Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);
            }

            if (da != null) { return View(da); }
            else { return RedirectToAction("NotFound", "Error"); }
        }

        [Route("Lineage/{line}")]
        public ActionResult Lineage(string line)
        {
            ViewBag.IsLine = true;
            ViewBag.LineName = line;

            Event e = new Event();
            e.EventType = _dataSetContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault();
            e.Status = _dataSetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
            e.TimeCreated = DateTime.Now;
            e.TimeNotified = DateTime.Now;
            e.IsProcessed = false;
            e.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;
            if (line == "Personal Lines" || line.ToUpper() == "PL")
            {
                e.Line_CDE = "PL";
            }
            else if (line == "Commercial Lines" || line.ToUpper() == "CL")
            {
                e.Line_CDE = "CL";
            }

            e.Reason = "Viewed Lineage for " + line;
            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

            return View();
        }


        [Route("Lineage/{line}/{assetName}")]
        [Route("Lineage/{line}/{assetName}/{businessObject}/{sourceElement}")]
        public ActionResult Lineage(string line, string assetName, string businessObject, string sourceElement)
        {
            var das = new List<DataAsset>(_dataAssetContext.GetDataAssets());

            assetName = (assetName == null) ? das[0].Name : assetName;

            DataAsset da = _dataAssetContext.GetDataAsset(assetName);
            if (da != null)
            {
                da.AssetNotifications = _dataAssetContext.GetAssetNotificationsByDataAssetId(da.Id).Where(w => w.StartTime < DateTime.Now && w.ExpirationTime > DateTime.Now).ToList();
                da.LastUpdated = DateTime.Now;
                da.Status = 1;

                Event e = new Event();
                e.EventType = _dataSetContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault();
                e.Status = _dataSetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
                e.TimeCreated = DateTime.Now;
                e.TimeNotified = DateTime.Now;
                e.IsProcessed = false;
                e.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;
                e.DataAsset = da.Id;

                if(line == "Personal Lines" || line.ToUpper() == "PL")
                {
                    e.Line_CDE = "PL";
                }
                else if (line == "Commercial Lines" || line.ToUpper() == "CL")
                {
                    e.Line_CDE = "CL";
                }

                e.Reason = "Viewed Lineage for " + da.DisplayName;
                Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);
            }

            ViewBag.IsLine = false;
            ViewBag.DataAsset = da;

            if (da != null) { return View(da); }
            else { return RedirectToAction("NotFound", "Error"); }
        }

        public ActionResult DataAsset(string assetName)
        {
            var das = new List<DataAsset>(_dataAssetContext.GetDataAssets());
            ViewBag.DataAssets = das.Select(x => new Models.AssetUIModel(x)).ToList();
            ViewBag.CanUserSwitch = SharedContext.CurrentUser.CanUserSwitch;

            assetName = (assetName == null) ? das[0].Name : assetName;

            DataAsset da = _dataAssetContext.GetDataAsset(assetName);
            if (da != null)
            {
                da.AssetNotifications = _dataAssetContext.GetAssetNotificationsByDataAssetId(da.Id).Where(w => w.StartTime < DateTime.Now && w.ExpirationTime > DateTime.Now).ToList();
                da.LastUpdated = DateTime.Now;
                da.Status = 1;

                Event e = new Event();
                e.EventType = _dataSetContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault();
                e.Status = _dataSetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
                e.TimeCreated = DateTime.Now;
                e.TimeNotified = DateTime.Now;
                e.IsProcessed = false;
                e.UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId;
                e.DataAsset = da.Id;
                e.Reason = "Viewed Data Asset";
                Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);
            }

            if (da != null) { return View("Index", da); }
            else { return RedirectToAction("NotFound", "Error"); }
        }


    }
}