using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.Exceptions
{
    [Serializable]
    public class JiraServiceException : Exception
    {
        public JiraServiceException() { }
        public JiraServiceException(string message) : base(message) { }
        public JiraServiceException(string message, Exception exception) : base(message, exception) { }
        protected JiraServiceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
