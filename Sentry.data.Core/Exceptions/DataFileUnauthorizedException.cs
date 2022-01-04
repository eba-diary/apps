using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class DataFileUnauthorizedException : UnauthorizedAccessException
    {
        public DataFileUnauthorizedException() { }
        public DataFileUnauthorizedException(string message) : base(message) { }
        public DataFileUnauthorizedException(string message, Exception inner) : base(message, inner) { }
        protected DataFileUnauthorizedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
