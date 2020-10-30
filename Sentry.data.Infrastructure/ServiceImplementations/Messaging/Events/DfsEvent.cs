using Sentry.Messaging.Common;

namespace Sentry.data.Infrastructure
{
    public class DfsEvent : BaseEventMessage
    {
        public DfsEvent()
        {
            EventType = "DFSEVENT";
        }
        public DfsEventPayload PayLoad { get; set; }
    }
}
