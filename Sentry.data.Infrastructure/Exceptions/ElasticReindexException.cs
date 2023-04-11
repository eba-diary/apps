using System;
using System.Runtime.Serialization;

namespace Sentry.data.Infrastructure
{
    [Serializable]
    public class ElasticReindexException : Exception
    {
        public ElasticReindexException() { }
        public ElasticReindexException(string message) : base(message) { }
        public ElasticReindexException(string message, Exception exception) : base(message, exception) { }
        protected ElasticReindexException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
