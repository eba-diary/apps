using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Exceptions
{
    public class AWSLambdaServiceException : Exception, ISerializable
    {
        public AWSLambdaServiceException() { }
        public AWSLambdaServiceException(string message) : base(message) { }
        public AWSLambdaServiceException(string message, Exception exception) : base(message, exception) { }
        protected AWSLambdaServiceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
