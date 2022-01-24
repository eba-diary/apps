using Nest;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IElasticContext
    {
        void Index<T>(T document) where T : class;
        IList<T> Search<T>(Func<SearchDescriptor<T>, ISearchRequest> selector) where T : class;
        IList<T> Search<T>(SearchRequest<T> searchRequest) where T : class;
        AggregateDictionary Aggregate<T>(Func<SearchDescriptor<T>, ISearchRequest> selector) where T : class;
        AggregateDictionary Aggregate<T>(SearchRequest<T> searchRequest) where T : class;
    }
}
