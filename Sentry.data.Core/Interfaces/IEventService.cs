using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IEventService
    {
        void PublishSuccessEventByConfigId(string eventType, string userId, string reason, int configId);
        void PublishSuccessEvent(string eventType, string userId, string reason, int datasetId);
    }
}