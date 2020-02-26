using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class DataFlowNotFound : Exception
    {
        public DataFlowNotFound() { }
        public DataFlowNotFound(string message) : base(message) { }
        public DataFlowNotFound(string message, Exception exception) : base(message, exception) { }
        protected DataFlowNotFound(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
