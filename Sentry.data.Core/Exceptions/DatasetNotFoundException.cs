using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    public class DatasetNotFoundException : Exception, ISerializable
    {
        public DatasetNotFoundException() { }
        public DatasetNotFoundException(string message) : base(message) { }
        public DatasetNotFoundException(string message, Exception exception) : base(message, exception) { }
        protected DatasetNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
