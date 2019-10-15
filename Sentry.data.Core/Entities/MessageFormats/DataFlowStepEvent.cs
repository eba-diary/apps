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
        public int ActionId { get; set; }
        public string ActionGuid { get; set; }
        public string SourceBucket { get; set; }
        public string SourceKey { get; set; }
        public string TargetPrefix { get; set; }
        public string TargetBucket { get; set; }
    }
}
