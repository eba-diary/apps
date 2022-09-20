using System;
using System.Runtime.Serialization;

namespace Sentry.data.Infrastructure
{
    [Serializable]
    public class GoogleBigQueryNotFoundException : Exception
    {
        public GoogleBigQueryNotFoundException() { }
        public GoogleBigQueryNotFoundException(string message) : base(message) { }
        public GoogleBigQueryNotFoundException(string message, Exception exception) : base(message, exception) { }
        protected GoogleBigQueryNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
