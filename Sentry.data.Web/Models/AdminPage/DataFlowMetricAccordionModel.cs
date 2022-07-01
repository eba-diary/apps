using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public class DataFlowMetricAccordionModel
    {
        public List<string> FileNames { get; set; }
        public List<DateTime> FirstEventTimes { get; set; }
        public List<DateTime> LastEventTimes { get; set; }
        public List<TimeSpan> Durations { get; set; }
        public List<List<DataFlowMetricDto>> FlowEventGroups { get; set; }
        public DataFlowMetricAccordionModel()
        {
            FileNames = new List<string>();
            FirstEventTimes = new List<DateTime>();
            LastEventTimes = new List<DateTime>();
            Durations = new List<TimeSpan>();
            FlowEventGroups = new List<List<DataFlowMetricDto>>();
        }
    }
}