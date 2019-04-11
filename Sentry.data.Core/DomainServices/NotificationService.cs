using System;
using System.Collections.Generic;
using System.Linq;
using Sentry.Core;

namespace Sentry.data.Core
{
    public class NotificationService : INotificationService
    {
        private readonly IDatasetContext _domainContext;
        private readonly ISecurityService _securityService;
        private readonly UserService _userService;

        public NotificationService(IDatasetContext domainContext, ISecurityService securityService, UserService userService)
        {
            _domainContext = domainContext;
            _securityService = securityService;
            _userService = userService;
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

        public NotificationModel GetNotificationModelForDisplay(int notificationId)
        {
            IApplicationUser user = _userService.GetCurrentUser();
            NotificationModel model;

            if (notificationId == 0)
            {
                model = new NotificationModel()
                {
                    CreateUser = user.AssociateId,
                    StartTime = DateTime.Now,
                    ExpirationTime = DateTime.Now.AddHours(1)
                };
            }
            else
            {
                model = _domainContext.Notification.Fetch(x => x.ParentObject).FirstOrDefault(x => x.NotificationId == notificationId).ToModel();
            }
            return model;
        }

        public NotificationModel GetNotificationModelForModify(int notificationId)
        {
            NotificationModel model = GetNotificationModelForDisplay(notificationId);
            model.AllDataAssets = GetAssetsForUserSecurity();
            model.AllBusinessAreas = GetBusinessAreasForUserSecurity();
            return model;
        }

        public void SubmitNotification(NotificationModel model)
        {
            Notification an = null;

            if (model.NotificationId == 0)
            {
                an = model.ToCore();
                an.CreateUser = _userService.GetCurrentUser().AssociateId;
                _domainContext.Add(an);
            }
            else
            {
                an = _domainContext.Notification.FirstOrDefault(x => x.NotificationId == model.NotificationId);
                an.ExpirationTime = model.ExpirationTime;
                an.StartTime = model.StartTime;
                an.MessageSeverity = model.MessageSeverity;
                an.Message = model.Message;
                an.ParentObject = model.ObjectId;
            }

            _domainContext.SaveChanges();
        }

        public List<NotificationModel> GetNotificationsForDataAsset()
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

        public List<NotificationModel> GetNotificationForBusinessArea();

        public List<NotificationModel> GetAllNotifications()
        {
            List<NotificationModel> models = new List<NotificationModel>();
            List<Notification> notifications = _domainContext.Notification.ToList();
            IApplicationUser user = _userService.GetCurrentUser();

            foreach (var notification in notifications)
            {                
                NotificationModel model = notification.ToModel();

                switch (model.NotificationType)
                {
                    case GlobalConstants.Notifications.DATAASSET_TYPE:
                        DataAsset da = _domainContext.GetById<DataAsset>(notification.ParentObject);
                        model.ObjectName = da.DisplayName;
                        UserSecurity us = _securityService.GetUserSecurity(da, user);
                        model.CanEdit = us.CanModifyNotifications;
                        break;
                    case GlobalConstants.Notifications.BUSINESSAREA_TYPE:
                        BusinessArea ba = _domainContext.GetById<BusinessArea>(notification.ParentObject);
                        model.ObjectName = ba.Name;
                        //UserSecurity us = _securityService.GetUserSecurity(notification.ParentObject, user);
                        //model.CanEdit = us.CanModifyNotifications;
                        model.CanEdit = true;
                        break;
                    default:
                        break;
                }
                models.Add(model);
            }

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
    }
}
