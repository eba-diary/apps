﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class AWSLambdaRequestLimitException : Exception, ISerializable
    {
        public AWSLambdaRequestLimitException() { }
        public AWSLambdaRequestLimitException(string message) : base(message) { }
        public AWSLambdaRequestLimitException(string message, Exception exception) : base(message, exception) { }
        protected AWSLambdaRequestLimitException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
