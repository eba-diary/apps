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

        public NotificationService(IDatasetContext domainContext, ISecurityService securityService, UserService userService, IEventService eventService)
        {
            _domainContext = domainContext;
            _securityService = securityService;
            _userService = userService;
            _eventService = eventService;
        }


        public bool CanUserModifyNotifications()
        {
            List<DataAsset> dataAssets = _domainContext.DataAsset.ToList();
            IApplicationUser user = _userService.GetCurrentUser();

            foreach (var asset in dataAssets)
            {
                if (_securityService.GetUserSecurity(asset, user).CanModifyNotifications)
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
                    ExpirationTime = DateTime.Now.AddHours(1)
                };
            }
            else
            {
                //int objectId = int.Parse(notificationId.Split('_')[1]);
                model = _domainContext.Notification.FirstOrDefault(x => x.NotificationId == notificationId).ToModel();
            }
            return model;
        }

        public NotificationDto GetNotificationModelForModify(int notificationId)
        {
            NotificationDto model = GetNotificationModelForDisplay(notificationId);
            model.AllDataAssets = GetAssetsForUserSecurity();
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

            _domainContext.SaveChanges();

            //the way that other components work when publishing events is to pass a constant which is essentially 
            //equal to the EventType.Description, this is sort of strange because the actual event service then grabs
            //the appropriate EventType based on the description here, because i need to move on I will use this design
            //pattern because it works, so don't judge me.
            string eventTypeDescription = null;

            switch (dto.MessageSeverity)
            {
                case NotificationSeverity.Critical:
                    eventTypeDescription = addNotification == true ? GlobalConstants.EventType.NOTIFICATION_CRITICAL_ADD : GlobalConstants.EventType.NOTIFICATION_CRITICAL_UPDATE;
                    break;
                case NotificationSeverity.Warning:
                    eventTypeDescription = addNotification == true ? GlobalConstants.EventType.NOTIFICATION_WARNING_ADD : GlobalConstants.EventType.NOTIFICATION_WARNING_UPDATE;
                    break;
                case NotificationSeverity.Info:
                    eventTypeDescription = addNotification == true ? GlobalConstants.EventType.NOTIFICATION_INFO_ADD : GlobalConstants.EventType.NOTIFICATION_INFO_UPDATE;
                    break;
                default:
                    Logger.Error("Notification Severity Not Found to log EventType of " + dto.MessageSeverity.ToString() + " for NotificationId = " + dto.NotificationId.ToString());
                    break;
            }

            if(eventTypeDescription != null)
                _eventService.PublishSuccessEventByNotificationId(eventTypeDescription, _userService.GetCurrentUser().AssociateId, eventTypeDescription, dto.NotificationId);

            return dto.NotificationId;
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
        public List<DataAsset> GetAssetsForAccessRequest()
        {
            return _domainContext.DataAsset.ToList();
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

            return baList;
        }

        public List<Permission> GetPermissionsForAccessRequest()
        {
            return _domainContext.Permission.Where(x => x.SecurableObject == GlobalConstants.SecurableEntityName.DATA_ASSET).ToList();
        }

        public string RequestAccess(AccessRequest request)
        {
            DataAsset da = _domainContext.DataAsset.FirstOrDefault(x=> x.Id == request.SecurableObjectId);

            if (da != null)
            {
                IApplicationUser user = _userService.GetCurrentUser();

                request.PermissionForUserId = user.AssociateId;
                request.PermissionForUserName = user.DisplayName;
                request.SecurableObjectName = da.DisplayName;
                request.SecurityId = da.Security.SecurityId;
                request.RequestorsId = user.AssociateId;
                request.RequestorsName = user.DisplayName;
                request.IsProd = bool.Parse(Configuration.Config.GetHostSetting("RequireApprovalHPSMTickets"));
                request.RequestedDate = DateTime.Now;
                request.ApproverId = request.SelectedApprover;
                request.Permissions = _domainContext.Permission.Where(x => request.SelectedPermissionCodes.Contains(x.PermissionCode) &&
                                                                                                                x.SecurableObject == GlobalConstants.SecurableEntityName.DATA_ASSET).ToList();
                return _securityService.RequestPermission(request);
            }

            return string.Empty;
        }

        public List<KeyValuePair<string, string>> GetApproversByDataAsset(int dataAssetId)
        {
            DataAsset dataAsset = _domainContext.DataAsset.FirstOrDefault(x => x.Id == dataAssetId);

            IApplicationUser owner = _userService.GetByAssociateId(dataAsset.PrimaryOwnerId);
            IApplicationUser contact = _userService.GetByAssociateId(dataAsset.PrimaryContactId);

            var owners = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(dataAsset.PrimaryOwnerId, owner.DisplayName + " (Owner)"),
                new KeyValuePair<string, string>(dataAsset.PrimaryContactId, contact.DisplayName + " (Contact)")
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

        public List<BusinessAreaSubscription> GetAllUserSubscriptions(EventTypeGroup group)
        {
            return _domainContext.GetAllUserSubscriptionsByEventTypeGroup(_userService.GetCurrentUser().AssociateId, group);
        }

        public IEnumerable<EventType> GetEventTypes(EventTypeGroup group)
        {
            IQueryable<EventType> et = _domainContext.EventTypes.Where( w => w.Display && w.Group == group.GetDescription());
            return et;
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
            
            if(dto.group == EventTypeGroup.BusinessArea)
            {
                List<BusinessAreaSubscription> oldSubs = GetAllUserSubscriptions(dto.group);

                foreach (BusinessAreaSubscription newSub in dto.CurrentSubscriptionsBusinessArea)
                {
                    bool insertMe = true;

                    if (oldSubs != null)
                    {
                        BusinessAreaSubscription oldSub = (BusinessAreaSubscription)oldSubs.FirstOrDefault(w => w.ID == newSub.ID);
                        if (oldSub != null)
                        {
                            if (oldSub.Interval.Interval_ID != newSub.Interval.Interval_ID)
                                oldSub.Interval = newSub.Interval;

                            insertMe = false;
                        }
                    }

                    if (insertMe)
                    {
                        _domainContext.Merge<BusinessAreaSubscription>
                        (
                            new BusinessAreaSubscription
                            (
                                newSub.BusinessAreaType,
                                newSub.EventType,
                                newSub.Interval,
                                _userService.GetCurrentUser().AssociateId
                             )
                        );
                    }
                }
                
                List<BusinessAreaSubscription> delSubs = oldSubs.Where(w => w.Interval.Interval_ID == 5).ToList();
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
    }
}
