using Polly;
using Polly.Registry;
using Sentry.Configuration;
using Sentry.data.Core;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class ApacheLivyProvider : IApacheLivyProvider
    {
        private readonly IAsyncPolicy _asyncProviderPolicy;
        private readonly IAsyncPolicy<HttpResponseMessage> _asyncProviderPolicy_WithTimeout;

        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ApacheLivyProvider(HttpClient httpClient, IPolicyRegistry<string> policyRegistry)
        {
            _httpClient = httpClient;
            _baseUrl = Config.GetHostSetting("ApacheLivy");
            _asyncProviderPolicy = policyRegistry.Get<IAsyncPolicy>(PollyPolicyKeys.ApacheLivyProviderAsyncPolicy);
        }

        public async Task<HttpResponseMessage> PostRequestAsync(string resource, HttpContent postContent)
        {
            var pollyResult = await _asyncProviderPolicy_WithTimeout.ExecuteAndCaptureAsync(async () =>
            {
                var x =  await _httpClient.PostAsync(_baseUrl + $"/{resource}", postContent).ConfigureAwait(false);

                //if status is not OK, then exception will be thrown, triggering retry policy.
                x.EnsureSuccessStatusCode();

                return x;

            }).ConfigureAwait(false);

            //result is wrapped in PollyResult object, therefore, return Results within the wrapper object
            HttpResponseMessage response = pollyResult.Result;

            return response;
        }

        public async Task<HttpResponseMessage> GetRequestAsync(string resource)
        {
            var pollyResponse = await _asyncProviderPolicy.ExecuteAndCaptureAsync(async () =>
            {
                var x = await _httpClient.GetAsync(_baseUrl + resource).ConfigureAwait(false);

                //if status is not OK, then exception will be thrown, triggering retry policy.
                x.EnsureSuccessStatusCode();

                return x;

            }).ConfigureAwait(false);

            //result is wrapped in PollyResult object, therefore, return Results within the wrapper object
            HttpResponseMessage response = pollyResponse.Result;

            return response;
        }

        public async Task<HttpResponseMessage> DeleteRequestAsync(string resource)
        {
            var pollyResponse = await _asyncProviderPolicy.ExecuteAndCaptureAsync(async () =>
            {
                var x = await _httpClient.DeleteAsync(_baseUrl + resource).ConfigureAwait(false);

                //if status is not OK, then exception will be thrown, triggering retry policy.
                x.EnsureSuccessStatusCode();

                return x;

            }).ConfigureAwait(false);

            //result is wrapped in PollyResult object, therefore, return Results within the wrapper object
            HttpResponseMessage response = pollyResponse.Result;

            return response;
        }

        public void AddRequestHeaders(IDictionary<string, string> headers)
        {
            foreach(var item in headers)
            {
                _httpClient.DefaultRequestHeaders.Add(item.Key, item.Value);
            }
        }

        public void ClearAcceptHeader()
        {
            _httpClient.DefaultRequestHeaders.Accept.Clear();
        }

        public void AddMediaTypeAcceptHeader(string mediaType)
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
        }
    }
}
