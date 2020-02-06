using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class AWSLambdaInvalidParameterException : ArgumentException, ISerializable
    {
        public AWSLambdaInvalidParameterException() { }
        public AWSLambdaInvalidParameterException(string message) : base(message) { }
        public AWSLambdaInvalidParameterException(string message, Exception exception) : base(message, exception) { }
        protected AWSLambdaInvalidParameterException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
