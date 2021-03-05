using Sentry.data.Core;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class ApacheLivyProvider : IApacheLivyProvider
    {
        private HttpClient _httpClient;
        private readonly string _baseUrl;

        public ApacheLivyProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _baseUrl = Sentry.Configuration.Config.GetHostSetting("ApacheLivy");
        }

        public async Task<HttpResponseMessage> PostRequestAsync(string resource, HttpContent postContent)
        {
            return await _httpClient.PostAsync( _baseUrl + $"/{resource}", postContent).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> GetRequestAsync(string resource)
        {
            return await _httpClient.GetAsync(_baseUrl + resource).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> DeleteRequestAsync(string resource)
        {
            return await _httpClient.DeleteAsync(_baseUrl + resource).ConfigureAwait(false);
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
