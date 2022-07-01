using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DataFileFlowMetricsDto
    {
        public string FileName { get; set; }
        public DateTime FirstEventTime { get; set; }
        public DateTime LastEventTime { get; set; }
        public TimeSpan Duration { get; set; }
        public List<DataFlowMetricDto> FlowEvents { get; set; }
        public DataFileFlowMetricsDto()
        {
            FlowEvents = new List<DataFlowMetricDto>();
        }
    }
}
