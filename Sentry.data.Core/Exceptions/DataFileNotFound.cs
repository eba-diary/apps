using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class DataFileNotFoundException : UnauthorizedAccessException, ISerializable
    {
        public DataFileNotFoundException() { }
        public DataFileNotFoundException(string message) : base(message) { }
        public DataFileNotFoundException(string message, Exception inner) : base(message, inner) { }
        protected DataFileNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
