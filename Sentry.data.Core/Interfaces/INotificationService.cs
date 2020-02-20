
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface INotificationService
    {
        bool CanUserModifyNotifications();
        NotificationDto GetNotificationModelForModify(int notificationId);
        NotificationDto GetNotificationModelForDisplay(int notificationId);
        int SubmitNotification(NotificationDto dto);
        List<NotificationDto> GetNotificationsForDataAsset();
        List<NotificationDto> GetNotificationForBusinessArea(BusinessAreaType type);
        List<NotificationDto> GetAllNotifications();
        List<DataAsset> GetAssetsForUserSecurity();
        List<BusinessArea> GetBusinessAreasForUserSecurity();
        List<DataAsset> GetAssetsForAccessRequest();
        List<Permission> GetPermissionsForAccessRequest();
        string RequestAccess(AccessRequest request);
        List<KeyValuePair<string, string>> GetApproversByDataAsset(int dataAssetId);

        List<BusinessAreaSubscription> GetAllUserSubscriptions(EventTypeGroup group);
        IEnumerable<EventType> GetEventTypes(EventTypeGroup group);
        List<Interval> GetAllIntervals();
        Interval GetInterval(string description);
        void CreateUpdateSubscription(SubscriptionDto dto);

        EventType FindEventTypeParent(EventType child);
    }
}
