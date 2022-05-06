using System;
using System.Collections.Generic;
using System.Linq;
using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.Common.Logging;


namespace Sentry.data.Core
{
    public class NotificationService : INotificationService
    {
        private readonly IDatasetContext _domainContext;
        private readonly ISecurityService _securityService;
        private readonly UserService _userService;
        private readonly IEventService _eventService;
        private readonly IDataFeatures _featureFlags;

        public NotificationService(IDatasetContext domainContext, ISecurityService securityService, UserService userService, IEventService eventService, IDataFeatures dataFeatures)
        {
            _domainContext = domainContext;
            _securityService = securityService;
            _userService = userService;
            _eventService = eventService;
            _featureFlags = dataFeatures;
        }


        public bool CanUserModifyNotifications()
        {
            List<BusinessArea> businessAreas = _domainContext.BusinessAreas.ToList();
            IApplicationUser user = _userService.GetCurrentUser();

            foreach (var ba in businessAreas)
            {
                if (_securityService.GetUserSecurity(ba, user).CanModifyNotifications)
                {
                    return true;
                }
            }

            return false;
        }

        public NotificationDto GetNotificationModelForDisplay(int notificationId)
        {
            IApplicationUser user = _userService.GetCurrentUser();
            NotificationDto model;

            if (notificationId == 0)
            {
                model = new NotificationDto()
                {
                    CreateUser = user.AssociateId,
                    StartTime = DateTime.Now,
                    ExpirationTime = DateTime.MaxValue
                };
            }
            else
            {
                model = _domainContext.Notification.FirstOrDefault(x => x.NotificationId == notificationId).ToModel();
            }
            return model;
        }

        public NotificationDto GetNotificationModelForModify(int notificationId)
        {
            NotificationDto model = GetNotificationModelForDisplay(notificationId);
            model.AllBusinessAreas = GetBusinessAreasForUserSecurity();
            return model;
        }

        public int SubmitNotification(NotificationDto dto)
        {
            Notification notification = null;
            bool addNotification = true;

            if (dto.NotificationId == 0)
            {
                notification = dto.ToCore();
                notification.CreateUser = _userService.GetCurrentUser().AssociateId;
                _domainContext.Add(notification);
                dto.NotificationId = notification.NotificationId;
            }
            else
            {
                notification = _domainContext.Notification.FirstOrDefault(x => x.NotificationId == dto.NotificationId);
                notification.ExpirationTime = dto.ExpirationTime;
                notification.StartTime = dto.StartTime;
                notification.MessageSeverity = dto.MessageSeverity;
                notification.Message = dto.Message;
                notification.NotificationType = dto.NotificationType;
                notification.ParentObject = int.Parse(dto.ObjectId);
                notification.Title = dto.Title;
                addNotification = false;
            }

            //OVERRIDE NotificationCategory TO NULL IF NOT DSC NOTIFICATION
            if (!IsNotificationDSC(dto))
            {
                notification.NotificationCategory = null;
                notification.NotificationSubCategoryReleaseNotes = null;
                notification.NotificationSubCategoryNews = null;
            }
            else
            {
                notification.NotificationCategory = dto.NotificationCategory;

                //NOTE: I tried to use Ternary Operators but i got an error that target-typed conditional expression' is not available in C# 7.3. Please use language version 9.0 or greater
                if (    dto.NotificationCategory.GetDescription() == GlobalConstants.EventType.NOTIFICATION_DSC_RELEASE_NOTES
                        && _featureFlags.CLA3882_DSC_NOTIFICATION_SUBCATEGORY.GetValue()                                            //REMOVE THIS LINE ONLY WHEN FEATURE FLAG IS NA
                )
                {
                    notification.NotificationSubCategoryReleaseNotes = dto.NotificationSubCategoryReleaseNotes;
                }
                else
                {
                    notification.NotificationSubCategoryReleaseNotes = null;
                }


                if (    dto.NotificationCategory.GetDescription() == GlobalConstants.EventType.NOTIFICATION_DSC_NEWS
                        && _featureFlags.CLA3882_DSC_NOTIFICATION_SUBCATEGORY.GetValue()                                            //REMOVE THIS LINE ONLY WHEN FEATURE FLAG IS NA
                )
                {
                    notification.NotificationSubCategoryNews = dto.NotificationSubCategoryNews;
                }
                else
                {
                    notification.NotificationSubCategoryNews = null;
                }
            }

            _domainContext.SaveChanges();
            CreateEvent(addNotification, notification);
            return dto.NotificationId;
        }

