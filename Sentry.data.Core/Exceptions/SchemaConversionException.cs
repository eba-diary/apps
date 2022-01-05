using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class SchemaConversionException : Exception
    {
        public SchemaConversionException() { }
        public SchemaConversionException(string message) : base(message) { }
        public SchemaConversionException(string message, Exception exception) : base(message, exception) { }
        protected SchemaConversionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
