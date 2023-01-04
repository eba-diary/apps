using System;
using System.Runtime.Serialization;

namespace Sentry.data.Infrastructure
{
    [Serializable]
    public class HttpsJobProviderException : Exception
    {
        public HttpsJobProviderException() { }
        public HttpsJobProviderException(string message) : base(message) { }
        public HttpsJobProviderException(string message, Exception exception) : base(message, exception) { }
        protected HttpsJobProviderException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
