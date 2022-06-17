using Polly;
using Polly.Registry;
using Sentry.data.Core;
using Sentry.data.Core.Interfaces;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class ConfluentConnectorProvider : IKafkaConnectorProvider
    {
        private readonly IAsyncPolicy _asyncProviderPolicy;
        private readonly IHttpClientProvider _httpClient;

        public ConfluentConnectorProvider(IHttpClientProvider httpClientProvider, IPolicyRegistry<string> policyRegistry)
        {
            _httpClient = httpClientProvider;
            _asyncProviderPolicy = policyRegistry.Get<IAsyncPolicy>(PollyPolicyKeys.ApacheLivyProviderAsyncPolicy);
        }

        public string BaseUrl { get; set; } = "";

        public async Task<HttpResponseMessage> PostRequestAsync(string resource, HttpContent postContent)
        {
            var pollyResponse = await _asyncProviderPolicy.ExecuteAsync(async () =>
            {
                var x =  await _httpClient.PostAsync(BaseUrl + $"/{resource}", postContent).ConfigureAwait(false);

                return x;

            }).ConfigureAwait(false);

             HttpResponseMessage response = pollyResponse;

            return response;
        }

        public async Task<HttpResponseMessage> GetRequestAsync(string resource)
        {
            var pollyResponse = await _asyncProviderPolicy.ExecuteAsync(async () =>
            {
                var x = await _httpClient.GetAsync(BaseUrl + resource).ConfigureAwait(false);

                return x;

            }).ConfigureAwait(false);

            HttpResponseMessage response = pollyResponse;

            return response;
        }

        public async Task<HttpResponseMessage> DeleteRequestAsync(string resource)
        {
            var pollyResponse = await _asyncProviderPolicy.ExecuteAsync(async () =>
            {
                var x = await _httpClient.DeleteAsync(BaseUrl + resource).ConfigureAwait(false);

                return x;

            }).ConfigureAwait(false);

            HttpResponseMessage response = pollyResponse;

            return response;
        }
    }
}
