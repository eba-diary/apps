using System;
using System.Runtime.Serialization;

namespace Sentry.data.Infrastructure
{
    [Serializable]
    public class AcceptableErrorException : Exception
    {
        public AcceptableErrorException() { }
        public AcceptableErrorException(string message) : base(message) { }
        public AcceptableErrorException(string message, Exception exception) : base(message, exception) { }
        protected AcceptableErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
