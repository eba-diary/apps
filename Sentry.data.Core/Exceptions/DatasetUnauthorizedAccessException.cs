﻿using System;
using System.Runtime.Serialization;

namespace Sentry.data.Core.Exceptions
{
    public class DatasetUnauthorizedAccessException : UnauthorizedAccessException, ISerializable
    {
        public DatasetUnauthorizedAccessException() { }
        public DatasetUnauthorizedAccessException(string message) : base(message) { }
        public DatasetUnauthorizedAccessException(string message, Exception inner) : base(message, inner) { }
        protected DatasetUnauthorizedAccessException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
