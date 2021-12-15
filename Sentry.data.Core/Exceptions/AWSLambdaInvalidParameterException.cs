using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class AWSLambdaInvalidParameterException : ArgumentException
    {
        public AWSLambdaInvalidParameterException() { }
        public AWSLambdaInvalidParameterException(string message) : base(message) { }
        public AWSLambdaInvalidParameterException(string message, Exception exception) : base(message, exception) { }
        protected AWSLambdaInvalidParameterException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
