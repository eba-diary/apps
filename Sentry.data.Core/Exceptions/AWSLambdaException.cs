using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class AwsLambdaException : Exception
    {
        public AwsLambdaException() { }
        public AwsLambdaException(string message) : base(message) { }
        public AwsLambdaException(string message, Exception exception) : base(message, exception) { }
        protected AwsLambdaException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
