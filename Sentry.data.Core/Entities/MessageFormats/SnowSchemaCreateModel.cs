using Sentry.Messaging.Common;

namespace Sentry.data.Core
{
    public class SnowSchemaCreateModel : BaseEventMessage
    {
        public SnowSchemaCreateModel()
        {
            EventType = "SNOW-SCHEMA-CREATE-REQUESTED";
        }

        public string SnowStatus { get; set; }
        public string InitiatorID { get; set; }
        public string ChangeIND { get; set; }

        public void UpdateStatus(ConsumptionLayerTableStatusEnum status)
        {
            SnowStatus = status.ToString();
        }
    }
}
