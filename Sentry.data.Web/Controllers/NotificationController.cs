using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sentry.data.Core;

namespace Sentry.data.Web.Controllers
{
    public class NotificationController : BaseController
    {

        private INotificationService _notificationService;
        private UserService _userService;

        public NotificationController(INotificationService notificationService, UserService userService)
        {
            _notificationService = notificationService;
            _userService = userService;
        }

        public ActionResult ManageNotification()
        {
            ManageNotificationViewModel model = new ManageNotificationViewModel()
            {
                CanModifyNotifications = _notificationService.CanUserModifyNotifications()
            };
            return View(model);
        }

        [HttpGet]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.CAN_MODIFY_NOTIFICATIONS)]
        public ActionResult CreateNotification()
        {
            CreateAssetNotificationModel canm = new CreateAssetNotificationModel();
            canm.AllDataAssets = GetDataAssetsList();
            canm.AllSeverities = GetNotificationSeverities();
            canm.CreateUser = user.AssociateId;
            canm.DisplayCreateUser = _associateInfoService.GetAssociateInfo(user.AssociateId);
            canm.StartTime = DateTime.Now; //preset Start Time
            canm.ExpirationTime = DateTime.Now.AddHours(1);

            return View(canm);
        }

        [HttpPost]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.CAN_MODIFY_NOTIFICATIONS)]
        public ActionResult CreateAssetNotification(CreateAssetNotificationModel canm)
        {
            IApplicationUser user = _userService.GetCurrentUser();

            if (!user.CanManageAssetAlerts)
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

        public JsonResult GetAssetNotificationInfoForGrid(int Id, [ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest dtRequest)
        {
            IEnumerable<BaseNotificationModel> files = null;
            if (Id > 0)
            {
                files = _dataAssetContext.GetAssetNotificationsByDataAssetId(Id).Select((an) => new BaseNotificationModel(an, _associateInfoService));
            }
            else
            {
                files = _dataAssetContext.GetAllAssetNotifications().Select((an) => new BaseNotificationModel(an, _associateInfoService));
            }

            DataTablesQueryableAdapter<BaseNotificationModel> dtqa = new DataTablesQueryableAdapter<BaseNotificationModel>(files.AsQueryable(), dtRequest);
            return Json(dtqa.GetDataTablesResponse(), JsonRequestBehavior.AllowGet);
        }


        [HttpGet()]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.CAN_MODIFY_NOTIFICATIONS)]
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
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.CAN_MODIFY_NOTIFICATIONS)]
        public ActionResult EditAssetNotification(EditAssetNotificationModel ean)
        {
            IApplicationUser user = _userService.GetCurrentUser();

            if (!user.CanManageAssetAlerts)
            {
                throw new UnauthorizedAccessException();
            }

            AssetNotifications an = _dataAssetContext.GetAssetNotificationByID(ean.NotificationId);

            if (ean.ExpirationTime <= ean.StartTime)
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

        public AssetNotifications UpdateAssetNotificationFromModel(AssetNotifications an, EditAssetNotificationModel ean)
        {
            an.ExpirationTime = ean.ExpirationTime;
            an.MessageSeverity = ean.SeverityID;
            an.Message = ean.Message;
            return an;
        }

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