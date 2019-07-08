using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using RestSharp;
using Sentry.data.Core;
using StructureMap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Sentry.Common.Logging;

namespace Sentry.data.Infrastructure
{
    public abstract class BaseHttpsProvider : BaseJobProvider, IBaseHttpsProvider
    {
        #region Declarations
        protected IRestClient _client;
        protected IRestRequest _request;
        protected Uri _uri;
        protected IDatasetContext _dsContext;
        protected IConfigService _configService;
        protected IEncryptionService _encryptionService;
        #endregion

        protected BaseHttpsProvider(IDatasetContext datasetContext, 
            IConfigService configService, IEncryptionService encryptionService)
        {
            _dsContext = datasetContext;
            _configService = configService;
            _encryptionService = encryptionService;
        }

        protected abstract void ConfigureClient();
        protected abstract void ConfigureRequest();

        public override void ConfigureProvider(RetrieverJob job)
        {
            _job = job;
            _uri = job.GetUri();
            _request = new RestRequest();

            if (job.DataSource.SourceAuthType.Is<TokenAuthentication>())
            {
                ConfigureTokenAuth(_request, job);
            }

            if (job.DataSource.SourceAuthType.Is<OAuthAuthentication>())
            {
                ConfigureOAuth(_request, job);                
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
                            _request.AddHeader("Accept", header.Value);
                            break;
                        case "EXPECT":
                            _request.AddHeader("Except", header.Value);
                            break;
                        default:
                            _request.AddHeader(header.Key, header.Value);
                            break;
                    }
                }
            }

            _request.Resource = _uri.ToString();
        }

        public abstract IRestResponse SendRequest();
        //{
        //    List<IRestResponse> responses = new List<IRestResponse>();

        //    RestClient client = new RestClient
        //    {
        //        Proxy = new WebProxy(Configuration.Config.GetHostSetting("SentryWebProxyHost"))
        //        {
        //            Credentials = CredentialCache.DefaultNetworkCredentials
        //        }
        //    };

        //    responses.Add(client.ExecuteAsGet(_request, "GET"));
            
        //    return responses;
        //}

        public void CopyToStream(Stream targetStream)
        {
            RestClient client = new RestClient
            {
                Proxy = new WebProxy(Configuration.Config.GetHostSetting("SentryWebProxyHost"))
                {
                    Credentials = CredentialCache.DefaultNetworkCredentials
                }
            };

            _request.ResponseWriter = (responseStream) => responseStream.CopyTo(targetStream);

            client.DownloadData(_request);            
        }

        public abstract List<IRestResponse> SendPagingRequest();

        #region TokenAuthSpecific
        protected void ConfigureTokenAuth(IRestRequest req, RetrieverJob job)
        {
            req.AddHeader(((HTTPSSource)job.DataSource).AuthenticationHeaderName, _encryptionService.DecryptString(((HTTPSSource)job.DataSource).AuthenticationTokenValue, Configuration.Config.GetHostSetting("EncryptionServiceKey"), ((HTTPSSource)job.DataSource).IVKey));
        }
        #endregion

        #region OAuthSpecific
        protected abstract string GetOAuthAccessToken(HTTPSSource source);
        protected abstract void ConfigureOAuth(IRestRequest req, RetrieverJob job);
        protected abstract void AddOAuthGrantType(List<KeyValuePair<string, string>> list, HTTPSSource source);
        protected abstract string SignOAuthToken(string claims, HTTPSSource source);
        protected abstract string GenerateJwtToken(HTTPSSource source);
        #endregion

        #region Private Methods
        protected static string Base64UrlEncode(byte[] input)
        {
            var output = Convert.ToBase64String(input);
            output = output.Split('=')[0]; // Remove any trailing '='s
            output = output.Replace('+', '-'); // 62nd char of encoding
            output = output.Replace('/', '_'); // 63rd char of encoding
            return output;
        }
        protected string GenerateClaims(HTTPSSource source)
        {
            Dictionary<string, object> claims = new Dictionary<string, object>();
            List<OAuthClaim> sourceClaims;

            sourceClaims = _dsContext.OAuthClaims.Where(w => w.DataSourceId == source).ToList();

            foreach (OAuthClaim claim in sourceClaims)
            {
                switch (claim.Type)
                {
                    case Core.GlobalEnums.OAuthClaims.exp:
                        claims.Add(claim.Type.ToString(), DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Add(TimeSpan.FromSeconds(source.TokenExp)).TotalSeconds);
                        break;
                    default:
                        claims.Add(claim.Type.ToString(), claim.Value);
                        break;
                }
            }

            //add Issue At time
            claims.Add("iat", DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);

            return JsonConvert.SerializeObject(claims);
        }
        protected static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(timestamp);
        }
        #endregion
    }
}
