using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class AwsLambdaServiceException : Exception
    {
        public AwsLambdaServiceException() { }
        public AwsLambdaServiceException(string message) : base(message) { }
        public AwsLambdaServiceException(string message, Exception exception) : base(message, exception) { }
        protected AwsLambdaServiceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
