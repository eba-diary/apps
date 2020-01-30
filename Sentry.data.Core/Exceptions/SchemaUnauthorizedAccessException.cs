using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    public class SchemaUnauthorizedAccessException : UnauthorizedAccessException, ISerializable
    {
        public SchemaUnauthorizedAccessException() { }
        public SchemaUnauthorizedAccessException(string message) : base(message) { }
        public SchemaUnauthorizedAccessException(string message, Exception exception) : base(message, exception) { }
        protected SchemaUnauthorizedAccessException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