        //CHECK IF PASSED in NOTIFICATION IS DSC OR NOT
        private bool IsNotificationDSC(NotificationDto dto)
        {
            bool isNotificationDSC = false;
            if (dto.NotificationType != null 
                && dto.ObjectId != null 
                && dto.NotificationType == GlobalConstants.Notifications.BUSINESSAREA_TYPE 
                && int.Parse(dto.ObjectId) == (int)BusinessAreaType.DSC)
            {
                isNotificationDSC = true;
            }

            return isNotificationDSC;
        }

        public void AutoExpire(int notificationId)
        {
            Notification notification = _domainContext.Notification.FirstOrDefault(x => x.NotificationId == notificationId);

            //set now time to be static so logic below always is dealing the same date
            DateTime nowTime = DateTime.Now;
            if (notification.StartTime >= nowTime)
            {
                notification.StartTime = nowTime.Subtract(new TimeSpan(0, 1, 0));        //set to now minus a minute
            }
            
            notification.ExpirationTime = nowTime;
            _domainContext.SaveChanges();
            CreateEvent(false, notification);
        }

        private void CreateEvent(bool addNotification, Notification notification)
        {
            //the way that other components work when publishing events is to pass a constant which is essentially 
            //equal to the EventType.Description, this is sort of strange because the actual event service then grabs
            //the appropriate EventType based on the description here, because i need to move on I will use this design
            //pattern because it works, so don't judge me.
            string eventTypeDescription = null;

            //BusinessAreaType == DSC has different EventTypes
            if(notification.ParentObject == (int)BusinessAreaType.DSC)
            {
                if (notification.NotificationCategory == NotificationCategory.ReleaseNotes && notification.NotificationSubCategoryReleaseNotes != null)
                {
                    EventType eventTypeReleaseNotes = _domainContext.EventTypes.FirstOrDefault(w => w.ParentDescription == notification.NotificationCategory.GetDescription() && w.DisplayName == notification.NotificationSubCategoryReleaseNotes.GetDescription() );
                    eventTypeDescription = eventTypeReleaseNotes.Description;
                }
                else if (notification.NotificationCategory == NotificationCategory.News && notification.NotificationSubCategoryNews != null)
                {
                    EventType eventTypeNews = _domainContext.EventTypes.FirstOrDefault(w => w.ParentDescription == notification.NotificationCategory.GetDescription() && w.DisplayName == notification.NotificationSubCategoryNews.GetDescription());
                    eventTypeDescription = eventTypeNews.Description;
                }
                else
                {
                    eventTypeDescription = notification.NotificationCategory.GetDescription();
                }
                
            }
            else
            {
                switch (notification.MessageSeverity)
                {
                    case NotificationSeverity.Critical:
                        eventTypeDescription = addNotification ? GlobalConstants.EventType.NOTIFICATION_CRITICAL_ADD : GlobalConstants.EventType.NOTIFICATION_CRITICAL_UPDATE;
                        break;
                    case NotificationSeverity.Warning:
                        eventTypeDescription = addNotification ? GlobalConstants.EventType.NOTIFICATION_WARNING_ADD : GlobalConstants.EventType.NOTIFICATION_WARNING_UPDATE;
                        break;
                    case NotificationSeverity.Info:
                        eventTypeDescription = addNotification ? GlobalConstants.EventType.NOTIFICATION_INFO_ADD : GlobalConstants.EventType.NOTIFICATION_INFO_UPDATE;
                        break;
                    default:
                        Logger.Error("Notification Severity Not Found to log EventType of " + notification.MessageSeverity.ToString() + " for NotificationId = " + notification.NotificationId.ToString());
                        break;
                }
            }


            if (eventTypeDescription != null)
                _eventService.PublishSuccessEventByNotificationId(eventTypeDescription, eventTypeDescription, notification);
        }

