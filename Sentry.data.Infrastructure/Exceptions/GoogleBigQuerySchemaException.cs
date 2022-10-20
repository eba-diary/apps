using System;
using System.Runtime.Serialization;

namespace Sentry.data.Infrastructure
{
    [Serializable]
    public class GoogleBigQuerySchemaException : Exception
    {
        public GoogleBigQuerySchemaException() { }
        public GoogleBigQuerySchemaException(string message) : base(message) { }
        public GoogleBigQuerySchemaException(string message, Exception exception) : base(message, exception) { }
        protected GoogleBigQuerySchemaException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
