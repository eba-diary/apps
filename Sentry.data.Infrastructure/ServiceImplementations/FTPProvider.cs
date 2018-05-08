using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Sentry.data.Core;
using System.Net;
using RestSharp;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Sentry.data.Infrastructure
{
    public class FtpProvider : IFtpProvider, IDisposable
    {

        private FtpWebRequest _ftpRequest;
        private Stream _streamResult;        

        private void CreateDwnldRequest(Uri url, NetworkCredential creds)
        {
            this._ftpRequest = (FtpWebRequest)WebRequest.Create(url);
            this._ftpRequest.Proxy = new WebProxy(Configuration.Config.GetHostSetting("SentryWebProxyHost"), Int32.Parse(Configuration.Config.GetSetting("SentryWebProxyPort")));
            this._ftpRequest.Proxy.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
            this._ftpRequest.Credentials = creds;
            this._ftpRequest.ReadWriteTimeout = Timeout.Infinite;
            this._ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
        }

        public void Dispose()
        {
            if (_streamResult != null) _streamResult.Dispose();
        }

        public Stream GetJobStream(RetrieverJob Job)
        {
            CreateDwnldRequest(Job.GetUri(), Job.DataSource.SourceAuthType.GetCredentials(Job));
            _streamResult = this._ftpRequest.GetResponse().GetResponseStream();
            return _streamResult;
        }
    }
}