        public List<NotificationDto> GetNotificationsForDataAsset()
        {
            throw new NotImplementedException();
            //List<NotificationModel> models = new List<NotificationModel>();
            //List<Notification> notifications = _domainContext.Notification.Fetch(x=> x.ParentObject).ThenFetch(x=> x.Security).ThenFetchMany(x=> x.Tickets).ToList();

            //foreach(var notification in notifications)
            //{
            //    NotificationModel model = notification.ToModel();
            //    IApplicationUser user = _userService.GetByAssociateId(notification.CreateUser);
            //    try
            //    {
            //        model.CreateUser = user.DisplayName;
            //    }catch(Exception ex)
            //    {
            //        Common.Logging.Logger.Error($"Could not get user by Id: {notification.CreateUser}", ex);
            //    }

            //    //UserSecurity us = _securityService.GetUserSecurity(notification.ParentObject, user);
            //    //model.CanEdit = us.CanModifyNotifications;

            //    models.Add(model);
            //}

            //return models;
        }

        public List<NotificationDto> GetNotificationForBusinessArea(BusinessAreaType type)
        {
            BusinessArea ba = _domainContext.BusinessAreas.Where(w => w.Id == (int)type).FirstOrDefault();
            List<NotificationDto> notifications = _domainContext.Notification.Where(w => w.ParentObject == ba.Id).ToList().ToModels(_domainContext, _securityService, _userService);
            return notifications;
        }

        public List<NotificationDto> GetAllNotifications()
        {
            List<Notification> notifications = _domainContext.Notification.ToList();
            List<NotificationDto> models = notifications.ToModels(_domainContext, _securityService, _userService);

            return models;
        }

        /// <summary>
        /// Gets assets for the request dropdown.
        /// </summary>
        /// <returns></returns>
        public List<BusinessArea> GetBusinessAreasForAccessRequest()
        {
            return _domainContext.BusinessAreas.ToList();
        }

        /// <summary>
        /// Gets the assets the user has permission to
        /// </summary>
        public List<DataAsset> GetAssetsForUserSecurity()
        {
            IApplicationUser user = _userService.GetCurrentUser();
            List<DataAsset> assetsWithPermission = new List<DataAsset>();
            //List<DataAsset> dataAssets = _domainContext.DataAsset.FetchSecurityTree(_domainContext);
            List<DataAsset> dataAssets = _domainContext.DataAsset.ToList();
            //only bring back the assests the user has permission to create alerts on.
            foreach(var asset in dataAssets)
            {
                UserSecurity security = _securityService.GetUserSecurity(asset, user);
                if(security != null && security.CanModifyNotifications)
                {
                    assetsWithPermission.Add(asset);
                }
            }

            return assetsWithPermission;
        }

        public List<BusinessArea> GetBusinessAreasForUserSecurity()
        {
            List<BusinessArea> baList = _domainContext.BusinessAreas.ToList();
            IApplicationUser user = _userService.GetCurrentUser();
            List<BusinessArea> baWithPermission = new List<BusinessArea>();
            foreach (var ba in baList)
            {
                UserSecurity security = _securityService.GetUserSecurity(ba, user);
                if (security != null && security.CanModifyNotifications)
                {
                    baWithPermission.Add(ba);
                }
            }

            return baWithPermission;
        }

        public List<Permission> GetPermissionsForAccessRequest()
        {
            return _domainContext.Permission.Where(x => x.SecurableObject == GlobalConstants.SecurableEntityName.BUSINESSAREA).ToList();
        }

