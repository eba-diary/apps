using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class DataInventoryInvalidSearchException : Exception
    {
        public DataInventoryInvalidSearchException() { }
        public DataInventoryInvalidSearchException(string message) : base(message) { }
        public DataInventoryInvalidSearchException(string message, Exception inner) : base(message, inner) { }
        protected DataInventoryInvalidSearchException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
