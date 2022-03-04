using Polly;
using Polly.Registry;
using Sentry.data.Core;
using Sentry.data.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace Sentry.data.Infrastructure
{
    public class FtpProvider : IFtpProvider, IDisposable
    {

        private Stream _streamResult;
        private NetworkCredential _creds;
        private readonly ISyncPolicy _providerPolicy;
        private readonly IDataFeatures _dataFeatures;

        public FtpProvider(IReadOnlyPolicyRegistry<string> registry, IDataFeatures dataFeatures)
        {
            _providerPolicy = registry.Get<ISyncPolicy>(PollyPolicyKeys.FtpProviderPolicy);
            _dataFeatures = dataFeatures;
        }
        
        public void SetCredentials(NetworkCredential creds)
        {
            _creds = creds;
        }

        private FtpWebRequest CreateDwnldRequest(string url, NetworkCredential creds)
        {
            FtpWebRequest req = (FtpWebRequest)WebRequest.Create(url);

            string proxyUrl = (_dataFeatures.CLA3819_EgressEdgeMigration.GetValue()) ? Configuration.Config.GetHostSetting("EdgeWebProxyUrl") : Configuration.Config.GetHostSetting("WebProxyUrl");

            req.Proxy = new WebProxy(proxyUrl)
            {
                Credentials = System.Net.CredentialCache.DefaultNetworkCredentials
            };
            req.Credentials = creds;
            req.ReadWriteTimeout = Timeout.Infinite;
            return req;
        }

        public void Dispose()
        {
            if (_streamResult != null) _streamResult.Dispose();
        }

        public Stream GetFileStream(string url)
        {
            if (_creds == null)
            {
                throw new ArgumentException("Set provider credentials");
            }
            FtpWebRequest request = CreateDwnldRequest(url, _creds);
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            WebResponse response = null;

            //Use Polly to perform retry logic
            _providerPolicy.Execute(() =>
            {
                response = request.GetResponse();
            });

            if (response == null)
            {
                throw new RetrieverJobProcessingException($"No Response returned - url:{url}");
            }

            _streamResult = response.GetResponseStream();


            return _streamResult;
        }

        public WebResponse DeleteFile(FtpWebRequest request, string directory, string fileName)
        {
            request.Method = WebRequestMethods.Ftp.DeleteFile;
            return request.GetResponse();
        }

        public List<RemoteFile> ListDirectoryContent(string url, string filter)
        {
            if (_creds == null)
            {
                throw new ArgumentException("Set provider credentials");
            }

            FtpWebRequest request = CreateDwnldRequest(url, _creds);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

            FtpWebResponse response = null;

            //Use Polly to perform retry logic
            _providerPolicy.Execute(() =>
            {
                response = (FtpWebResponse)request.GetResponse();
            });

            if (response == null)
            {
                throw new RetrieverJobProcessingException($"No Response returned - url:{url}");
            }

            Stream responseStream = response.GetResponseStream();


            //Pattern used: https://stackoverflow.com/a/39771146
            string pattern = @"^(\w+\s\d+\s\d+\s+\d+:\d+)\s+(Directory|<DIR>|\d+)\s+<A HREF=\S+>(.+)<\S+$";
            Regex regex = new Regex(pattern);
            List<RemoteFile> resultList = new List<RemoteFile>();

            using (StreamReader reader = new StreamReader(responseStream))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    Match match = regex.Match(line);

                    if (match.Length > 0)
                    {
                        RemoteFile file = new RemoteFile()
                        {
                            Name = match.Groups[3].Value,
                            Size = (match.Groups[2].Value != "<DIR>" && match.Groups[2].Value != "Directory") ? long.Parse(match.Groups[2].Value) : 0,
                            Modified = DateTime.Parse(match.Groups[1].Value),
                            Type = (match.Groups[2].Value != "<DIR>" && match.Groups[2].Value != "Directory") ? "File" : "Directory"
                        };

                        resultList.Add(file);
                    }
                }
            }

            switch (filter.ToUpper())
            {
                case "FILES":
                    return resultList.Where(w => w.Type == "File").ToList();
                case "DIRECTORIES":
                    return resultList.Where(w => w.Type == "Directory").ToList();
                default:
                    return resultList;
            }
        }


        //MakeDirectory is not allowed through HTTP proxy.
        // Leaving code here incase we want to revisit alternate ways to 
        //  not utilize proxy for these requests.
        public void CreateDirectory(string url)
        {
            if (_creds == null)
            {
                throw new ArgumentException("Set provider credentials");
            }

            FtpWebRequest request = CreateDwnldRequest(url, _creds);
            request.Proxy = null;
            request.Method = WebRequestMethods.Ftp.MakeDirectory;
            FtpWebResponse resp = (FtpWebResponse)request.GetResponse();
        }

        public void RenameFile(string sourceUrl, string targetUrl)
        {
            if (_creds == null)
            {
                throw new ArgumentException("Set provider credentials");
            }

            //Add logic to append date and timestamp to file name before renaming.

            FtpWebRequest request = CreateDwnldRequest(sourceUrl, _creds);
            request.Method = WebRequestMethods.Ftp.Rename;
            request.RenameTo = targetUrl;
            FtpWebResponse resp = (FtpWebResponse)request.GetResponse();
        }

        #region WinSCPImplementation

        //public FtpProvider (string hostName, int portNumber, NetworkCredential creds)
        //{
        //    CreateSessionOptions(hostName, portNumber, creds);
        //    session = new Session();
        //    var d = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss");
        //    session.DebugLogPath = "C:\\Temp\\AppLogs\\WinSCP\\Debuglog_"+ d + ".txt";
        //    session.SessionLogPath = "C:\\Temp\\AppLogs\\WinSCP\\Sessionlog\\Sessionlog_" + d + ".txt";

        //    //session.Open(_sessionOptions);

        //}

        //private void CreateSessionOptions(string hostname, int portNumber, NetworkCredential creds)
        //{
        //    _sessionOptions = new SessionOptions
        //    {
        //        Protocol = Protocol.Ftp,
        //        //HostName = hostname,
        //        //UserName = creds.UserName,
        //        //Password = creds.Password,
        //        HostName = "ftp.ncdc.noaa.gov",
        //        UserName = "ftp",
        //        Password = "janeDoe@Sentry.com",
        //        PortNumber = portNumber,
        //        Timeout = TimeSpan.FromSeconds(120),
        //        FtpMode = FtpMode.Passive

        //    };
        //    _sessionOptions.AddRawSettings("ProxyHost", "webproxy.sentry.com");
        //    _sessionOptions.AddRawSettings("ProxyPort", "80");
        //    _sessionOptions.AddRawSettings("ProxyMethod", "3");
        //    _sessionOptions.AddRawSettings("ProxyUsername", Configuration.Config.GetHostSetting("ServiceAccountID"));
        //    _sessionOptions.AddRawSettings("ProxyPassword", Configuration.Config.GetHostSetting("ServiceAccountPass"));

        //    _sessionOptions.AddRawSettings("FtpForcePasvIp2", "0");

        //    //_sessionOptions.AddRawSettings("ProxyMethod", "4");
        //    //_sessionOptions.AddRawSettings("FtpProxyLogonType", "5");
        //}

        //public RemoteDirectoryInfo ListFiles()
        //{
        //    RemoteDirectoryInfo dirInfo = session.ListDirectory(remotePath);
        //    return dirInfo;
        //}

        //public void GetFile(string fileName, string targetPath)
        //{
        //    if (String.IsNullOrWhiteSpace(remotePath))
        //    {
        //        throw new ArgumentNullException("remotePath", "remotePath is required to be set.");
        //    }

        //    session.GetFiles(RemotePath.EscapeFileMask(remotePath + "/" + fileName), targetPath).Check();
        //}        
        #endregion
    }

}
