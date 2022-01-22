using Nest;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
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

        public IList<T> Search<T>(Func<SearchDescriptor<T>, ISearchRequest> selector) where T : class
        {
            return RequestFluent(selector).Documents.ToList();
        }

        public IList<T> Search<T>(SearchRequest<T> searchRequest) where T : class
        {
            return RequestInitializer(searchRequest).Documents.ToList();
        }

        public AggregateDictionary Aggregate<T>(Func<SearchDescriptor<T>, ISearchRequest> searchRequest) where T : class
        {
            return RequestFluent(searchRequest).Aggregations;
        }

        public AggregateDictionary Aggregate<T>(SearchRequest<T> searchRequest) where T : class
        {
            return RequestInitializer(searchRequest).Aggregations;
        }
        #endregion

        #region Methods
        private ISearchResponse<T> RequestFluent<T>(Func<SearchDescriptor<T>, ISearchRequest> selector) where T : class
        {
            return GetResponse(() => _client.Search(selector));
        }

        private ISearchResponse<T> RequestInitializer<T>(SearchRequest<T> searchRequest) where T : class
        {
            return GetResponse(() => _client.Search<T>(searchRequest));
        }

        private T GetResponse<T>(Func<T> request) where T : IResponse
        {
            T response = request();

            if (!response.IsValid)
            {
                throw response.OriginalException;
            }

            return response;
        }
        #endregion
    }
}
