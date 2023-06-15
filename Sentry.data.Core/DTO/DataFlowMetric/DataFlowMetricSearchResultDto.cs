using Nest;
using System;
using System.Collections.Generic;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core
{
    public class DataFlowMetricSearchResultDto
    {
        public long SearchTotal { get; set; }
        public List<DataFlowMetric> DataFlowMetricResults { get; set; } = new List<DataFlowMetric>();
        public List<DataFlowMetricSearchAggregateDto> TermAggregates { get; set; }
    }
}