
using System.Collections.Generic;
using System.Threading.Tasks;

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
        List<BusinessArea> GetBusinessAreasForAccessRequest();
        List<Permission> GetPermissionsForAccessRequest();
        Task<string> RequestAccess(AccessRequest request);
        List<KeyValuePair<string, string>> GetApproversByBusinessArea(int businessAreaId);

        List<BusinessAreaSubscription> GetAllUserSubscriptionsFromDatabase(EventTypeGroup group);
        IEnumerable<EventType> GetEventTypes(EventTypeGroup group);
        List<Interval> GetAllIntervals();
        Interval GetInterval(string description);
        void CreateUpdateSubscription(SubscriptionDto dto);

        EventType FindEventTypeParent(EventType child);

        void AutoExpire(int notificationId);
    }
}
