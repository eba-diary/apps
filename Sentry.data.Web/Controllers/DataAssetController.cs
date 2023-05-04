using Sentry.data.Core;
using Sentry.data.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Sentry.data.Web.Controllers
{
    [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATA_ASSET_VIEW)]
    public class DataAssetController : BaseController
    {
        #region Fields
        public readonly MetadataRepositoryService _metadataRepositoryService;
        public readonly IDataAssetContext _dataAssetContext;
        public readonly IDatasetContext _dataSetContext;
        public readonly IAssociateInfoProvider _associateInfoService;
        public readonly IUserService _userService;
        #endregion

        #region Constructor
        public DataAssetController(MetadataRepositoryService metadataRepositoryService, IDataAssetContext dataAssetContext, IDatasetContext datasetContext, IAssociateInfoProvider associateInfoService, IUserService userService)
        {
            _metadataRepositoryService = metadataRepositoryService;
            _dataAssetContext = dataAssetContext;
            _dataSetContext = datasetContext;
            _associateInfoService = associateInfoService;
            _userService = userService;
        }
        #endregion

        #region Controller Methods
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

                CreateEvent("Views Data Asset", da.Id);
            }

            return da != null ? View(da) : (ActionResult)RedirectToAction("NotFound", "Error");
        }

        [Route("Lineage/{line}")]
        public ActionResult Lineage(string line)
        {
            ViewBag.IsLine = true;
            ViewBag.LineName = line;

            string lineCde = null;
            if (line == "Personal Lines" || line.ToUpper() == "PL")
            {
                lineCde = "PL";
            }
            else if (line == "Commercial Lines" || line.ToUpper() == "CL")
            {
                lineCde = "CL";
            }

            CreateEvent($"Viewed Lineage for {line}", lineCode: lineCde);

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

                string lineCode = null;
                if (line == "Personal Lines" || line.ToUpper() == "PL")
                {
                    lineCode = "PL";
                }
                else if (line == "Commercial Lines" || line.ToUpper() == "CL")
                {
                    lineCode = "CL";
                }

                CreateEvent($"Viewed Lineage for {da.DisplayName}", da.Id, lineCode);
            }

            ViewBag.IsLine = false;
            ViewBag.DataAsset = da;

            return da != null ? View(da) : (ActionResult)RedirectToAction("NotFound", "Error");
        }

        public ActionResult DataAsset(string assetName)
        {
            var das = new List<DataAsset>(_dataAssetContext.GetDataAssets());
            ViewBag.DataAssets = das.Select(x => new Models.AssetUIModel(x)).ToList();
            ViewBag.CanUserSwitch = SharedContext.CurrentUser.CanUserSwitch;

            assetName = assetName ?? das[0].Name;

            DataAsset da = _dataAssetContext.GetDataAsset(assetName);
            if (da != null)
            {
                da.AssetNotifications = _dataAssetContext.GetAssetNotificationsByDataAssetId(da.Id).Where(w => w.StartTime < DateTime.Now && w.ExpirationTime > DateTime.Now).ToList();
                da.LastUpdated = DateTime.Now;
                da.Status = 1;

                CreateEvent("Viewed Data Asset", da.Id);
            }

            return da != null ? View("Index", da) : (ActionResult)RedirectToAction("NotFound", "Error");
        }
        #endregion

        #region Methods
        private void CreateEvent(string reason, int? dataAssetId = null, string lineCode = null)
        {
            Event e = new Event()
            {
                EventType = _dataSetContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault(),
                Status = _dataSetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault(),
                UserWhoStartedEvent = SharedContext.CurrentUser.AssociateId,
                Reason = reason,
                DataAsset = dataAssetId,
                Line_CDE = lineCode
            };

            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);
        }
        #endregion
    }
}