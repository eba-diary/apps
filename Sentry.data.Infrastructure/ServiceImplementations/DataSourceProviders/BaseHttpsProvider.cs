using Newtonsoft.Json;
using Polly;
using RestSharp;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using StructureMap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;

namespace Sentry.data.Infrastructure
{
    public abstract class BaseHttpsProvider : BaseJobProvider, IBaseHttpsProvider
    {
        #region Declarations
        protected RestClient _client;
        protected RestRequest _request;
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
            RestClient restClient, IDataFeatures dataFeatures)
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
        
        public virtual RestRequest Request
        {
            get { return _request; }
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

        public abstract RestResponse SendRequest();

        public static string ParseContentType(string contentType)
        {
            //Mime types
            //https://technet.microsoft.com/en-us/library/cc995276.aspx
            //https://www.iana.org/assignments/media-types/media-types.xhtml

            Logger.Info($"incoming_contenttype - {contentType}");

            var content = new ContentType(contentType);

            using (IContainer Container = Sentry.data.Infrastructure.Bootstrapper.Container.GetNestedContainer())
            {
                IDatasetContext _datasetContext = Container.GetInstance<IDatasetContext>();

                MediaTypeExtension extensions = _datasetContext.MediaTypeExtensions.Where(w => w.Key == content.MediaType).FirstOrDefault();

                if (extensions == null)
                {
                    Logger.Warn($"Detected new MediaType ({content.MediaType}), defaulting to txt");
                    return "txt";
                }

                Logger.Info($"detected_mediatype - {extensions.Value}");
                return extensions.Value;
            }
        }

        public abstract List<RestResponse> SendPagingRequest();

        #region TargetSpecificMethods
        protected abstract void FindTargetJob();
        protected abstract void SetTargetPath(string extension);
        #endregion

        #region TokenAuthSpecific
        protected void ConfigureTokenAuth(RestRequest req, RetrieverJob job)
        {
            req.AddHeader(((HTTPSSource)job.DataSource).AuthenticationHeaderName, EncryptionService.DecryptString(((HTTPSSource)job.DataSource).AuthenticationTokenValue, Configuration.Config.GetHostSetting("EncryptionServiceKey"), ((HTTPSSource)job.DataSource).IVKey));
        }
        #endregion

        #region OAuthSpecific
        protected abstract string GetOAuthAccessToken(HTTPSSource source);
        protected abstract void ConfigureOAuth(RestRequest req, RetrieverJob job);
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
                        claims.Add(claim.Type.ToString(), DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Add(TimeSpan.FromSeconds(source.GetActiveTokens().FirstOrDefault().TokenExp)).TotalSeconds);
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
