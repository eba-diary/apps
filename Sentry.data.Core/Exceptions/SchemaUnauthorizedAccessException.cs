using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class SchemaUnauthorizedAccessException : UnauthorizedAccessException
    {
        public SchemaUnauthorizedAccessException() { }
        public SchemaUnauthorizedAccessException(string message) : base(message) { }
        public SchemaUnauthorizedAccessException(string message, Exception exception) : base(message, exception) { }
        protected SchemaUnauthorizedAccessException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
