using RestSharp;
using System.IO;

namespace Sentry.data.Core
{
    public interface IBaseHttpsProvider
    {
        void ConfigureProvider(RetrieverJob job);
        RestResponse SendRequest();
    }
}
