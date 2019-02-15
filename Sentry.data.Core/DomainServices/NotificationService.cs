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
                model = _domainContext.Notification.Fetch(x => x.ParentDataAsset).FirstOrDefault(x => x.NotificationId == notificationId).ToModel();
            }
            return model;
        }

        public NotificationModel GetNotificationModelForModify(int notificationId)
        {
            NotificationModel model = GetNotificationModelForDisplay(notificationId);
            model.AllDataAssets = GetAssetsForUserSecurity();
            return model;
        }

        public void SubmitNotification(NotificationModel model)
        {
            AssetNotifications an = null;
            if (model.NotificationId == 0)
            {
                an = model.ToCore();
                an.CreateUser = _userService.GetCurrentUser().AssociateId;
                an.ParentDataAsset = _domainContext.DataAsset.FirstOrDefault(x => x.Id == model.DataAssetId);
                _domainContext.Add(an);
            }
            else
            {
                an = _domainContext.Notification.FirstOrDefault(x => x.NotificationId == model.NotificationId);
                an.ExpirationTime = model.ExpirationTime;
                an.StartTime = model.StartTime;
                an.MessageSeverity = model.MessageSeverity;
                an.Message = model.Message;
                an.ParentDataAsset = _domainContext.DataAsset.FirstOrDefault(x => x.Id == model.DataAssetId);
            }

            _domainContext.SaveChanges();
        }

        public List<NotificationModel> GetNotificationsForDataAsset()
        {
            List<NotificationModel> da = _domainContext.Notification.Fetch(x=> x.ParentDataAsset).ToModels();

            foreach(var asset in da)
            {
                IApplicationUser user = _userService.GetByAssociateId(asset.CreateUser);
                try
                {
                    asset.CreateUser = user.DisplayName;
                }catch(Exception ex)
                {
                    Common.Logging.Logger.Error($"Could not get user by Id: {asset.CreateUser}", ex);
                }

            }
            return da;
        }


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

        public List<Permission> GetPermissionsForAccessRequest()
        {
            return _domainContext.Permission.Where(x => x.SecurableObject == GlobalConstants.SecurableEntityName.DATA_ASSET).ToList();
        }
    }
}
