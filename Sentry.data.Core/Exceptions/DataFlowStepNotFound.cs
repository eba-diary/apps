using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class DataFlowStepNotFound : Exception
    {
        public DataFlowStepNotFound() { }
        public DataFlowStepNotFound(string message) : base(message) { }
        public DataFlowStepNotFound(string message, Exception exception) : base(message, exception) { }
        protected DataFlowStepNotFound(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
