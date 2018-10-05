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
        void SetCredentials(NetworkCredential creds);
        Stream GetFileStream(string url);
        List<RemoteFile> ListDirectoryContent(string url, string filter);
        void CreateDirectory(string url);
        void RenameFile(string sourceUrl, string targetUrl);
    }
}
