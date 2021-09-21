using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    /// <summary>
    /// Thrown by the DataFlowService when attempting to create a new DataFlow, but the schema chosen is already
    /// tied to a different DataFlow
    /// </summary>
    [Serializable]
    public class SchemaInUseException : Exception
    {
        public SchemaInUseException() { }
        public SchemaInUseException(string message) : base(message) { }
        public SchemaInUseException(string message, Exception exception) : base(message, exception) { }
        protected SchemaInUseException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
