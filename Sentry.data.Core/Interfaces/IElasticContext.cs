using Nest;
using System;

namespace Sentry.data.Core
{
    public interface IElasticContext
    {
        void Index<T>(T document) where T : class;
        ElasticResult<T> Search<T>(Func<SearchDescriptor<T>, ISearchRequest> selector) where T : class;
        ElasticResult<T> Search<T>(SearchRequest<T> searchRequest) where T : class;
    }
}
