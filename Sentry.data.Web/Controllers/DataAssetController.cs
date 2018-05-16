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

namespace Sentry.data.Web.Controllers
{
    public class DataAssetController : BaseController
    {
        private IDataAssetProvider _dataAssetProvider;
        private MetadataRepositoryService _metadataRepositoryService;
        private IDatasetContext _dsContext;
        private IAssociateInfoProvider _associateInfoService;
        private UserService _userService;
        private List<DataAsset> das;

        public DataAssetController(IDataAssetProvider dap, MetadataRepositoryService metadataRepositoryService, IDatasetContext dsContext, IAssociateInfoProvider associateInfoService, UserService userService)
        {
            _dataAssetProvider = dap;
            _metadataRepositoryService = metadataRepositoryService;
            _dsContext = dsContext;
            _associateInfoService = associateInfoService;
            _userService = userService;
        }

        [AuthorizeByPermission(PermissionNames.DatasetAsset)]
        public ActionResult Index(int id)
        {
            das = new List<DataAsset>(_dsContext.GetDataAssets());
            ViewBag.DataAssets = das;
            ViewBag.CanUserSwitch = SharedContext.CurrentUser.CanUserSwitch;

            id = (id == 0) ? das.FirstOrDefault(x => x.DisplayName.ToLower().Contains("sera pl")).Id : id;

            DataAsset da = _dsContext.GetDataAsset(id);

            if (da != null)
            {
                da.AssetNotifications = _dsContext.GetAssetNotificationsByDataAssetId(da.Id).Where(w => w.StartTime < DateTime.Now && w.ExpirationTime > DateTime.Now).ToList();
                da.LastUpdated = DateTime.Now;
                da.Status = 1;
            }

            //ViewData["fluid"] = true;

            if (da != null) { return View(da); }
            else { return RedirectToAction("NotFound", "Error"); }
        }

        [Route("Lineage/{line}")]
        [AuthorizeByPermission(PermissionNames.UserSwitch)]
        public ActionResult Lineage(string line)
        {
            ViewBag.IsLine = true;
            ViewBag.LineName = line;

            return View();
        }


        [Route("Lineage/{line}/{assetName}")]
        [Route("Lineage/{line}/{assetName}/{businessObject}/{sourceElement}")]
        [AuthorizeByPermission(PermissionNames.UserSwitch)]
        public ActionResult Lineage(string line, string assetName, string businessObject, string sourceElement)
        {
            das = new List<DataAsset>(_dsContext.GetDataAssets());

            assetName = (assetName == null) ? das[0].Name : assetName;

            DataAsset da = _dsContext.GetDataAsset(assetName);
            if (da != null)
            {
                da.AssetNotifications = _dsContext.GetAssetNotificationsByDataAssetId(da.Id).Where(w => w.StartTime < DateTime.Now && w.ExpirationTime > DateTime.Now).ToList();
                da.LastUpdated = DateTime.Now;
                da.Status = 1;
            }

            ViewBag.IsLine = false;
            ViewBag.DataAsset = da;

            if (da != null) { return View(da); }
            else { return RedirectToAction("NotFound", "Error"); }
        }

        [Route("DataAsset/Test")]
        public ActionResult Test()
        {
            return View();
        }




        public ActionResult DataAsset(string assetName)
        {
            das = new List<DataAsset>(_dsContext.GetDataAssets());
            ViewBag.DataAssets = das;
            ViewBag.CanUserSwitch = SharedContext.CurrentUser.CanUserSwitch;

            assetName = (assetName == null) ? das[0].Name : assetName;

            DataAsset da = _dsContext.GetDataAsset(assetName);
            if (da != null)
            {
                da.AssetNotifications = _dsContext.GetAssetNotificationsByDataAssetId(da.Id).Where(w => w.StartTime < DateTime.Now && w.ExpirationTime > DateTime.Now).ToList();
                da.LastUpdated = DateTime.Now;
                da.Status = 1;
            }

            //ViewData["fluid"] = true;

            if (da != null) { return View("Index", da); }
            else { return RedirectToAction("NotFound", "Error"); }
        }
        //public ActionResult GetAssetNotifications(int Id)
        //{
        //    List<AssetNotifications> aList = _dataAssetProvider.GetNotificationsById(Id).Where(w => w.StartTime < DateTime.Now && w.ExpirationTime > DateTime.Now).ToList();
        //    return aList;
        //}
        //[AuthorizeByPermission(per)]
        public ActionResult ManageAssetNotification()
        {
            BaseAssetNotificationModel banm = new BaseAssetNotificationModel();
            return View(banm);
        }

        [HttpGet]
        public ActionResult CreateAssetNotification()
        {
            IApplicationUser user = _userService.GetCurrentUser();

            CreateAssetNotificationModel canm = new CreateAssetNotificationModel();
            canm.AllDataAssets = GetDataAssetsList();
            canm.AllSeverities = GetNotificationSeverities();
            canm.CreateUser = user.AssociateId;
            canm.DisplayCreateUser = _associateInfoService.GetAssociateInfo(user.AssociateId);
            canm.StartTime = DateTime.Now; //preset Start Time
            canm.ExpirationTime = DateTime.Now.AddHours(1);
            return PartialView("_CreateAssetNotification",canm);
        }

