using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.DataTables.QueryableAdapter;
using Sentry.DataTables.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Sentry.data.Web.Controllers
{
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

        public JsonResult GetNotificationInfoForGrid(DataTablesRequest dtRequest)
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
        public async Task<ActionResult> SubmitAccessRequest(NotificationAccessRequestModel model)
        {
            AccessRequest ar = model.ToCore();
            string ticketId = null;

            try
            {
                ticketId = await _notificationService.RequestAccess(ar);
            }
            catch (Exception ex)
            {
                Logger.Error("Failure to submit notification access request", ex);
            }

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
                List<BusinessAreaSubscription> parents = BuildAllParentsWithAssignedChildren(dbSubscriptions);
                
                //STEP 5:   UPDATE SUBSCRIPTIONS FROM ALL CURRENT EVENTTYPES
                UpdateUserSubscriptionsFromAllEventTypes(_notificationService.GetEventTypes(model.group).ToList(), ref parents, (BusinessAreaType)model.businessAreaID);

                //STEP 6:  UPDATE CURRENT SUBCRIPTIONS
                model.SubscriptionsBusinessAreas = parents.OrderBy(o => o.EventType.Type_ID).ToList();

                //STEP 7:   CREATE MODEL VERSION FOR VIEW TO USE
                model.SubscriptionsBusinessAreaModels = model.SubscriptionsBusinessAreas.ToWeb();
            }

            return PartialView("_SubscribeHero", model);
        }

        private List<BusinessAreaSubscription> BuildAllParentsWithAssignedChildren(List<BusinessAreaSubscription> subs)
        {
            //GRAB PARENTS ONLY
            List<BusinessAreaSubscription> parents = subs.Where(w => w.EventType.ParentDescription == null).ToList();

            //ASSIGN CHILDREN TO EACH PARENT
            foreach (BusinessAreaSubscription parent in parents)
            {
                //grab subs that match eventtype except itself
                parent.Children = subs.Where(w => w.EventType.ParentDescription == parent.EventType.Description && w.ID != parent.ID).ToList();
            }

            return parents;
        }


        //THIS METHOD IS A RECURSIVE METHOD THAT ITERATES THROUGH ALL LATEST EVENTTYPES AND ENSURES THAT USER HAS A SUBCRIPTION TO EACH ONE
        //IF A EVENTTYPE HAS CHILDREN, ENSURE CHILDREN ARE LOADED PROPERLY ALONG WITH WHICH ONES ARE SELECTED AKA HAVE INTERVAL != Never
        private void UpdateUserSubscriptionsFromAllEventTypes(List<EventType> eventTypes, ref List<BusinessAreaSubscription> dbSubscriptions, BusinessAreaType businessAreaType )
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
                        UpdateUserSubscriptionsFromAllEventTypes(et.ChildEventTypes, ref tempChildren, businessAreaType);
                        newSubscription.Children = tempChildren;
                    }
                    dbSubscriptions.Add(newSubscription);
                }
                else if (et.ChildEventTypes != null && et.ChildEventTypes.Count > 0)                            //UPDATE SCENARIO:  SUBSCRIPTION EXISTS BUT NEED TO CHECK IF THEY HAVE ALL CHILDREN
                {
                    tempChildren = dbSubscription.Children;
                    UpdateUserSubscriptionsFromAllEventTypes(et.ChildEventTypes, ref tempChildren, businessAreaType);
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
            List<BusinessAreaSubscription> newList = sm.SubscriptionsBusinessAreaModels.ToDto();

            //2: REBUILD ALL CHILD SUBSCRIPTIONS PROPERTIES BECAUSE MVC HAS NO COMPLEX DATA STRUCTURE MODEL BINDING, EXISTING PARENT SUBSCRIPTIONS WILL NOT BE REPLACED
            UpdateUserSubscriptionsFromAllEventTypes(_notificationService.GetEventTypes(sm.group).ToList(), ref newList, (BusinessAreaType)sm.businessAreaID);

            //3: ANY CHILD SUBCRIPTION SELECTED, ASSIGN PARENT's INTERVAL TYPE
            UpdateChildIntervalsAndPrimaryKey(sm.group, ref newList);

            //4:UPDATE MODEL TO INCLUDE NEW CHILDREN
            sm.SubscriptionsBusinessAreas = newList;

            SubscriptionDto dto = sm.ToDto();
            _notificationService.CreateUpdateSubscription(dto);
            return Redirect(Request.UrlReferrer.PathAndQuery);
        }

        //UPDATE CHILD INTERVALS TO PARENTS IF CHILD EXISTS IN ChildBusinessAreaSubscriptionSelections
        //ALSO UPDATE the ID OF CHILD SINCE MVC RAZOR CAN'T BIND AND SAVE NESTED CHILDREN IN MODEL
        //UPDATE NON SELECTED CHILDREN TO NEVER
        private void UpdateChildIntervalsAndPrimaryKey(EventTypeGroup group, ref List<BusinessAreaSubscription> newList)
        {
            List<BusinessAreaSubscription> dbSubscriptions = _notificationService.GetAllUserSubscriptionsFromDatabase(group).OrderBy(o => o.EventType.Type_ID).ToList();
            
            //CREATE NEVER INTERVAL
            Interval neverInterval = new Interval();
            neverInterval.Interval_ID = (int)IntervalType.Never;

            //LOOP THROUGH EACH PARENT IN LIST AND ADJUST CHILDREN
            foreach (BusinessAreaSubscription parent in newList)
            {
                //PROCESS PARENTS WITH CHILDREN ONLY, PARENTS WITHOUT CHILDREN KEEP EXISTING SELECTION
                if(parent.Children != null && parent.Children.Count > 0)
                {
                    //IF CHILDREN SELECTED, MARK THOSE WITH PARENT INTERVAL, MARK REST WITH NEVER INTERVAL
                    if(parent.ChildrenSelections != null && parent.ChildrenSelections.Any())
                    {
                        //UPDATE ALL SELECTED CHILDREN TO HAVE PARENTS INTERVAL
                        List<BusinessAreaSubscription> childrenWithParentsInterval = parent.Children.Where(w => parent.ChildrenSelections.Contains(w.EventType.Type_ID)).ToList();
                        UpdateChildrenToHaveInterval(dbSubscriptions, ref childrenWithParentsInterval, parent.Interval);

                        //UPDATE NON SELECTED CHILDREN TO HAVE NEVER INTERVAL SINCE THEY WERENT SELECTED
                        List<BusinessAreaSubscription> childrenWithNeverInterval = parent.Children.Where(w => !parent.ChildrenSelections.Contains(w.EventType.Type_ID)).ToList();
                        UpdateChildrenToHaveInterval(dbSubscriptions, ref childrenWithNeverInterval, neverInterval);
                    }
                    else    //NO SELECTIONS EXISTED SO BASICALY MARK EVERYTHING WITH INTERVAL = NEVER
                    {
                        //UPDATE PARENT INTERVAL
                        parent.Interval = neverInterval;                                     

                        //UPDATE ALL CHILDREN TO HAVE NEVER INTERVAL
                        List<BusinessAreaSubscription> tempChildren = parent.Children;
                        UpdateChildrenToHaveInterval(dbSubscriptions, ref tempChildren, neverInterval);
                        parent.Children = tempChildren;
                    }
                }
            }
        }

        //UPDATE CHILDREN WITH PASSED IN INTERVAL, CALL DB TO FIND ID
        private void UpdateChildrenToHaveInterval(List<BusinessAreaSubscription> dbSubscriptions, ref List<BusinessAreaSubscription> children, Interval interval)
        {
            foreach (BusinessAreaSubscription child in children)     //UPDATE CHILDREN INTERVALS SINCE NOTHING WAS SELECTED
            {
                child.Interval = interval;

                BusinessAreaSubscription dbChild = dbSubscriptions.FirstOrDefault(w => w.BusinessAreaType == child.BusinessAreaType && w.EventType == child.EventType && w.SentryOwnerName == child.SentryOwnerName);
                if (dbChild != null)
                {
                    child.ID = dbChild.ID;
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