using System.Net.Http;

namespace Sentry.data.Core
{
    public interface IHttpClientGenerator
    {
        HttpClient GenerateHttpClient();
    }
}
