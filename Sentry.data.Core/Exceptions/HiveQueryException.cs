using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class HiveQueryException : Exception, ISerializable
    {
        public HiveQueryException() { }
        public HiveQueryException(string message) : base(message) { }
        public HiveQueryException(string message, Exception exception) : base(message, exception) { }
        protected HiveQueryException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}