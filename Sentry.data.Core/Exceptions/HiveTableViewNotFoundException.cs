using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class HiveTableViewNotFoundException : Exception
    {
        public HiveTableViewNotFoundException() { }
        public HiveTableViewNotFoundException(string message) : base(message) { }
        public HiveTableViewNotFoundException(string message, Exception exception) : base(message, exception) { }
        protected HiveTableViewNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
