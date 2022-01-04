using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class AwsLambdaNotFoundException : Exception
    {
        public AwsLambdaNotFoundException() { }
        public AwsLambdaNotFoundException(string message) : base(message) { }
        public AwsLambdaNotFoundException(string message, Exception exception) : base(message, exception) { }
        protected AwsLambdaNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
