using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class FeatureNotEnabledException : Exception
    {
        public FeatureNotEnabledException() { }
        public FeatureNotEnabledException(string message) : base(message) { }
        public FeatureNotEnabledException(string message, Exception exception) : base(message, exception) { }
        protected FeatureNotEnabledException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
