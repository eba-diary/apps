﻿using System;
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

            List<SelectListItem> AreaList = new List<SelectListItem>();
            foreach (var item in _notificationService.GetAssetsForUserSecurity())
            {
                AreaList.Add(new SelectListItem { Text = item.DisplayName, Value = Core.GlobalConstants.Notifications.DATAASSET_TYPE + "_" + item.Id });
            }
            foreach (var item in _notificationService.GetBusinessAreasForUserSecurity())
            {
                AreaList.Add(new SelectListItem { Text = item.Name, Value = Core.GlobalConstants.Notifications.BUSINESSAREA_TYPE + "_" + item.Id });
            }

            model.AllSeverities = default(NotificationSeverity).ToEnumSelectList();
            model.AllDataAssets = AreaList;
            return View("ModifyNotification",model);
        }

        public JsonResult GetNotificationInfoForGrid([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest dtRequest)
        {
            List<NotificationModel> files = _notificationService.GetAllNotifications().ToWeb();
            DataTablesQueryableAdapter<NotificationModel> dtqa = new DataTablesQueryableAdapter<NotificationModel>(files.AsQueryable(), dtRequest);
            return Json(dtqa.GetDataTablesResponse(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult AccessRequest()
        {
            NotificationAccessRequestModel model = new NotificationAccessRequestModel()
            {
                AllPermissions = _notificationService.GetPermissionsForAccessRequest().ToModel(),
                AllSecurableObjects = _notificationService.GetAssetsForAccessRequest().Select(x=> new SelectListItem() { Value = x.Id.ToString(), Text = x.DisplayName}).ToList(),
                AllApprovers = new List<SelectListItem>()
            };
            model.AllSecurableObjects.Insert(0, new SelectListItem() { Value = "0", Text = "Select a notification area" });
            return PartialView("NotificationAccessRequest", model);
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

        public JsonResult GetApproversByDataAsset(int dataAssetId)
        {
            var owners = _notificationService.GetApproversByDataAsset(dataAssetId).Select(x=> new SelectListItem() {Value = x.Key, Text = x.Value }).ToList();
            return Json(owners, JsonRequestBehavior.AllowGet);
        }

    }
}