using Nest;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IElasticContext
    {
        Task IndexAsync<T>(T document) where T : class;
        Task<T> GetDocumentAsync<T>(DocumentPath<T> id) where T : class;
        Task<ElasticResult<T>> SearchAsync<T>(Func<SearchDescriptor<T>, ISearchRequest> selector) where T : class;
        Task<ElasticResult<T>> SearchAsync<T>(SearchRequest<T> searchRequest) where T : class;
        Task<bool> Update<T>(T document) where T : class;
        void DeleteMany<T>(List<T> toDelete) where T : class;
        void DeleteByQuery<T>(Func<DeleteByQueryDescriptor<T>, IDeleteByQueryRequest> query) where T : class;
        void IndexMany<T>(List<T> toIndex) where T : class;
    }
}
