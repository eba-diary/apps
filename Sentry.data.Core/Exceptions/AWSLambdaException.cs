using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Exceptions
{
    public class AWSLambdaException : Exception, ISerializable
    {
        public AWSLambdaException() { }
        public AWSLambdaException(string message) : base(message) { }
        public AWSLambdaException(string message, Exception exception) : base(message, exception) { }
        protected AWSLambdaException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
