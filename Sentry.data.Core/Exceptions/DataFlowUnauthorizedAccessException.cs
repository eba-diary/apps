using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class DataFlowUnauthorizedAccessException : UnauthorizedAccessException
    {
        public DataFlowUnauthorizedAccessException() { }
        public DataFlowUnauthorizedAccessException(string message) : base(message) { }
        public DataFlowUnauthorizedAccessException(string message, Exception inner) : base(message, inner) { }
        protected DataFlowUnauthorizedAccessException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
