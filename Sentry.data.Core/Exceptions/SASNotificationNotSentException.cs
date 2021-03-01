using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class SASNotificationNotSentException : Exception
    {
        public SASNotificationNotSentException() { }
        public SASNotificationNotSentException(string message) : base(message) { }
        public SASNotificationNotSentException(string message, Exception exception) : base(message, exception) { }
        protected SASNotificationNotSentException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
