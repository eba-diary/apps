using System;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class EventMetric
    {

        public virtual int EventMetricsId { get; set; }
        public virtual string FlowExecutionGuid { get; set; }
        public virtual string RunInstanceGuid { get; set; }
        public virtual DataFlowStep Step { get; set; }
        public virtual string ServiceRunGuid { get; set; }
        public virtual string ProcessRunGuid { get; set; }
        public virtual int Partition { get; set; }
        public virtual int Offset { get; set; }
        public virtual string MessageKey { get; set; }
        public virtual string MessageValue { get; set; }
        public virtual string ApplicationName { get; set; }
        public virtual string MachineName { get; set; }
        public virtual string StatusCode { get; set; }
        public virtual string MetricsData { get; set; }
        public virtual DateTime CreatedDTM { get; set; }
        public virtual DataFlow DataFlow { get; set; }
    }
}
