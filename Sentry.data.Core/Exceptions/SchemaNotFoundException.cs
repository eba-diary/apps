using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    public class SchemaNotFoundException : Exception, ISerializable
    {
        public SchemaNotFoundException() { }
        public SchemaNotFoundException(string message) : base(message) { }
        public SchemaNotFoundException(string message, Exception exception) : base(message, exception) { }
        protected SchemaNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
