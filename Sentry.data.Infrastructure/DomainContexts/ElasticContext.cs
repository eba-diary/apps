using Nest;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class ElasticContext : IElasticContext
    {
        #region Fields
        private readonly IElasticClient _client;
        #endregion

        #region Constructors
        public ElasticContext(IElasticClient client)
        {
            _client = client;
        }
        #endregion

        #region IElasticContext Implementation
        public async Task IndexAsync<T>(T document) where T : class
        {
            await _client.IndexDocumentAsync(document).ConfigureAwait(false);
        }

        public async Task<T> GetByIdAsync<T>(DocumentPath<T> id) where T : class
        {
            var response = await _client.GetAsync(id).ConfigureAwait(false);

            if (response.IsValid && response.Found)
            {
                return response.Source;
            }
            else
            {
                return null;
            }
        }

        public async Task<ElasticResult<T>> SearchAsync<T>(Func<SearchDescriptor<T>, ISearchRequest> selector) where T : class
        {
            return await GetResponse(() => _client.SearchAsync(selector)).ConfigureAwait(false);
        }

        public async Task<ElasticResult<T>> SearchAsync<T>(SearchRequest<T> searchRequest) where T : class
        {            
            return await GetResponse(() => _client.SearchAsync<T>(searchRequest)).ConfigureAwait(false);
        }

        public async Task<bool> Update<T>(T document) where T : class
        {
            IUpdateResponse<T> response = await _client.UpdateAsync(new DocumentPath<T>(document), u => u.Doc(document)).ConfigureAwait(false);
            return response.IsValid;
        }

        public async Task IndexManyAsync<T>(List<T> toIndex) where T : class
        {
            await _client.IndexManyAsync(toIndex).ConfigureAwait(false);
        }

        public async Task DeleteManyAsync<T>(IEnumerable<T> documents) where T : class
        {
            await _client.DeleteManyAsync(documents).ConfigureAwait(false);
        }

        public void DeleteByQuery<T>(Func<DeleteByQueryDescriptor<T>, IDeleteByQueryRequest> query) where T : class
        {
           _client.DeleteByQuery(query);
        }

        public async Task DeleteByIdAsync<T>(DocumentPath<T> id) where T : class
        {
            await _client.DeleteAsync(id);
        }
        #endregion

        #region Methods
        private async Task<ElasticResult<T>> GetResponse<T>(Func<Task<ISearchResponse<T>>> request) where T : class
        {
            ISearchResponse<T> response = await request().ConfigureAwait(false);

            return new ElasticResult<T>()
            {
                SearchTotal = response.Total,
                Documents = response.Documents.ToList(),
                Aggregations = response.Aggregations
            };
        }
        #endregion
    }
}
