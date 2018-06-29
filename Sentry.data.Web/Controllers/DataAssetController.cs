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

        [AuthorizeByPermission(PermissionNames.DataAssetView)]
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
        [AuthorizeByPermission(PermissionNames.DataAssetView)]
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
        [AuthorizeByPermission(PermissionNames.DataAssetView)]
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

        [AuthorizeByPermission(PermissionNames.DataAssetView)]
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

        [AuthorizeByPermission(PermissionNames.ManageAssetNotifications)]
        public ActionResult ManageAssetNotification()
        {
            BaseAssetNotificationModel banm = new BaseAssetNotificationModel();
            return View(banm);
        }

        [HttpGet]
        [AuthorizeByPermission(PermissionNames.ManageAssetNotifications)]
        public ActionResult CreateAssetNotification()
        {
            IApplicationUser user = _userService.GetCurrentUser();

            if (!user.CanManageAssetAlerts)
            {
                throw new UnauthorizedAccessException();
            }

            CreateAssetNotificationModel canm = new CreateAssetNotificationModel();
            canm.AllDataAssets = GetDataAssetsList();
            canm.AllSeverities = GetNotificationSeverities();
            canm.CreateUser = user.AssociateId;
            canm.DisplayCreateUser = _associateInfoService.GetAssociateInfo(user.AssociateId);
            canm.StartTime = DateTime.Now; //preset Start Time
            canm.ExpirationTime = DateTime.Now.AddHours(1);

            return View("CreateAssetNotification",canm);
        }

        [HttpPost]
        [AuthorizeByPermission(PermissionNames.ManageAssetNotifications)]
        public ActionResult CreateAssetNotification(CreateAssetNotificationModel canm)
        {
            IApplicationUser user = _userService.GetCurrentUser();

            if(!user.CanManageAssetAlerts)
            {
                throw new UnauthorizedAccessException();
            }

            AssetNotifications an = new AssetNotifications();

            if (canm.ExpirationTime <= canm.StartTime)
            {
                ModelState.AddModelError(string.Empty, "Expiration Time cannot be before Start Time");
            }

            if (canm.StartTime >= canm.ExpirationTime)
            {
                ModelState.AddModelError(string.Empty, "Start Time cannot be after Expiration Time");
            }

            if (canm.StartTime <= DateTime.Now.AddHours(-1))
            {
                ModelState.AddModelError(string.Empty, "Start Time cannot be greater than an hour in the past");
            }

            if (canm.ExpirationTime <= DateTime.Now)
            {
                ModelState.AddModelError(string.Empty, "Expiration Time cannot be in the past");
            }

            try
            {
                if (ModelState.IsValid)
                {
                    an = CreateAssetNotificationFromModel(an, canm);
                    _dataAssetContext.Merge<AssetNotifications>(an);
                    _dataAssetContext.SaveChanges();
                    return View("ManageAssetNotification");
                }
            }
            catch (Sentry.Core.ValidationException ex)
            {
                AddCoreValidationExceptionsToModel(ex);
                _dataSetContext.Clear();
            }
          
            canm.CreateUser = user.AssociateId;
            canm.DisplayCreateUser = _associateInfoService.GetAssociateInfo(user.AssociateId);
            canm.AllDataAssets = GetDataAssetsList();
            canm.AllSeverities = GetNotificationSeverities();

            return View("CreateAssetNotification", canm);
        }

        [AuthorizeByPermission(PermissionNames.ManageAssetNotifications)]
        public JsonResult GetAssetNotificationInfoForGrid(int Id, [ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest dtRequest)
        {
            IEnumerable<BaseAssetNotificationModel> files = null;
            if (Id > 0)
            {
                files = _dataAssetContext.GetAssetNotificationsByDataAssetId(Id).Select((an) => new BaseAssetNotificationModel(an, _associateInfoService));
            }
            else
            {
                files = _dataAssetContext.GetAllAssetNotifications().Select((an) => new BaseAssetNotificationModel(an, _associateInfoService));
            }

            DataTablesQueryableAdapter<BaseAssetNotificationModel> dtqa = new DataTablesQueryableAdapter<BaseAssetNotificationModel>(files.AsQueryable(), dtRequest);
            return Json(dtqa.GetDataTablesResponse(), JsonRequestBehavior.AllowGet);
        }


        [HttpGet()]
        [AuthorizeByPermission(PermissionNames.ManageAssetNotifications)]
        public ActionResult EditAssetNotification(int notificationId)
        {
            IApplicationUser user = _userService.GetCurrentUser();

            if (!user.CanManageAssetAlerts)
            {
                throw new UnauthorizedAccessException();
            }

            AssetNotifications an = _dataAssetContext.GetAssetNotificationByID(notificationId);
            EditAssetNotificationModel ean = new EditAssetNotificationModel(an, _associateInfoService);
            ean.AllSeverities = GetNotificationSeverities();
            ean.SeverityID = an.MessageSeverity;  //Preselect current value

            return View("EditAssetNotification", ean);
        }

        [HttpPost()]
        [AuthorizeByPermission(PermissionNames.ManageAssetNotifications)]
        public ActionResult EditAssetNotification(EditAssetNotificationModel ean)
        {
            IApplicationUser user = _userService.GetCurrentUser();

            if (!user.CanManageAssetAlerts)
            {
                throw new UnauthorizedAccessException();
            }

            AssetNotifications an = _dataAssetContext.GetAssetNotificationByID(ean.NotificationId);

            if(ean.ExpirationTime <= ean.StartTime)
            {
                ModelState.AddModelError(string.Empty, "Expiration Time cannot be before Start Time");
            }

            if (ean.StartTime >= ean.ExpirationTime)
            {
                ModelState.AddModelError(string.Empty, "Start Time cannot be after Expiration Time");
            }

            if (ean.StartTime <= DateTime.Now.AddHours(-1))
            {
                ModelState.AddModelError(string.Empty, "Start Time cannot be greater than an hour in the past");
            }

            if (ean.ExpirationTime <= DateTime.Now)
            {
                ModelState.AddModelError(string.Empty, "Expiration Time cannot be in the past");
            }

            try
            {
                if (ModelState.IsValid)
                {
                    an = UpdateAssetNotificationFromModel(an, ean);
                    _dataSetContext.SaveChanges();
                    return View("ManageAssetNotification");
                }
            }
            catch (Sentry.Core.ValidationException ex)
            {
                AddCoreValidationExceptionsToModel(ex);
                _dataSetContext.Clear();
            }

            //Return partial view when there are errors
            ean.AllSeverities = GetNotificationSeverities();
            return View("EditAssetNotification", ean);
        }

        [AuthorizeByPermission(PermissionNames.ManageAssetNotifications)]
        public AssetNotifications UpdateAssetNotificationFromModel(AssetNotifications an, EditAssetNotificationModel ean)
        {
            an.ExpirationTime = ean.ExpirationTime;
            an.MessageSeverity = ean.SeverityID;
            an.Message = ean.Message;
            return an;
        }

        [AuthorizeByPermission(PermissionNames.ManageAssetNotifications)]
        public AssetNotifications CreateAssetNotificationFromModel(AssetNotifications an, CreateAssetNotificationModel can)
        {
            an.Message = can.Message;
            an.CreateUser = can.CreateUser;
            an.StartTime = can.StartTime;
            an.ExpirationTime = can.ExpirationTime;
            an.MessageSeverity = can.SeverityID;
            an.ParentDataAsset = _dataAssetContext.GetDataAsset(can.DataAssetID);
            
            return an;
        }

        private IEnumerable<SelectListItem> GetNotificationSeverities()
        {
            List<SelectListItem> items = Enum.GetValues(typeof(NotificationSeverity)).Cast<NotificationSeverity>().Select(v => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }).ToList();

            return items;
        }

        private IEnumerable<SelectListItem> GetDataAssetsList()
        {
            List<SelectListItem> assets = _dataAssetContext.GetDataAssets().Select(v => new SelectListItem { Text = v.DisplayName, Value = ((int)v.Id).ToString() }).ToList();

            return assets;
        }

    }
}