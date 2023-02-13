using System;
using System.Runtime.Serialization;

namespace Sentry.data.Web.API
{
    [Serializable]
    public class RequestModelValidationException : Exception
    {
        public RequestModelValidationException()
        {
        }

        public RequestModelValidationException(ValidationResponseModel validationResponse)
        {
            ValidationResponse = validationResponse;
        }

        public RequestModelValidationException(string message) : base(message)
        {
        }

        public RequestModelValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RequestModelValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ValidationResponseModel ValidationResponse { get; }
    }
}