using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IFtpProvider
    {
        Stream GetJobStream(RetrieverJob Job);
    }
}
