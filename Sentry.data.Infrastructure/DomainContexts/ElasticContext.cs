using Nest;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public void Index<T>(T document) where T : class
        {
            //Commenting out to not allow indexing to Elastic via app at this time
            //GetResponse(() => _client.IndexDocument(document));

            throw new NotImplementedException();
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

        public void DeleteMany<T>(List<T> toDelete) where T : class
        {
            _client.DeleteMany(toDelete);
        }

        public void IndexMany<T>(List<T> toIndex) where T : class
        {
            _client.IndexMany(toIndex);
        }

        public void  DeleteByQuery<T>(Func<DeleteByQueryDescriptor<T>, IDeleteByQueryRequest> query) where T : class
        {
           _client.DeleteByQuery(query);
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
