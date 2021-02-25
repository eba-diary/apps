using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IApacheLivyProvider
    {
        Task<HttpResponseMessage> PostRequestAsync(string resource, HttpContent postContent);
        Task<HttpResponseMessage> GetRequestAsync(string resource);
        Task<HttpResponseMessage> DeleteRequestAsync(string resource);
        void AddRequestHeaders(IDictionary<string, string> headers);
        void ClearAcceptHeader();
        void AddMediaTypeAcceptHeader(string mediaType);
    }
}
