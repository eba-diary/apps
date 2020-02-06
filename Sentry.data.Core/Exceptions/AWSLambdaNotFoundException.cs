using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Exceptions
{
    public class AWSLambdaNotFoundException : Exception, ISerializable
    {
        public AWSLambdaNotFoundException() { }
        public AWSLambdaNotFoundException(string message) : base(message) { }
        public AWSLambdaNotFoundException(string message, Exception exception) : base(message, exception) { }
        protected AWSLambdaNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
