using System.Net.Http;
using System.Threading.Tasks;

namespace Sentry.data.Core.Interfaces
{
    public interface IHttpClientProvider
    {
        Task<HttpResponseMessage> GetAsync(string requestUri);
        Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content);
        Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content);
        Task<HttpResponseMessage> DeleteAsync(string requestUri);
    }
}
