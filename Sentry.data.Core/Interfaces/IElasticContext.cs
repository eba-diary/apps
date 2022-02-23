using Nest;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IElasticContext
    {
        void Index<T>(T document) where T : class;
        Task<ElasticResult<T>> SearchAsync<T>(Func<SearchDescriptor<T>, ISearchRequest> selector) where T : class;
        Task<ElasticResult<T>> SearchAsync<T>(SearchRequest<T> searchRequest) where T : class;
        Task<bool> Update<T>(T document) where T : class;
        BulkResponse DeleteMany<T>(List<T> toDelete) where T : class;
        DeleteByQueryResponse DeleteByQuery<T>(Func<DeleteByQueryDescriptor<T>, IDeleteByQueryRequest> query) where T : class;
        BulkResponse IndexMany<T>(List<T> toIndex) where T : class;
    }
}
