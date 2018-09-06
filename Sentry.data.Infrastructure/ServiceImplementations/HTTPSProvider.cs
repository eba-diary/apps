using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure
{
    public class HTTPSProvider
    {
        private HttpWebRequest _request;
        private Uri _uri;
        
        public HTTPSProvider(RetrieverJob job, List<KeyValuePair<string,string>> headers)
        {
            _uri = job.GetUri();
            _request = (HttpWebRequest)WebRequest.Create(_uri);
            _request.Method = "GET";
            _request.Proxy = new WebProxy(Configuration.Config.GetHostSetting("SentryWebProxyHost"))
            {
                Credentials = CredentialCache.DefaultNetworkCredentials
            };

            if (job.DataSource.SourceAuthType.Is<TokenAuthentication>())
            {
                EncryptionService encryptService = new EncryptionService();
                _request.Headers.Add(((HTTPSSource)job.DataSource).AuthenticationHeaderName, encryptService.DecryptString(((HTTPSSource)job.DataSource).AuthenticationTokenValue, Configuration.Config.GetHostSetting("EncryptionServiceKey"), ((HTTPSSource)job.DataSource).IVKey));
            }
            
            //Add datasource specific headers to request
            List<RequestHeader> headerList = ((HTTPSSource)job.DataSource).RequestHeaders;

            if (headerList != null)
            {
                foreach (RequestHeader header in headerList)
                {
                    switch (header.Key.ToUpper())
                    {
                        case "ACCEPT":
                            _request.Accept = header.Value;
                            break;
                        case "EXPECT":
                            _request.Expect = header.Value;
                            break;
                        default:
                            _request.Headers.Add(header.Key, header.Value);
                            break;
                    }
                }
            }            
        }

        public HttpWebResponse SendRequest()
        {
            return (HttpWebResponse)_request.GetResponse();                    
        } 
    }
}
