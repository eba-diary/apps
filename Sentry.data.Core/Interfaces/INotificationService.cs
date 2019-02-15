
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface INotificationService
    {
        bool CanUserModifyNotifications();
        NotificationModel GetModifyNotificationModel();
        void SubmitNotification(NotificationModel model);
        List<NotificationModel> GetNotificationsForDataAsset(int dataAssetId = 0);
    }
}
