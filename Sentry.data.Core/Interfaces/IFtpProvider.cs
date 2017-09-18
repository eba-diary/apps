using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IFtpProvider
    {
        void DownloadFile(string url, NetworkCredential cred, string destination);
    }
}
