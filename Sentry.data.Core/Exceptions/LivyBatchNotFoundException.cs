using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class LivyBatchNotFoundException : Exception
    {
        public LivyBatchNotFoundException() { }
        public LivyBatchNotFoundException(string message) : base(message) { }
        public LivyBatchNotFoundException(string message, Exception exception) : base(message, exception) { }
        protected LivyBatchNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
