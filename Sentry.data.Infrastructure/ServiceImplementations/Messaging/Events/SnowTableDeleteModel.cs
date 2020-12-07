using Sentry.Messaging.Common;

namespace Sentry.data.Infrastructure
{
    public class SnowTableDeleteModel : BaseEventMessage
    {
        public SnowTableDeleteModel()
        {
            EventType = "SNOW-TABLE-DELETE-REQUESTED";
        }
        public int SchemaID { get; set; }
        public string SnowStatus { get; set; }
    }
}
