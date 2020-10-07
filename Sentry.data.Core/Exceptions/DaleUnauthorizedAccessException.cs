using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class DaleUnauthorizedAccessException : UnauthorizedAccessException
    {
        public DaleUnauthorizedAccessException() { }
        public DaleUnauthorizedAccessException(string message) : base(message) { }
        public DaleUnauthorizedAccessException(string message, Exception inner) : base(message, inner) { }
        protected DaleUnauthorizedAccessException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
