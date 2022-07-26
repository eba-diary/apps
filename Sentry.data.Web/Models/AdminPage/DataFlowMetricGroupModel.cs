using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public class DataFlowMetricGroupModel
    {
        public int DatasetFileId { get; set; }
        public string FileName { get; set; }
        public DateTime FirstEventTime { get; set; }
        public DateTime LastEventTime { get; set; }
        public string Duration { get; set; }
        public List<DataFlowMetricModel> FlowEvents { get; set; }
        public bool AllEventsPresent { get; set; }
        public bool AllEventsComplete { get; set; }
        public string TargetCode { get; set; }

    }
}