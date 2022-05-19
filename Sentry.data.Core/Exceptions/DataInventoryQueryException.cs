using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class DataInventoryQueryException : Exception
    {
        public DataInventoryQueryException() { }
        public DataInventoryQueryException(string message) : base(message) { }
        public DataInventoryQueryException(string message, Exception inner) : base(message, inner) { }
        protected DataInventoryQueryException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
