using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class JobNotFoundException : Exception
    {
        public JobNotFoundException() { }
        public JobNotFoundException(string message) : base(message) { }
        public JobNotFoundException(string message, Exception exception) : base(message, exception) { }
        protected JobNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
