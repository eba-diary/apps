using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class DaleInvalidSearchException : Exception
    {
        public DaleInvalidSearchException() { }
        public DaleInvalidSearchException(string message) : base(message) { }
        public DaleInvalidSearchException(string message, Exception inner) : base(message, inner) { }
        protected DaleInvalidSearchException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
