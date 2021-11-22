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
        private readonly UserService _userService;

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
        public ActionResult ModifyNotification(int notificationId = 0)
        {
            if (_notificationService.CanUserModifyNotifications())
            {
                NotificationModel model = _notificationService.GetNotificationModelForModify(notificationId).ToWeb();
                return View(model);
            }
            return View("NotFound");
        }

        [HttpGet]
        public ActionResult ExpireNotification(int notificationId = 0)
        {
            if (!_notificationService.CanUserModifyNotifications())
            {
                return View("NotFound");
            }
            
            _notificationService.AutoExpire(notificationId);

            //go back to ManageNotifications page to see refreshed notifications
            return Redirect("/Notification/ManageNotification");                                    
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
                model.NotificationId = _notificationService.SubmitNotification(model.ToCore());
                ManageNotificationViewModel vm = new ManageNotificationViewModel()
                {
                    CanModifyNotifications = _notificationService.CanUserModifyNotifications()
                };

                return Redirect("/Notification/ManageNotification");
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
            model.AllNotificationCategories = default(NotificationCategory).ToEnumSelectList();
            return View("ModifyNotification",model);
        }

        public JsonResult GetNotificationInfoForGrid([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest dtRequest)
        {
            List<NotificationModel> files = _notificationService.GetAllNotifications().ToWeb();
            DataTablesQueryableAdapter<NotificationModel> dtqa = new DataTablesQueryableAdapter<NotificationModel>(files.AsQueryable(), dtRequest);
            JsonResult result = Json(dtqa.GetDataTablesResponse(), JsonRequestBehavior.AllowGet);
            result.MaxJsonLength = Int32.MaxValue;              //need to set MaxJsonLength to avoid 500 exceptions because of large json coming back since notifications contain images now
            return result;
        }

        public ActionResult AccessRequest()
        {
            NotificationAccessRequestModel model = new NotificationAccessRequestModel()
            {
                AllPermissions = _notificationService.GetPermissionsForAccessRequest().ToModel(),
                AllSecurableObjects = _notificationService.GetBusinessAreasForAccessRequest().Select(x=> new SelectListItem() { Value = x.Id.ToString(), Text = x.Name}).ToList(),
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
                return PartialView("_Success", new SuccessModel("Notification access was successfully requested.", "Change Id: " + ticketId, true));
            }
        }

        public JsonResult GetApproversByBusinessArea(int businessAreaId)
        {
            var owners = _notificationService.GetApproversByBusinessArea(businessAreaId).Select(x=> new SelectListItem() {Value = x.Key, Text = x.Value }).ToList();
            return Json(owners, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetNotifications(BusinessAreaType businessAreaType)
        {
             return Json(_notificationService.GetNotificationForBusinessArea(businessAreaType).ToModel(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SubscribeDisplay(int group)
        {
            SubscriptionModel sm = new SubscriptionModel();
            sm.group = (EventTypeGroup) group;                                                                                                               //need to teach MODEL what KIND of Subscription it is,either DATASET=1 or BUSINESSAREA=2
            sm.AllIntervals  = _notificationService.GetAllIntervals().Select((c) => new SelectListItem { Text = c.Description, Value = c.Interval_ID.ToString() });
            sm.SentryOwnerName = _userService.GetCurrentUser().AssociateId;

            //BUSINESSAREA      AUSTIN:  FUTURE we will put DATASET in here too, you can maybe even add another function to pass stuff here and make below even more generic
            if(sm.group == EventTypeGroup.BusinessArea)
            {
                BusinessAreaType bat = BusinessAreaType.PersonalLines;
                sm.businessAreaID = (int)BusinessAreaType.PersonalLines;

                //get list of subscriptions the user has saved
                List<BusinessAreaSubscription> tempCurrentSubscriptionsBusinessArea = _notificationService.GetAllUserSubscriptions(sm.group).OrderBy(o => o.EventType.Type_ID).ToList();

                //add any missing subscriptions the user may not have saved
                foreach (Core.EventType et in _notificationService.GetEventTypes(sm.group))
                {
                    if (!tempCurrentSubscriptionsBusinessArea.Any(x => x.EventType.Type_ID == et.Type_ID))
                    {
                        BusinessAreaSubscription subscription = new BusinessAreaSubscription();
                        subscription.BusinessAreaType = bat;
                        subscription.SentryOwnerName = _userService.GetCurrentUser().AssociateId;
                        subscription.EventType = et;
                        subscription.Interval = _notificationService.GetInterval("Never");
                        subscription.ID = 0;

                        tempCurrentSubscriptionsBusinessArea.Add(subscription);
                    }
                }

                sm.CurrentSubscriptionsBusinessArea = tempCurrentSubscriptionsBusinessArea.OrderBy(o => o.EventType.Type_ID).ToList();
            }
            
            return PartialView("_SubscribeHero", sm);
        }

        [HttpPost]
        public ActionResult SubscribeUpdate(SubscriptionModel sm)
        {
            SubscriptionDto dto = sm.ToDto();
            _notificationService.CreateUpdateSubscription(dto);
            return Redirect(Request.UrlReferrer.PathAndQuery);
        }

        [HttpGet]
        //method called by Notification.js to get notification title and message from DB so JS can decode it and load Quill
        public JsonResult GetQuillContents(int notificationId)
        {
            NotificationModel model = _notificationService.GetNotificationModelForModify(notificationId).ToWeb();
            JsonResult result = Json(new { message = model.Message, title = model.Title }, JsonRequestBehavior.AllowGet);
            return result;
        }

    }
}