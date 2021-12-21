using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class DatasetNotFoundException : Exception
    {
        public DatasetNotFoundException() { }
        public DatasetNotFoundException(string message) : base(message) { }
        public DatasetNotFoundException(string message, Exception exception) : base(message, exception) { }
        protected DatasetNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
