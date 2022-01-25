using Nest;
using Sentry.data.Core;
using System;
using System.Linq;

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

        public ElasticResult<T> Search<T>(Func<SearchDescriptor<T>, ISearchRequest> selector) where T : class
        {
            return ResultFrom(GetResponse(() => _client.Search(selector)));
        }

        public ElasticResult<T> Search<T>(SearchRequest<T> searchRequest) where T : class
        {
            return ResultFrom(GetResponse(() => _client.Search<T>(searchRequest)));
        }
        #endregion

        #region Methods
        private T GetResponse<T>(Func<T> request) where T : IResponse
        {
            T response = request();

            if (!response.IsValid)
            {
                throw response.OriginalException;
            }

            return response;
        }

        private ElasticResult<T> ResultFrom<T>(ISearchResponse<T> response) where T : class
        {
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
