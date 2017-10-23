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
        private List<DataAsset> das;

        public DataAssetController(IDataAssetProvider dap, MetadataRepositoryService metadataRepositoryService, IDatasetContext dsContext, IAssociateInfoProvider associateInfoService)
        {
            _dataAssetProvider = dap;
            _metadataRepositoryService = metadataRepositoryService;
            _dsContext = dsContext;
            _associateInfoService = associateInfoService;
        }
        
        public ActionResult Index(int id)
        {
            id = (id == 0) ? das[0].Id : id;

            DataAsset da = _dataAssetProvider.GetDataAsset(id);
            da.AssetNotifications = _dsContext.GetAssetNotificationsByDataAssetId(da.Id).Where(w => w.StartTime < DateTime.Now && w.ExpirationTime > DateTime.Now).ToList();
            da.LastUpdated = DateTime.Now;
            da.Status = 1;
            das = new List<DataAsset>(_dataAssetProvider.GetDataAssets());
            ViewBag.DataAssets = das;
            //ViewData["fluid"] = true;

            if (da != null) { return View(da); }
            else { return RedirectToAction("NotFound", "Error"); }
        }

        public ActionResult DataAsset(string assetName)
        {
            assetName = (assetName == null) ? das[0].Name : assetName;

            DataAsset da = _dataAssetProvider.GetDataAsset(assetName);
            da.AssetNotifications = _dsContext.GetAssetNotificationsByDataAssetId(da.Id).Where(w => w.StartTime < DateTime.Now && w.ExpirationTime > DateTime.Now).ToList();

            da.LastUpdated = DateTime.Now;
            da.Status = 1;
            das = new List<DataAsset>(_dataAssetProvider.GetDataAssets());
            ViewBag.DataAssets = das;            
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
            if(an.ExpirationTime != ean.ExpirationTime) { an.ExpirationTime = ean.ExpirationTime; }
            return an;
        }

        private IEnumerable<SelectListItem> GetNotificationSeverities()
        {
            List<SelectListItem> items = Enum.GetValues(typeof(NotificationSeverity)).Cast<NotificationSeverity>().Select(v => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }).ToList();

            return items;
        }
    }
}