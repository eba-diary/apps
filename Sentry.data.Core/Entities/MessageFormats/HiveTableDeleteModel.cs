using Sentry.Messaging.Common;

namespace Sentry.data.Core
{
    public class HiveTableDeleteModel : BaseEventMessage
    {
        public HiveTableDeleteModel()
        {
            EventType = "HIVE-TABLE-DELETE-REQUESTED";
        }
        public int SchemaID{ get; set; }
        public string HiveStatus { get; set; }
        public string InitiatorID { get; set; }
    }
}
