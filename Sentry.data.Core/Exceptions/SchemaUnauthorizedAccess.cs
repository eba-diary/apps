using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Exceptions
{
    public class SchemaUnauthorizedAccess : UnauthorizedAccessException
    {
        public SchemaUnauthorizedAccess() { }
        public SchemaUnauthorizedAccess(string message) : base(message) { }
        public SchemaUnauthorizedAccess(string message, Exception exception) : base(message, exception) { }
    }
}
