using Polly;
using Polly.Registry;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.data.Core.Interfaces;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class ApacheLivyProvider : IApacheLivyProvider
    {
        private readonly IAsyncPolicy _asyncProviderPolicy;
        private readonly IHttpClientProvider _httpClient;
        private string _baseUrl;

        public ApacheLivyProvider(IHttpClientProvider httpClientProvider, IPolicyRegistry<string> policyRegistry)
        {
            _httpClient = httpClientProvider;
            _asyncProviderPolicy = policyRegistry.Get<IAsyncPolicy>(PollyPolicyKeys.ApacheLivyProviderAsyncPolicy);
        }

        public void SetBaseUrl(string baseUrl)
        {
            this._baseUrl = baseUrl;
        }

        public string GetBaseUrl()
        {
            return _baseUrl;
        }

        public async Task<HttpResponseMessage> PostRequestAsync(string resource, string content)
        {
            HttpContent contentPost = new StringContent(content, Encoding.UTF8, "application/json");

            return await PostRequestAsync(resource, contentPost).ConfigureAwait(false);
        }

        public virtual async Task<HttpResponseMessage> PostRequestAsync(string resource, HttpContent postContent)
        {
            string stringContent = await postContent.ReadAsStringAsync().ConfigureAwait(false);
            Logger.Debug($"{nameof(PostRequestAsync)} - baseurl:{_baseUrl}");
            Logger.Debug($"{nameof(PostRequestAsync)} - resource:{resource}");
            Logger.Debug($"{nameof(PostRequestAsync)} - postContent:{stringContent}");
            var pollyResponse = await _asyncProviderPolicy.ExecuteAsync(async () =>
            {
                var x =  await _httpClient.PostAsync(_baseUrl + $"/{resource}", postContent).ConfigureAwait(false);

                return x;

            }).ConfigureAwait(false);

             HttpResponseMessage response = pollyResponse;

            return response;
        }

        public virtual Task<HttpResponseMessage> GetRequestAsync(string resource)
        {
            if (string.IsNullOrEmpty(_baseUrl)) { throw new ArgumentNullException(nameof(resource),"Client url is required"); }
            if (string.IsNullOrEmpty(resource)) { throw new ArgumentNullException(nameof(resource),"resource is required"); }


            Logger.Debug($"{nameof(GetRequestAsync)} - baseurl:{_baseUrl}");
            Logger.Debug($"{nameof(GetRequestAsync)} - resource:{resource}");

            return GetRequestAsync();
            async Task<HttpResponseMessage> GetRequestAsync()
            {
                return await _asyncProviderPolicy.ExecuteAsync(async () =>
                {
                    var x = await _httpClient.GetAsync(_baseUrl + resource).ConfigureAwait(false);

                    return x;

                }).ConfigureAwait(false);
            }
        }

        internal virtual async Task<HttpResponseMessage> GetRequestInternalAsync(string resource)
        {
            return await _asyncProviderPolicy.ExecuteAsync(async () =>
            {
                var x = await _httpClient.GetAsync(_baseUrl + resource).ConfigureAwait(false);

                return x;

            }).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> DeleteRequestAsync(string resource)
        {
            var pollyResponse = await _asyncProviderPolicy.ExecuteAsync(async () =>
            {
                var x = await _httpClient.DeleteAsync(_baseUrl + resource).ConfigureAwait(false);

                return x;

            }).ConfigureAwait(false);

            HttpResponseMessage response = pollyResponse;

            return response;
        }
    }
}
