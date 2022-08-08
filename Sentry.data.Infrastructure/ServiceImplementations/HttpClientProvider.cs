using Sentry.data.Core.Interfaces;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class HttpClientProvider : IHttpClientProvider
    {
        private readonly HttpClient _httpClient;

        public HttpClientProvider() { }

        public HttpClientProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public virtual Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            return _httpClient.GetAsync(requestUri);
        }
        
        public virtual Task<HttpResponseMessage> GetAsync(string requestUri, HttpCompletionOption httpCompletionOption)
        {
            return _httpClient.GetAsync(requestUri, httpCompletionOption);
        }
        public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
        {
            return _httpClient.PostAsync(requestUri, content);
        }

        public Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content)
        {
            return _httpClient.PutAsync(requestUri, content);
        }

        public Task<HttpResponseMessage> DeleteAsync(string requestUri)
        {
            return _httpClient.DeleteAsync(requestUri);
        }
    }
}
