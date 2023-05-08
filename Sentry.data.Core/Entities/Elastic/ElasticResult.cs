using Nest;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class ElasticResult<T> where T : class
    {
        public long SearchTotal { get; set; }
        public IList<T> Documents { get; set; }
        public AggregateDictionary Aggregations { get; set; }
        public List<IHit<T>> Hits { get; set; }
    }
}
