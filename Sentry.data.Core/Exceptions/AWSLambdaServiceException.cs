using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class AWSLambdaServiceException : Exception
    {
        public AWSLambdaServiceException() { }
        public AWSLambdaServiceException(string message) : base(message) { }
        public AWSLambdaServiceException(string message, Exception exception) : base(message, exception) { }
        protected AWSLambdaServiceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
