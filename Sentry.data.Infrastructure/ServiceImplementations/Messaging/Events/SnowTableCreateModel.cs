using Sentry.data.Core;
using Sentry.Messaging.Common;

namespace Sentry.data.Infrastructure
{
    public class SnowTableCreateModel : BaseEventMessage
    {
        public SnowTableCreateModel()
        {
            EventType = "SNOW-TABLE-CREATE-REQUESTED";
        }

        public string SnowStatus { get; set; }
        public int SchemaID { get; set; }
        public int RevisionID { get; set; }
        public string InitiatorID { get; set; }
        public void UpdateStatus(ConsumptionLayerTableStatusEnum status)
        {
            SnowStatus = status.ToString();
        }
    }
}
