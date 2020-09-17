using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class DaleQueryException : Exception
    {
        public DaleQueryException() { }
        public DaleQueryException(string message) : base(message) { }
        public DaleQueryException(string message, Exception inner) : base(message, inner) { }
        protected DaleQueryException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
