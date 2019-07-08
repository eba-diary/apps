using System.Collections.Generic;
using System.IO;
using RestSharp;

namespace Sentry.data.Core
{
    public interface IBaseHttpsProvider
    {
        void ConfigureProvider(RetrieverJob job);
        IRestResponse SendRequest();
        void CopyToStream(Stream targetStream);
    }
}
