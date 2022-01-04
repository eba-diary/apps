using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class AwsLambdaRequestLimitException : Exception
    {
        public AwsLambdaRequestLimitException() { }
        public AwsLambdaRequestLimitException(string message) : base(message) { }
        public AwsLambdaRequestLimitException(string message, Exception exception) : base(message, exception) { }
        protected AwsLambdaRequestLimitException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
