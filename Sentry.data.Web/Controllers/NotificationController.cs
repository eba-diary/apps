using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.DataTables.Mvc;
using Sentry.DataTables.QueryableAdapter;
using Sentry.DataTables.Shared;

namespace Sentry.data.Web.Controllers
{
    [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATA_ASSET_VIEW)]
    public class NotificationController : BaseController
    {

        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
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
        public ActionResult ModifyNotification(int notificationId = 0)
        {
            if (_notificationService.CanUserModifyNotifications())
            {
                NotificationModel model = _notificationService.GetNotificationModelForModify(notificationId).ToWeb();
                return View(model);
            }
            return View("NotFound");
        }

        [HttpPost]
        public ActionResult SubmitNotification(NotificationModel model)
        {
            if (!_notificationService.CanUserModifyNotifications())
            {
                return View("NotFound");
            }

            AddCoreValidationExceptionsToModel(model.Validate());
            if (ModelState.IsValid)
            {
                _notificationService.SubmitNotification(model.ToCore());
                ManageNotificationViewModel vm = new ManageNotificationViewModel()
                {
                    CanModifyNotifications = _notificationService.CanUserModifyNotifications()
                };
                return View("ManageNotification", vm);
            }

            model.AllSeverities = default(NotificationSeverity).ToEnumSelectList();
            model.AllDataAssets = _notificationService.GetAssetsForUserSecurity().Select(v => new SelectListItem { Text = v.DisplayName, Value = v.Id.ToString() }).ToList();
            return View("ModifyNotification",model);
        }

        public JsonResult GetNotificationInfoForGrid([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest dtRequest)
        {
            List<NotificationModel> files = _notificationService.GetNotificationsForDataAsset().ToWeb();
            DataTablesQueryableAdapter<NotificationModel> dtqa = new DataTablesQueryableAdapter<NotificationModel>(files.AsQueryable(), dtRequest);
            return Json(dtqa.GetDataTablesResponse(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult AccessRequest()
        {
            NotificationAccessRequestModel model = new NotificationAccessRequestModel()
            {
                AllPermissions = _notificationService.GetPermissionsForAccessRequest().ToModel()
            };
            return View("NotificationAccessRequest", model);
        }

        [HttpPost]
        public ActionResult SubmitAccessRequest(NotificationAccessRequestModel model)
        {
            AccessRequest ar = model.ToCore();
            string ticketId = _notificationService.RequestAccess(ar);

            if (string.IsNullOrEmpty(ticketId))
            {
                return PartialView("_Success", new SuccessModel("There was an error processing your request.", "", false));
            }
            else
            {
                return PartialView("_Success", new SuccessModel("Notification access was successfully requested.", "HPSM Change Id: " + ticketId, true));
            }
        }

        public ActionResult GetNotificationPartialView(int notificationId)
        {
            return View("_Notification",_notificationService.GetNotificationModelForDisplay(notificationId).ToWeb());
        }

    }
}