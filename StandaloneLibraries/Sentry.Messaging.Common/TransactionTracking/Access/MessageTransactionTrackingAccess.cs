using System;

namespace Sentry.Messaging.Common
{
    public static class MessageTransactionTrackingAccess
    {
        public static Func<string> EnvironmentProvider { get; set; } = () => "";

        private static IMessageTracker _internalMessageTracker;
        public static Func<IMessageTracker> MessageTransactionTrackerProvider { get; set; } = () => new EmptyMessageTracker();
        public static IMessageTracker MessageTransactionTracker
        {
            get
            {
                //if we are scoped per request, then return a new one everytime
                //JWZ - regression test hijack existing environment
                if (ScopePerRequest) return MessageTransactionTrackerProvider.Invoke();

                if (_internalMessageTracker == null)
                {
                    _internalMessageTracker = MessageTransactionTrackerProvider.Invoke();
                }

                return _internalMessageTracker;
            }
        }
        public static bool ScopePerRequest { get; set; } = false;
        public static string MessagingApplication { get; set; } = "";
        public static string SerializationOption { get; set; } = SerializeMessageOptions.SerializeOriginalMessageOnly;
        public static string MessagingIdPrefix { get; set; } = "";
    }
}
