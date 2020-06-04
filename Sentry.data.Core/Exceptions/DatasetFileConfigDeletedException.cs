using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    public class DatasetFileConfigDeletedException : Exception
    {
        public DatasetFileConfigDeletedException() { }
        public DatasetFileConfigDeletedException(string message) : base(message) { }
        public DatasetFileConfigDeletedException(string message, Exception exception) : base(message, exception) { }
        protected DatasetFileConfigDeletedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
