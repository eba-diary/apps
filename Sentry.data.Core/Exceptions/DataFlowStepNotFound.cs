using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
