using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DataFlowMetricSearchAggregateDto
    {
        /// <summary>
        /// id or key of aggreagted object
        /// </summary>
        public int key { get; set; }

        /// <summary>
        /// number of documnets under aggreagted object
        /// </summary>
        public long docCount { get; set; }
    }
}
