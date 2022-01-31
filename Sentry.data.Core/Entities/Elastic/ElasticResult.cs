using Nest;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class ElasticResult<T>
    {
        public long SearchTotal { get; set; }
        public IList<T> Documents { get; set; }
        public AggregateDictionary Aggregations { get; set; }
    }
}
