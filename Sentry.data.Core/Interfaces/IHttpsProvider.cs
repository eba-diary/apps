using System.Collections.Generic;
using System.IO;
using RestSharp;

namespace Sentry.data.Core
{
    public interface IHttpsProvider
    {
        void ConfigureProvider(RetrieverJob job, List<KeyValuePair<string, string>> headers);
        IRestResponse SendRequest();
        void CopyToStream(Stream targetStream);
    }
}
