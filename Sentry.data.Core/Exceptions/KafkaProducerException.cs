﻿using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class KafkaProducerException : Exception
    {
        public KafkaProducerException() { }
        public KafkaProducerException(string message) : base(message) { }
        public KafkaProducerException(string message, Exception exception) : base(message, exception) { }
        protected KafkaProducerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}