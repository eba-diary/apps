using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class DataFlowStepNotImplementedException : Exception, ISerializable
    {
        public DataFlowStepNotImplementedException() { }
        public DataFlowStepNotImplementedException(string message) : base(message) { }
        public DataFlowStepNotImplementedException(string message, Exception exception) : base(message, exception) { }
        protected DataFlowStepNotImplementedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
