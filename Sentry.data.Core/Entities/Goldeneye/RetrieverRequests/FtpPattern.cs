using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public enum FtpPattern
    {
        NoPattern = 0,
        SpecificFileNoDelete = 1,
        RegexFileNoDelete = 2,
        SpecificFileArchive = 3,
        RegexFileSinceLastExecution = 4,
        SpecificFileSinceLastexecution = 5
    }
}
