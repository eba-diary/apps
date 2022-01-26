using Nest;
using Sentry.data.Core;
using System;
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
        #endregion

        #region Methods
        private async Task<ElasticResult<T>> GetResponse<T>(Func<Task<ISearchResponse<T>>> request) where T : class
        {
            ISearchResponse<T> response = await request().ConfigureAwait(false);

            if (!response.IsValid)
            {
                throw response.OriginalException;
            }

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
