using Nest;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IElasticDocumentClient
    {
        Task IndexAsync<T>(T document) where T : class;
        Task<T> GetByIdAsync<T>(DocumentPath<T> id) where T : class;
        Task<ElasticResult<T>> SearchAsync<T>(Func<SearchDescriptor<T>, ISearchRequest> selector) where T : class;
        Task<ElasticResult<T>> SearchAsync<T>(SearchRequest<T> searchRequest) where T : class;
        Task<bool> Update<T>(T document) where T : class;
        Task DeleteManyAsync<T>(List<T> documents) where T : class;
        Task DeleteByIdAsync<T>(DocumentPath<T> id) where T : class;
        void DeleteByQuery<T>(Func<DeleteByQueryDescriptor<T>, IDeleteByQueryRequest> query) where T : class;
        Task IndexManyAsync<T>(List<T> documents) where T : class;
        Task IndexManyAsync<T>(List<T> documents, string indexName) where T : class;
    }
}
