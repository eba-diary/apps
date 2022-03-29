using Sentry.Messaging.Common;

namespace Sentry.data.Core
{
    public class SparkConverterModel : BaseEventMessage
    {
        public SparkConverterModel()
        {
            EventType = "SPARKCONVERTERSTATUS";
        }

        public string Status { get; set; }
        public int SchemaID { get; set; }
        public int RevisionID { get; set; }
        public string InitiatorID { get; set; }
        public string ChangeIND { get; set; }
        public void UpdateStatus(ConsumptionLayerTableStatusEnum status)
        {
            Status = status.ToString();
        }
    }
}
