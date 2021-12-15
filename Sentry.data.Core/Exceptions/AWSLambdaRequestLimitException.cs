using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class AWSLambdaRequestLimitException : Exception
    {
        public AWSLambdaRequestLimitException() { }
        public AWSLambdaRequestLimitException(string message) : base(message) { }
        public AWSLambdaRequestLimitException(string message, Exception exception) : base(message, exception) { }
        protected AWSLambdaRequestLimitException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
