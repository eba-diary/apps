
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface INotificationService
    {
        bool CanUserModifyNotifications();
        NotificationModel GetNotificationModelForModify(int notificationId);
        NotificationModel GetNotificationModelForDisplay(int notificationId);
        void SubmitNotification(NotificationModel model);
        List<NotificationModel> GetNotificationsForDataAsset();
        List<DataAsset> GetAssetsForUserSecurity();
        List<Permission> GetPermissionsForAccessRequest();
    }
}
