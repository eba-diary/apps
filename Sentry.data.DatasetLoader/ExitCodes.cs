using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.DatasetLoader
{
    enum ExitCodes : int
    {
        Success = 0,
        Failure = 1,
        InvalidJson = 2,
        S3UploadError = 3,
        DatabaseError = 4

    }
}
