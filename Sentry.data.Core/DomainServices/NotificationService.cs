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

        public NotificationModel GetModifyNotificationModel()
        {
            IApplicationUser user = _userService.GetCurrentUser();
            NotificationModel model = new NotificationModel()
            {
                AllDataAssets = GetAssetsForUserSecurity(user),
                AllSeverities = Enum.GetValues(typeof(NotificationSeverity)).Cast<NotificationSeverity>().ToList(),
                CreateUser =user.AssociateId,
                StartTime = DateTime.Now,
                ExpirationTime = DateTime.Now.AddHours(1)
            };

            return model;
        }

        public void SubmitNotification(NotificationModel model)
        {
            AssetNotifications an = null;
            if (model.NotificationId == 0)
            {
                an = model.ToCore();
                _domainContext.Add(an);
            }
            else
            {
                an = _domainContext.Notification.FirstOrDefault(x => x.NotificationId == model.NotificationId);
                an.ExpirationTime = model.ExpirationTime;
                an.MessageSeverity = model.MessageSeverity;
                an.Message = model.Message;
            }

            _domainContext.SaveChanges();
        }

        public List<NotificationModel> GetNotificationsForDataAsset(int dataAssetId = 0)
        {
            List<NotificationModel> da;
            if (dataAssetId != 0)
            {
                da = _domainContext.Notification.Where(x => x.ParentDataAsset.Id == dataAssetId).Fetch(x => x.ParentDataAsset).ToModels();
            }
            else
            {
                da = _domainContext.Notification.ToModels();
            }
            return da;
        }


        private List<DataAsset> GetAssetsForUserSecurity(IApplicationUser user)
        {
            List<DataAsset> assetsWithPermission = new List<DataAsset>();
            List<DataAsset> dataAssets = _domainContext.DataAsset.FetchSecurityTree(_domainContext);
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
    }
}
