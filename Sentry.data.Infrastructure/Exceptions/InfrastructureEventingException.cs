using System;

namespace Sentry.data.Infrastructure.Exceptions
{
    /// <summary>
    /// This Exception is thrown when there are errors communicating with the Infrastructure Eventing API
    /// </summary>
    [Serializable]
    public class InfrastructureEventingException : Exception
    {
        public InfrastructureEventingException() { }
        public InfrastructureEventingException(string message) : base(message) { }
        public InfrastructureEventingException(string message, Exception inner) : base(message, inner) { }
        protected InfrastructureEventingException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
