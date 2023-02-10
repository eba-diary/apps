using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core
{
    [Serializable]
    public class ResourceForbiddenException : Exception
    {
        public ResourceForbiddenException()
        {
        }

        public ResourceForbiddenException(string message) : base(message)
        {
        }

        public ResourceForbiddenException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ResourceForbiddenException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
