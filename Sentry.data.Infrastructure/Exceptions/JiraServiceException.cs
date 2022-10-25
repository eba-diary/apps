using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.Exceptions
{
    [Serializable]
    public class JiraServiceException : Exception
    {
        public JiraServiceException(string message) : base(message)
        {

        }
    }
}
