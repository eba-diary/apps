using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DataFileFlowMetricsDto
    {
        public int DatasetFileId { get; set; }
        public string FileName { get; set; }
        public DateTime FirstEventTime { get; set; }
        public DateTime LastEventTime { get; set; }
        public TimeSpan Duration { get; set; }
        public List<DataFlowMetricDto> FlowEvents { get; set; }
        public bool AllEventsPresent { get; set; }
        public bool AllEventsComplete { get; set; }
        public string TargetCode { get; set; }
        public DataFileFlowMetricsDto()
        {
            AllEventsComplete = true;
            AllEventsPresent = false;
            FlowEvents = new List<DataFlowMetricDto>();
        }
    }
}