        public string RequestAccess(AccessRequest request)
        {
            BusinessArea ba = _domainContext.BusinessAreas.FirstOrDefault(x=> x.Id == request.SecurableObjectId);

            if (ba != null)
            {
                IApplicationUser user = _userService.GetCurrentUser();

                request.PermissionForUserId = user.AssociateId;
                request.PermissionForUserName = user.DisplayName;
                request.SecurableObjectName = ba.Name;
                request.SecurityId = ba.Security.SecurityId;
                request.RequestorsId = user.AssociateId;
                request.RequestorsName = user.DisplayName;
                request.IsProd = bool.Parse(Configuration.Config.GetHostSetting("RequireApprovalHPSMTickets"));
                request.RequestedDate = DateTime.Now;
                request.ApproverId = request.SelectedApprover;
                request.Permissions = _domainContext.Permission.Where(x => request.SelectedPermissionCodes.Contains(x.PermissionCode) &&
                                                                                                                x.SecurableObject == GlobalConstants.SecurableEntityName.BUSINESSAREA).ToList();
                return _securityService.RequestPermission(request);
            }

            return string.Empty;
        }

        public List<KeyValuePair<string, string>> GetApproversByBusinessArea(int businessAreaId)
        {
            BusinessArea ba = _domainContext.BusinessAreas.FirstOrDefault(x => x.Id == businessAreaId);
            IApplicationUser owner = _userService.GetByAssociateId(ba.PrimaryOwnerId);
            IApplicationUser contact = _userService.GetByAssociateId(ba.PrimaryContactId);

            var owners = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(ba.PrimaryOwnerId, owner.DisplayName + " (Owner)"),
                new KeyValuePair<string, string>(ba.PrimaryContactId, contact.DisplayName + " (Contact)")
            };

            return owners;
        }


        //public List<NotificationModel> ToModels(this List<Notification> cores)
        //{
        //    IApplicationUser user = _userService.GetCurrentUser();
        //    List<NotificationModel> models = new List<NotificationModel>();
        //    foreach (var notification in cores)
        //    {
        //        NotificationModel model = notification.ToModel();

        //        switch (model.NotificationType)
        //        {
        //            case GlobalConstants.Notifications.DATAASSET_TYPE:
        //                DataAsset da = _domainContext.GetById<DataAsset>(notification.ParentObject);
        //                model.ObjectName = da.DisplayName;
        //                UserSecurity us = _securityService.GetUserSecurity(da, user);
        //                model.CanEdit = us.CanModifyNotifications;
        //                break;
        //            case GlobalConstants.Notifications.BUSINESSAREA_TYPE:
        //                BusinessArea ba = _domainContext.GetById<BusinessArea>(notification.ParentObject);
        //                model.ObjectName = ba.Name;
        //                //UserSecurity us = _securityService.GetUserSecurity(notification.ParentObject, user);
        //                //model.CanEdit = us.CanModifyNotifications;
        //                model.CanEdit = true;
        //                break;
        //            default:
        //                break;
        //        }
        //        models.Add(model);
        //    }
        //    return models;
        //}

        public List<BusinessAreaSubscription> GetAllUserSubscriptionsFromDatabase(EventTypeGroup group)
        {
            List<BusinessAreaSubscription> subs = _domainContext.GetAllUserSubscriptionsByEventTypeGroup(_userService.GetCurrentUser().AssociateId, group);
            return subs;
        }

        public IEnumerable<EventType> GetEventTypes(EventTypeGroup group)
        {
            IQueryable<EventType> eventTypes = _domainContext.EventTypes.Where( w => w.Display && w.Group == group.GetDescription());
            foreach (EventType et in eventTypes)
            {
                IQueryable<EventType> children = _domainContext.EventTypes.Where(w => w.ParentDescription == et.Description);
                if(children != null)
                {
                    et.ChildEventTypes = children.ToList();
                }
            }

            return eventTypes;
        }

        public List<Interval> GetAllIntervals()
        {
            List<Interval> i = _domainContext.GetAllIntervals();
            return i;
        }

