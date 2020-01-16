using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Exceptions
{
    public class SchemaNotFound : Exception
    {
        public SchemaNotFound() { }
        public SchemaNotFound(string message) : base(message) { }
        public SchemaNotFound(string message, Exception exception) : base(message, exception) { }
    }
}
