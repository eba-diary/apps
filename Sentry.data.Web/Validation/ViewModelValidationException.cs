using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Sentry.data.Web
{
    [Serializable]
    public class ViewModelValidationException : Exception
    {
        public ViewModelValidationException()
        {
        }

        public ViewModelValidationException(List<ValidationResultViewModel> validationResults)
        {
            ValidationResults = validationResults;
        }

        public ViewModelValidationException(string message) : base(message)
        {
        }

        public ViewModelValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ViewModelValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public List<ValidationResultViewModel> ValidationResults { get; }
    }
}