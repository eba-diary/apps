using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class DataInventoryUnauthorizedAccessException : UnauthorizedAccessException
    {
        public DataInventoryUnauthorizedAccessException() { }
        public DataInventoryUnauthorizedAccessException(string message) : base(message) { }
        public DataInventoryUnauthorizedAccessException(string message, Exception inner) : base(message, inner) { }
        protected DataInventoryUnauthorizedAccessException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
