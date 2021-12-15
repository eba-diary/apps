using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class AwsLambdaInvalidParameterException : ArgumentException
    {
        public AwsLambdaInvalidParameterException() { }
        public AwsLambdaInvalidParameterException(string message) : base(message) { }
        public AwsLambdaInvalidParameterException(string message, Exception exception) : base(message, exception) { }
        protected AwsLambdaInvalidParameterException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