        [HttpPost]
        public ActionResult CreateAssetNotification(CreateAssetNotificationModel canm)
        {
            AssetNotifications an = new AssetNotifications();
            try
            {
                if (ModelState.IsValid)
                {
                    an = UpdateAssetNotificationFromModel(an, canm);
                    _dsContext.Merge<AssetNotifications>(an);
                    _dsContext.SaveChanges();
                    return AjaxSuccessJson();
                }
            }
            catch (Sentry.Core.ValidationException ex)
            {
                AddCoreValidationExceptionsToModel(ex);
                _dsContext.Clear();
            }
                        
            return CreateAssetNotification();
        }

        //[AuthorizeByPermission(PermissionNames.ManageAssetNotifications)]
        public JsonResult GetAssetNotificationInfoForGrid(int Id, [ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest dtRequest)
        {
            IEnumerable<BaseAssetNotificationModel> files = null;
            if (Id > 0)
            {
                files =  _dsContext.GetAssetNotificationsByDataAssetId(Id).ToList().Select((an) => new BaseAssetNotificationModel(an, _associateInfoService));
            }
            else
            {
                files = _dsContext.GetAllAssetNotifications().ToList().Select((an) => new BaseAssetNotificationModel(an, _associateInfoService));
            }

            DataTablesQueryableAdapter<BaseAssetNotificationModel> dtqa = new DataTablesQueryableAdapter<BaseAssetNotificationModel>(files.AsQueryable(), dtRequest);
            return Json(dtqa.GetDataTablesResponse(), JsonRequestBehavior.AllowGet);
        }

        //[HttpGet()]
        ////[AuthorizeByPermission(PermissionNames.ManageAssetNotifications)]
        //public ActionResult GetEditConfigPartialView(int notificationId)
        //{
        //    AssetNotifications an = _dsContext.GetAssetNotificationByID(notificationId);
        //   // EditDatasetFileConfigModel edfc = new (dfc);

        //    ViewBag.ModifyType = "Edit";

        //    //return PartialView("_EditConfigFile", edfc);
        //}

        [HttpGet()]
        [AuthorizeByPermission(PermissionNames.ManageDataFileConfigs)]
        public ActionResult GetEditAssetNotificationPartialView(int notificationId)
        {
            AssetNotifications an = _dsContext.GetAssetNotificationByID(notificationId);
            EditAssetNotificationModel ean = new EditAssetNotificationModel(an, _associateInfoService);
            ean.AllSeverities = GetNotificationSeverities();
            ean.SeverityID = an.MessageSeverity;  //Preselect current value

            return PartialView("_EditAssetNotification", ean);
        }

        [HttpPost()]
        //[AuthorizeByPermission(PermissionNames.ManageAssetNotifications)]
        public ActionResult EditAssetNotification(EditAssetNotificationModel ean)
        {
            AssetNotifications an = _dsContext.GetAssetNotificationByID(ean.NotificationId);

            try
            {
                if (ModelState.IsValid)
                {
                    an = UpdateAssetNotificationFromModel(an, ean);
                    _dsContext.SaveChanges();
                    return AjaxSuccessJson();
                }
            }
            catch (Sentry.Core.ValidationException ex)
            {
                AddCoreValidationExceptionsToModel(ex);
                _dsContext.Clear();
            }

            //Return partial view when there are errors
            return GetEditAssetNotificationPartialView(ean.NotificationId);
        }

        private AssetNotifications UpdateAssetNotificationFromModel(AssetNotifications an, EditAssetNotificationModel ean)
        {
            an.ExpirationTime = ean.ExpirationTime;
            an.MessageSeverity = ean.SeverityID;
            return an;
        }

        private AssetNotifications UpdateAssetNotificationFromModel(AssetNotifications an, CreateAssetNotificationModel can)
        {
            an.Message = can.Message;
            an.CreateUser = can.CreateUser;
            an.StartTime = can.StartTime;
            an.ExpirationTime = can.ExpirationTime;
            an.MessageSeverity = can.SeverityID;
            an.ParentDataAsset = _dsContext.GetDataAsset(can.DataAssetID);
            
            return an;
        }

        private IEnumerable<SelectListItem> GetNotificationSeverities()
        {
            List<SelectListItem> items = Enum.GetValues(typeof(NotificationSeverity)).Cast<NotificationSeverity>().Select(v => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }).ToList();

            return items;
        }

        private IEnumerable<SelectListItem> GetDataAssetsList()
        {
            List<SelectListItem> assets = _dsContext.GetDataAssets().Select(v => new SelectListItem { Text = v.DisplayName, Value = ((int)v.Id).ToString() }).ToList();

            return assets;
        }

    }
}