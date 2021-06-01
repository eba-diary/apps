using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class RetrieverJobProcessingException : Exception
    {
        public RetrieverJobProcessingException() { }
        public RetrieverJobProcessingException(string message) : base(message) { }
        public RetrieverJobProcessingException(string message, Exception exception) : base(message, exception) { }
        protected RetrieverJobProcessingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
