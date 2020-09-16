using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class DaleQueryException : UnauthorizedAccessException, ISerializable
    {
        public DaleQueryException() { }
        public DaleQueryException(string message) : base(message) { }
        public DaleQueryException(string message, Exception inner) : base(message, inner) { }
        protected DaleQueryException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
