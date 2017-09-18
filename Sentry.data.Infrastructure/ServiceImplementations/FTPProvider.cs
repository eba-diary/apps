using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;
using System.Net;
using RestSharp;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Sentry.data.Infrastructure
{
    class FTPProvider : IFtpProvider
    {
        private int _maxCacheSize = 400000;
        private int _bufferSize = 10000;
        private FtpWebRequest _ftpRequest;
        private string _url;
        private RestRequest _request;
        private FtpWebResponse _response = null;
        private JObject _jobject;
        private Stream _responseStream = null;
        private MemoryStream _downloadCache = null;
        private Stream _responsestream;

        private void CreateDwnldRequest(string url, NetworkCredential creds)
        {
            this._ftpRequest = (FtpWebRequest)WebRequest.Create(url);
            this._ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;

            this._ftpRequest.Proxy = new WebProxy(Configuration.Config.GetHostSetting("SentryWebProxyHost"), Int32.Parse(Configuration.Config.GetSetting("SentryWebProxyPort")));
            this._ftpRequest.Proxy.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
            this._ftpRequest.Credentials = creds;
            
        }

        public void DownloadFile(string url, NetworkCredential cred, string destination)
        {
            CreateDwnldRequest(url, cred);
            ProcessDownloadRequest(url, destination);
        }

        private void ProcessDownloadRequest(string url, string destination)
        {
            using (Stream ftpStream = this._ftpRequest.GetResponse().GetResponseStream())
            using (Stream fileStream = new FileStream(destination, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
            {
                ftpStream.CopyTo(fileStream);
            }
        }

        /// <summary>
        /// Write the data in cache to local file.
        /// </summary>
        void WriteCacheToFile(MemoryStream downloadCache, string downloadPath,
            int cachedSize)
        {
            using (FileStream fileStream = new FileStream(downloadPath,
                FileMode.Append))
            {
                byte[] cacheContent = new byte[cachedSize];
                downloadCache.Seek(0, SeekOrigin.Begin);
                downloadCache.Read(cacheContent, 0, cachedSize);
                fileStream.Write(cacheContent, 0, cachedSize);
            }
        }

    }
}
