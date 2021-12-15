using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class AWSLambdaException : Exception
    {
        public AWSLambdaException() { }
        public AWSLambdaException(string message) : base(message) { }
        public AWSLambdaException(string message, Exception exception) : base(message, exception) { }
        protected AWSLambdaException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
