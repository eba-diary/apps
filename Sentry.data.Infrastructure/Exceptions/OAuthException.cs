using System;
using System.Runtime.Serialization;

namespace Sentry.data.Infrastructure
{
    [Serializable]
    public class OAuthException : Exception
    {
        public OAuthException() { }
        public OAuthException(string message) : base(message) { }
        public OAuthException(string message, Exception exception) : base(message, exception) { }
        protected OAuthException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
