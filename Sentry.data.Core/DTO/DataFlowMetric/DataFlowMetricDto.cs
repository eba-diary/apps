using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DataFlowMetricDto
    {
        public DateTime QueryMadeDateTime { get; set; }
        public int SchemaId { get; set; }
        public JObject EventContents { get; set; }
        public int TotalFlowteps { get; set; }
        public DateTime FileModifiedDateTime { get; set; }
        public string OriginalFileName { get; set; }
        public int DatasetId { get; set; }
        public int CurrentFlowStep { get; set; }
        public int DataActionId { get; set; }
        public int DataFlowId { get; set; }
        public int Partition { get; set; }
        public int DataActionTypeId { get; set; }
        public int MessageKey { get; set; }
        public int Duration { get; set; }
        public int Offset { get; set; }
        public string DataFlowName { get; set; }
        public int DataFlowStepId { get; set; }
        public string DataFlowStepName { get; set; }
        public string FlowExecutionGuid { get; set; }
        public int FileSize { get; set; }
        public int EventMetricId { get; set; }
        public string StorageCode { get; set; }
        public DateTime FileCreatedDateTime { get; set; }
        public int RunInstanceGuid { get; set; }
        public string FileName { get; set; }
        public string SaidKeyCode { get; set; }
        public DateTime MetricGeneratedDateTime { get; set; }
        public int DatasetFileId { get; set; }
        public DateTime ProcessStartDateTime { get; set; }
        public string StatusCode { get; set; }

    }
}
