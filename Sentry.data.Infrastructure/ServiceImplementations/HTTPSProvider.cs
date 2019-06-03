using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
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

            if (job.DataSource.SourceAuthType.Is<OAuthAuthentication>())
            {       
                //
                var serviceAccountName = "oauthtest@akonkman1.iam.gserviceaccount.com";
                var permissions = "https://www.googleapis.com/auth/analytics.readonly";
                var tokenProvider = "https://www.googleapis.com/oauth2/v4/token";
                var privateKey = "-----BEGIN PRIVATE KEY----- private_key_goes_here... -----END PRIVATE KEY-----";

                // Create the JWT token
                var claims = new
                {
                    iss = serviceAccountName,
                    scope = permissions,
                    aud = tokenProvider,
                    exp = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Add(TimeSpan.FromMinutes(30)).TotalSeconds,
                    iat = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds,
                };

                var claimsPart = JsonConvert.SerializeObject(claims);
                privateKey = privateKey.Replace("-----BEGIN PRIVATE KEY-----", "").Replace("-----END PRIVATE KEY-----", "").Replace("\n", "");
                var jwtToken = Sign(claimsPart, privateKey);

                // Get the OAuth access token
                var httpClient = new System.Net.Http.HttpClient();
                var keyValues = new List<KeyValuePair<string, string>>();
                keyValues.Add(new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"));
                keyValues.Add(new KeyValuePair<string, string>("assertion", jwtToken));
                var oAuthPostContent = new System.Net.Http.FormUrlEncodedContent(keyValues);
                var oAuthPostResult = httpClient.PostAsync(tokenProvider, oAuthPostContent).Result;
                var response = oAuthPostResult.Content.ReadAsStringAsync().Result;
                var responseAsJson = Newtonsoft.Json.Linq.JObject.Parse(response);
                var accessToken = responseAsJson.GetValue("access_token");


                UriBuilder baseUri = new UriBuilder(job.GetUri());

                string queryToAppend = "access_token=" + accessToken;

                if (baseUri.Query != null && baseUri.Query.Length > 1)
                    baseUri.Query = baseUri.Query.Substring(1) + "&" + queryToAppend;
                else
                    baseUri.Query = queryToAppend;

                _uri = baseUri.Uri;
            };
            
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

        //private object Sign(string claimsPart, string privateKey)
        //{
        //    throw new NotImplementedException();
        //}

        public HttpWebResponse SendRequest()
        {
            return (HttpWebResponse)_request.GetResponse();                    
        } 
    }
}
