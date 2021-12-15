using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class AWSLambdaNotFoundException : Exception
    {
        public AWSLambdaNotFoundException() { }
        public AWSLambdaNotFoundException(string message) : base(message) { }
        public AWSLambdaNotFoundException(string message, Exception exception) : base(message, exception) { }
        protected AWSLambdaNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
