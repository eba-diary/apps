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
        private readonly IDataFeatures _featureFlags;

        public NotificationController(INotificationService notificationService, UserService userService, IDataFeatures featureFlags)
        {
            _notificationService = notificationService;
            _userService = userService;
            _featureFlags = featureFlags;
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
                model.CLA3882_DSC_NOTIFICATION_SUBCATEGORY = _featureFlags.CLA3882_DSC_NOTIFICATION_SUBCATEGORY.GetValue();
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
            model.AllNotificationSubCategoriesReleaseNotes = default(NotificationSubCategoryReleaseNotes).ToEnumSelectList();
            model.AllNotificationSubCategoriesNews = default(NotificationSubCategoryNews).ToEnumSelectList();


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
            //STEP 1:   CREATE MODEL TO HOLD ALL NECESSARY INFO FOR PARTIAL VIEW _SubscribeHero.  NOTE: Passed in Group determines which subscriptions will be displayed of user 
            SubscriptionModel model = new SubscriptionModel();
            model.group = (EventTypeGroup) group;                      //DATASET=1 or BUSINESSAREA=2   or BUSINESSAREA_DSC=3
            model.AllIntervals  = _notificationService.GetAllIntervals().Select((c) => new SelectListItem { Text = c.Description, Value = c.Interval_ID.ToString() });
            model.SentryOwnerName = _userService.GetCurrentUser().AssociateId;
            model.businessAreaID = (model.group == EventTypeGroup.BusinessArea) ? (int) BusinessAreaType.PersonalLines : (int) BusinessAreaType.DSC; 

            //STEP 2: PROCESS ONLY BUSINESSAREA and BUSINESSAREA_DSC.  FUTURE:  ADD DATASET
            if (model.group == EventTypeGroup.BusinessArea || model.group == EventTypeGroup.BusinessAreaDSC)
            {
                //STEP 3:  GET ALL SUBSCRIPTIONS USER HAS CURRENTLY
                List<BusinessAreaSubscription> dbSubscriptions = _notificationService.GetAllUserSubscriptionsFromDatabase(model.group).OrderBy(o => o.EventType.Type_ID).ToList();
                List<BusinessAreaSubscription> parents = AssignChildren(dbSubscriptions);
                

                //STEP 4:  GET ALL EVENTTYPES IN GROUP 
                IEnumerable<EventType> eventTypes = _notificationService.GetEventTypes(model.group);

                
                //STEP 5:   UPDATE SUBSCRIPTIONS
                UpdateSubscriptions(eventTypes.ToList(), ref parents, (BusinessAreaType)model.businessAreaID);

                //STEP 6:  UPDATE CURRENT SUBCRIPTIONS
                model.CurrentSubscriptionsBusinessArea = parents.OrderBy(o => o.EventType.Type_ID).ToList();

                model.CurrentSubscriptionsBusinessAreaModels = model.CurrentSubscriptionsBusinessArea.ToWeb();
            }

            return PartialView("_SubscribeHero", model);
        }

        private List<BusinessAreaSubscription> AssignChildren(List<BusinessAreaSubscription> subs)
        {
            List<BusinessAreaSubscription> parents = subs.Where(w => w.EventType.ParentDescription == null).ToList();

            foreach (BusinessAreaSubscription parent in parents)
            {
                //grab subs that match eventtype except itself
                parent.Children = subs.Where(w => w.EventType.ParentDescription == parent.EventType.Description && w.ID != parent.ID).ToList();
            }

            return parents;
        }


        //THIS METHOD IS A RECURSIVE METHOD THAT ITERATES THROUGH ALL EVENTTYPES AND ENSURES THAT USER HAS A SUBCRIPTION TO EACH ONE
        //IF A EVENTTYPE HAS CHILDREN, MAKE SURE TO ADD THAT TO BusinessAreaSubscription.childBusinessAreaSubscriptions
        private void UpdateSubscriptions(List<EventType> eventTypes, ref List<BusinessAreaSubscription> dbSubscriptions, BusinessAreaType businessAreaType )
        {
            //LOOP THROUGH EVENTTYPES
            foreach(EventType et in eventTypes)
            {
                //INITIALIZE NEW SUBSCRIPTION IF NOT EXIST
                if(dbSubscriptions == null)
                {
                    dbSubscriptions = new List<BusinessAreaSubscription>();
                }
                
                BusinessAreaSubscription dbSubscription = dbSubscriptions.FirstOrDefault(w => w.EventType.Type_ID == et.Type_ID);   //CHECK IF SUBSCRIPTION EXISTS TO GIVEN EVENTTYPE
                List<BusinessAreaSubscription> tempChildren = null;
                if (dbSubscription == null)                                                                      //INSERT SCENARIO:  SUBSCRIPTION = NULL MEANS ONE NEEDS TO BE ADDED   
                {
                    BusinessAreaSubscription newSubscription = new BusinessAreaSubscription();
                    newSubscription.BusinessAreaType = businessAreaType;
                    newSubscription.SentryOwnerName = _userService.GetCurrentUser().AssociateId;
                    newSubscription.EventType = et;
                    newSubscription.Interval = _notificationService.GetInterval("Never");
                    newSubscription.ID = 0;

                    if (et.ChildEventTypes != null && et.ChildEventTypes.Count > 0)
                    {
                        tempChildren = newSubscription.Children;                          //NOTE: CANT PASS ANY PARENT.Property AS REF in C#, so make a new var and UPDATE LATER
                        UpdateSubscriptions(et.ChildEventTypes, ref tempChildren, businessAreaType);
                        newSubscription.Children = tempChildren;
                    }
                    dbSubscriptions.Add(newSubscription);
                }
                else if (et.ChildEventTypes != null && et.ChildEventTypes.Count > 0)                            //UPDATE SCENARIO:  SUBSCRIPTION EXISTS BUT NEED TO CHECK IF THEY HAVE ALL CHILDREN
                {
                    tempChildren = dbSubscription.Children;
                    UpdateSubscriptions(et.ChildEventTypes, ref tempChildren, businessAreaType);
                    dbSubscription.Children = tempChildren;

                    if (dbSubscription.ChildrenSelections == null)                        //ONLY UPDATE IF NULL, IF MODEL HAS PREFILLED KEEP THEM SINCE THIS MAY BE COMING FROM SubscribeUpdate()
                    {
                        dbSubscription.ChildrenSelections = GetChildBusinessAreaSubscriptionSelections(dbSubscription.Children);
                    }
                    
                }
            }
        }

        private List<int> GetChildBusinessAreaSubscriptionSelections(List<BusinessAreaSubscription> subs)
        {
            List<int> myList = subs.Where(w => w.Interval.Interval_ID != (int)IntervalType.Never).Select(s => s.EventType.Type_ID).ToList();
            return myList;
        }

        [HttpPost]
        public ActionResult SubscribeUpdate(SubscriptionModel sm)
        {
            //1: CONVERT MODELS TO DTOS
            List<BusinessAreaSubscription> newList = sm.CurrentSubscriptionsBusinessAreaModels.ToDto();

            //2: REBUILD ALL CHILD SUBSCRIPTIONS PROPERTIES BECAUSE MVC HAS NO COMPLEX DATA STRUCTURE MODEL BINDING, EXISTING PARENT SUBSCRIPTIONS WILL NOT BE REPLACED
            IEnumerable<EventType> eventTypes = _notificationService.GetEventTypes(sm.group);
            UpdateSubscriptions(eventTypes.ToList(), ref newList, (BusinessAreaType)sm.businessAreaID);

            //3: ANY CHILD SUBCRIPTION SELECTED, ASSIGN PARENT's INTERVAL TYPE
            UpdateChildIntervalsAndPrimaryKey(sm.group, ref newList);

            //4:UPDATE MODEL TO INCLUDE NEW CHILDREN
            sm.CurrentSubscriptionsBusinessArea = newList;

            SubscriptionDto dto = sm.ToDto();
            _notificationService.CreateUpdateSubscription(dto);
            return Redirect(Request.UrlReferrer.PathAndQuery);
        }

        //UPDATE CHILD INTERVALS TO PARENTS IF CHILD EXISTS IN ChildBusinessAreaSubscriptionSelections
        //ALSO UPDATE the ID OF CHILD SINCE MVC RAZOR CAN'T BIND AND SAVE NESTED CHILDREN IN MODEL
        private void UpdateChildIntervalsAndPrimaryKey(EventTypeGroup group, ref List<BusinessAreaSubscription> newList)
        {
            List<BusinessAreaSubscription> dbSubscriptions = _notificationService.GetAllUserSubscriptionsFromDatabase(group).OrderBy(o => o.EventType.Type_ID).ToList();

            foreach (BusinessAreaSubscription parent in newList)
            {
                if(parent.ChildrenSelections != null && parent.ChildrenSelections.Count() > 0)
                {
                    List<BusinessAreaSubscription> children = parent.Children.Where(w => parent.ChildrenSelections.Contains(w.EventType.Type_ID)).ToList();
                    foreach (BusinessAreaSubscription child in children)
                    {
                        child.Interval = parent.Interval;

                        BusinessAreaSubscription dbChild = dbSubscriptions.FirstOrDefault(w => w.BusinessAreaType == child.BusinessAreaType && w.EventType == child.EventType && w.SentryOwnerName == child.SentryOwnerName);
                        if(dbChild != null)
                        {
                            child.ID = dbChild.ID;
                        }
                    }
                }
            }
        }

        



        [HttpGet]
        //method called by Notification.js to get notification title and message from DB so JS can decode it and load Quill
        public JsonResult GetQuillContents(int notificationId)
        {
            NotificationModel model = _notificationService.GetNotificationModelForModify(notificationId).ToWeb();
            model.CLA3882_DSC_NOTIFICATION_SUBCATEGORY = _featureFlags.CLA3882_DSC_NOTIFICATION_SUBCATEGORY.GetValue();
            JsonResult result = Json(new { message = model.Message, title = model.Title }, JsonRequestBehavior.AllowGet);
            return result;
        }

    }
}