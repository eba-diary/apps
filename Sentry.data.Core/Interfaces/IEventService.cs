using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IEventService
    {
        void PublishSuccessEvent(string eventType, string userId, string reason);
        void PublishSuccessEventByConfigId(string eventType, string userId, string reason, int configId);
        void PublishSuccessEventByDatasetId(string eventType, string userId, string reason, int datasetId);
        void PublishSuccessEventByNotificationId(string eventType, string userId, string reason, int notificationId);
    }
}