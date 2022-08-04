using Nest;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IEventService
    {
        Task PublishSuccessEvent(string eventType, string reason, string search = null);
        Task PublishSuccessEventByConfigId(string eventType, string reason, int configId);
        Task PublishSuccessEventByDatasetId(string eventType, string reason, int datasetId);
        Task PublishSuccessEventByDataAsset(string eventType, string reason, int dataAssetId, string lineCde, string search);
        Task PublishSuccessEventByNotificationId(string eventType, string reason, Notification notification);
        Task PublishSuccessEventBySchemaId(string eventType, string reason, int datasetId, int schemaId);
        Task PublishEventByDatasetFileDelete(string eventType, string reason, int datasetId, int schemaId, string deleteDetail);
        Task PublishEventByDatasetFileDelete(string eventType, string reason,string deleteDetail);

    }
}