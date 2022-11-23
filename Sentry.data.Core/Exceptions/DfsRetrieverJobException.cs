using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    [Serializable]
    public class DfsRetrieverJobException : Exception
    {
        public DfsRetrieverJobException() { }
        public DfsRetrieverJobException(string message) : base(message) { }
        public DfsRetrieverJobException(string message, Exception exception) : base(message, exception) { }
        protected DfsRetrieverJobException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
