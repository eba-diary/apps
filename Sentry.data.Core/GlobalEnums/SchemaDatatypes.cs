using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public enum SchemaDatatypes
    {
        NONE = 0,
        STRUCT = 1,
        INTEGER = 2,
        BIGINT = 3,
        DECIMAL = 4,
        VARCHAR = 5,
        DATE = 6,
        TIMESTAMP = 7,
        VARIANT = 8
    }
}
