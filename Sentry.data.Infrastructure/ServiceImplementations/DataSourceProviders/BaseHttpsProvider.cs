﻿using Newtonsoft.Json;
using Polly;
using RestSharp;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Sentry.data.Infrastructure
{
    public abstract class BaseHttpsProvider : BaseJobProvider, IBaseHttpsProvider
    {
        #region Declarations
        protected IRestClient _client;
        protected IRestRequest _request;
        protected Uri _uri;
        private readonly Lazy<IDatasetContext> _dsContext;
        protected Lazy<IConfigService> _configService;
        private readonly Lazy<IEncryptionService> _encryptionService;
        protected RetrieverJob _targetJob;
        protected DataFlowStep _targetStep;
        protected IAsyncPolicy _providerPolicyAsync;
        protected ISyncPolicy _providerPolicy;
        protected readonly IDataFeatures _dataFeatures;
        #endregion

        protected BaseHttpsProvider(Lazy<IDatasetContext> datasetContext, 
            Lazy<IConfigService> configService, Lazy<IEncryptionService> encryptionService,
            IRestClient restClient, IDataFeatures dataFeatures)
        {
            _dsContext = datasetContext;
            _configService = configService;
            _encryptionService = encryptionService;
            _client = restClient;
            _dataFeatures = dataFeatures;
        }

        protected IDatasetContext DatasetContext
        {
            get { return _dsContext.Value; }
        }
        protected IEncryptionService EncryptionService
        {
            get { return _encryptionService.Value; }
        }
        protected IConfigService ConfigService
        {
            get { return _configService.Value; }
        }
        
        public virtual IRestRequest Request
        {
            get { return _request; }
        }

        //public virtual RetrieverJob Job
        //{
        //    get { return _job; }
        //    set { _job = value; }
        //}

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

        public void CopyToStream(Stream targetStream)
        {
            string methodName = $"{nameof(BaseHttpsProvider).ToLower()}_{nameof(ConfigureClient).ToLower()}";
            Logger.Debug($"{methodName} Method Start");

            NetworkCredential proxyCredentials;
            string proxyUrl;

            if (_dataFeatures.CLA3819_EgressEdgeMigration.GetValue())
            {
                Logger.Debug($"{methodName} using edge proxy: true");
                string userName = Configuration.Config.GetHostSetting("ServiceAccountID");
                string password = Configuration.Config.GetHostSetting("ServiceAccountPassword");
                proxyUrl = Configuration.Config.GetHostSetting("EdgeWebProxyUrl");
                proxyCredentials = new NetworkCredential(userName, password);
            }
            else
            {
                Logger.Debug($"{methodName} using edge proxy: false");
                proxyUrl = Configuration.Config.GetHostSetting("WebProxyUrl");
                proxyCredentials = CredentialCache.DefaultNetworkCredentials;
            }
            Logger.Debug($"{methodName} proxyUser: {proxyCredentials.UserName}");

            RestClient client = new RestClient
            {
                Proxy = new WebProxy(proxyUrl)
                {
                    Credentials = proxyCredentials
                }
            };

            _request.ResponseWriter = (responseStream) => responseStream.CopyTo(targetStream);

            client.DownloadData(_request);

            Logger.Debug($"{methodName} Method End");
        }

        public abstract List<IRestResponse> SendPagingRequest();

        #region TargetSpecificMethods
        protected abstract void FindTargetJob();
        protected abstract void SetTargetPath(string extension);
        #endregion

        #region TokenAuthSpecific
        protected void ConfigureTokenAuth(IRestRequest req, RetrieverJob job)
        {
            req.AddHeader(((HTTPSSource)job.DataSource).AuthenticationHeaderName, EncryptionService.DecryptString(((HTTPSSource)job.DataSource).AuthenticationTokenValue, Configuration.Config.GetHostSetting("EncryptionServiceKey"), ((HTTPSSource)job.DataSource).IVKey));
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

            sourceClaims = DatasetContext.OAuthClaims.Where(w => w.DataSourceId == source).ToList();

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
