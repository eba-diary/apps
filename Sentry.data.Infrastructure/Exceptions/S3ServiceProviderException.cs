using System;
using System.Runtime.Serialization;

namespace Sentry.data.Infrastructure.Exceptions
{
    [Serializable]
    class S3ServiceProviderException : Exception, ISerializable
    {
        public S3ServiceProviderException() { }
        public S3ServiceProviderException(string message) : base(message) { }
        public S3ServiceProviderException(string message, Exception exception) : base(message, exception) { }
        protected S3ServiceProviderException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
