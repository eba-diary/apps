using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class SchemaRevisionNotFoundException : Exception, ISerializable
    {
        public SchemaRevisionNotFoundException() { }
        public SchemaRevisionNotFoundException(string message) : base(message) { }
        public SchemaRevisionNotFoundException(string message, Exception exception) : base(message, exception) { }
        protected SchemaRevisionNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
