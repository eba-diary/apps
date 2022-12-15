using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class DataApplicationServiceException : Exception
    {
        public DataApplicationServiceException() { }
        public DataApplicationServiceException(string message) : base(message) { }
        public DataApplicationServiceException(string message, Exception inner) : base(message, inner) { }
        protected DataApplicationServiceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
