using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Sentry.data.Core.Exceptions
{
    public class DatasetNotFound : Exception
    {
        public DatasetNotFound() { }
        public DatasetNotFound(string message) : base(message) { }
        public DatasetNotFound(string message, Exception exception) : base(message, exception) { }
    }
}
