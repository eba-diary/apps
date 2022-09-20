using System;
using System.Runtime.Serialization;

namespace Sentry.data.Infrastructure
{
    [Serializable]
    public class GoogleBigQueryJobProviderException : Exception
    {
        public GoogleBigQueryJobProviderException() { }
        public GoogleBigQueryJobProviderException(string message) : base(message) { }
        public GoogleBigQueryJobProviderException(string message, Exception exception) : base(message, exception) { }
        protected GoogleBigQueryJobProviderException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
