using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IEventService
    {
        void PublishSuccessEvent(string eventType, string userId, string reason, string lineCde = null, string search = null);
        void PublishSuccessEventByConfigId(string eventType, string userId, string reason, int configId);
        void PublishSuccessEventByDatasetId(string eventType, string userId, string reason, int datasetId);
        void PublishSuccessEventByDataAsset(string eventType, string userId, string reason, int dataAssetId, string lineCde = null, string search = null);
    }
}