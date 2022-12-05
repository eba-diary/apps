using Amazon.Auth.AccessControlPolicy;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class PagingHttpsJobProvider : IBaseJobProvider
    {
        #region Fields
        private readonly IDatasetContext _datasetContext;
        private readonly IS3ServiceProvider _s3ServiceProvider;
        private readonly IAuthorizationProvider _authorizationProvider;
        private readonly IHttpClientGenerator _httpClientGenerator;
        #endregion

        #region Constructor
        public PagingHttpsJobProvider(IDatasetContext datasetContext, IS3ServiceProvider s3ServiceProvider, IAuthorizationProvider authorizationProvider, IHttpClientGenerator httpClientGenerator) 
        {
            _datasetContext = datasetContext;
            _s3ServiceProvider = s3ServiceProvider;
            _authorizationProvider = authorizationProvider;
            _httpClientGenerator = httpClientGenerator;
        }
        #endregion

        #region IBaseJobProvider Implementation
        public void Execute(RetrieverJob job)
        {
            using (_authorizationProvider)
            {
                HTTPSSource source = (HTTPSSource)job.DataSource;

                using (HttpClient httpClient = _httpClientGenerator.GenerateHttpClient(job.DataSource.BaseUri.ToString()))
                {
                    httpClient.BaseAddress = source.BaseUri;
                    httpClient.Timeout = new TimeSpan(0, 10, 0);

                    if (source.SourceAuthType.Is<OAuthAuthentication>())
                    {
                        foreach (DataSourceToken dataSourceToken in source.Tokens)
                        {
                            string token = _authorizationProvider.GetOAuthAccessToken(source, dataSourceToken);
                            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                            RetrieveData(source, httpClient);

                            httpClient.DefaultRequestHeaders.Clear();
                        }
                    }
                    else
                    {
                        if (source.SourceAuthType.Is<TokenAuthentication>())
                        {
                            string token = _authorizationProvider.GetTokenAuthenticationToken(source);
                            httpClient.DefaultRequestHeaders.Add(source.AuthenticationHeaderName, token);
                        }

                        RetrieveData(source, httpClient);
                    }
                }
            }
        }
        #endregion

        #region Private
        private void RetrieveData(HTTPSSource source, HttpClient httpClient)
        {

        }
        #endregion

        #region Not Implemented
        public void ConfigureProvider(RetrieverJob job)
        {
            throw new NotImplementedException();
        }

        public void Execute(RetrieverJob job, string filePath)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
