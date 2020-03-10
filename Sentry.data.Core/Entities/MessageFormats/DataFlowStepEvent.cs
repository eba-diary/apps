using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.Messaging.Common;

namespace Sentry.data.Core
{
    public class DataFlowStepEvent : BaseEventMessage
    {
        public int DataFlowId { get; set; }
        public string DataFlowGuid { get; set; }
        public string FlowExecutionGuid { get; set; }
        public string RunInstanceGuid { get; set; }
        public int StepId { get; set; }
        public int ActionId { get; set; }
        public string ActionGuid { get; set; }
        public string SourceBucket { get; set; }
        public string SourceKey { get; set; }
        public string StepTargetPrefix { get; set; }
        public string StepTargetBucket { get; set; }
        public List<DataFlowStepEventTarget> DownstreamTargets { get; set; }
        public string FileSize { get; set; }
        public string S3EventTime { get; set; }
        public string OriginalS3Event { get; set; }
        public int SchemaId { get; set; }
    }

    public class DataFlowStepEventTarget
    {
        public string BucketName { get; set; }
        public string ObjectKey { get; set; }
    }
}
