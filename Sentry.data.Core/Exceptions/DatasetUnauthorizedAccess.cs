using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Exceptions
{
    public class DatasetUnauthorizedAccess : UnauthorizedAccessException
    {
        public DatasetUnauthorizedAccess() { }
        public DatasetUnauthorizedAccess(string message) : base(message) { }
        public DatasetUnauthorizedAccess(string message, Exception inner) : base(message, inner) { }
    }
}