        public Interval GetInterval(string description)
        {
            Interval i = _domainContext.GetInterval(description);
            return i;
        }

        public void CreateUpdateSubscription(SubscriptionDto dto)
        {
            if(dto.group == EventTypeGroup.BusinessArea || dto.group == EventTypeGroup.BusinessAreaDSC)
            {
                List<BusinessAreaSubscription> dbList = GetAllUserSubscriptionsFromDatabase(dto.group);
                List<BusinessAreaSubscription> newList = dto.CurrentSubscriptionsBusinessArea;

                UpdateSubscriptionsSmartly(ref dbList, newList);

                List<BusinessAreaSubscription> delSubs = dbList.Where(w => w.Interval.Interval_ID == 5).ToList();
                if (delSubs != null)
                {
                    foreach (BusinessAreaSubscription delSub in delSubs)
                    {
                        _domainContext.Remove<BusinessAreaSubscription>(delSub);
                    }
                }

                _domainContext.SaveChanges();
            }
        }

        //RECURSIVE METHOD THAT WILL TAKE NEW LIST OF SUBSCRIPTIONS AND UPDATE IN DATABASE
        private void UpdateSubscriptionsSmartly(ref List<BusinessAreaSubscription> dbList, List<BusinessAreaSubscription> newList)
        {
            foreach (BusinessAreaSubscription newItem in newList)
            {
                bool insertMe = true;
                if (dbList != null)
                {
                    //CHECK IF DB HAS NEW ITEM
                    BusinessAreaSubscription dbItem = (BusinessAreaSubscription)dbList.FirstOrDefault(w => w.ID == newItem.ID);
                    if (dbItem != null)
                    {
                        //UPDATE ONLY IF INTERVAL HAS CHANGED
                        if (dbItem.Interval.Interval_ID != newItem.Interval.Interval_ID)
                        {
                            dbItem.Interval = newItem.Interval;
                        }
                        insertMe = false;
                    }
                }
                if (insertMe)
                {
                    _domainContext.Merge<BusinessAreaSubscription>
                    (
                        new BusinessAreaSubscription
                        (
                            newItem.BusinessAreaType,
                            newItem.EventType,
                            newItem.Interval,
                            _userService.GetCurrentUser().AssociateId
                         )
                    );
                }

                //IF CHILDREN EXIST ON SUBSCRIPTION THEN RESURSIVELY CALL THIS FUNCTION AGAIN TO ADD NEW SUBSCRIPTIONS IF REQUIRED
                if (newItem.Children != null)
                {
                    UpdateSubscriptionsSmartly(ref dbList, newItem.Children);
                }
            }
        }

        public EventType FindEventTypeParent(EventType child)
        {
            string parentDescription = null;

            if(child.Group == EventTypeGroup.BusinessAreaDSC.GetDescription())
            {
                parentDescription = child.Description;
            }
            else
            {
                switch (child.Description)
                {
                    case GlobalConstants.EventType.NOTIFICATION_CRITICAL_ADD:
                    case GlobalConstants.EventType.NOTIFICATION_CRITICAL_UPDATE:
                        parentDescription = GlobalConstants.EventType.NOTIFICATION_CRITICAL;
                        break;
                    case GlobalConstants.EventType.NOTIFICATION_WARNING_ADD:
                    case GlobalConstants.EventType.NOTIFICATION_WARNING_UPDATE:
                        parentDescription = GlobalConstants.EventType.NOTIFICATION_WARNING;
                        break;
                    case GlobalConstants.EventType.NOTIFICATION_INFO_ADD:
                    case GlobalConstants.EventType.NOTIFICATION_INFO_UPDATE:
                        parentDescription = GlobalConstants.EventType.NOTIFICATION_INFO;
                        break;
                    default:
                        break;
                }
            }

            EventType parentEventType = (parentDescription != null)? _domainContext.EventTypes.FirstOrDefault(w => w.Description == parentDescription) : new EventType();      //NO PARENT

            return parentEventType;
        }
    }
}
