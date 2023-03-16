using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core
{
    [Serializable]
    public abstract class BaseResourceException : Exception
    {
        public string ResourceAction { get; }
        public int ResourceId { get; }

        protected BaseResourceException(string resourceAction, int resourceId)
        {
            ResourceAction = resourceAction;
            ResourceId = resourceId;
        }

        protected BaseResourceException(string message) : base(message)
        {
        }

        protected BaseResourceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BaseResourceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
