
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
        List<NotificationModel> GetNotificationForBusinessArea(BusinessAreaType type);
        List<NotificationModel> GetAllNotifications();
        List<DataAsset> GetAssetsForUserSecurity();
        List<BusinessArea> GetBusinessAreasForUserSecurity();
        List<DataAsset> GetAssetsForAccessRequest();
        List<Permission> GetPermissionsForAccessRequest();
        string RequestAccess(AccessRequest request);
        List<KeyValuePair<string, string>> GetApproversByDataAsset(int dataAssetId);

        List<BusinessAreaSubscription> GetAllUserSubscriptions(Group group);
        IEnumerable<EventType> GetEventTypes(Group group);
        List<Interval> GetAllIntervals();
        Interval GetInterval(string description);
        void CreateUpdateSubscription(SubscriptionDto dto);
    }
}
