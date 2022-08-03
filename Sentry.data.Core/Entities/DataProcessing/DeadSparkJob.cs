using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class DeadSparkJob
    {
        public int SubmissionID { get; set; }
        public DateTime SubmissionCreated { get; set; }
        public string DatasetName { get; set; }
        public string SchemaName { get; set; }
        public string SourceBucketName { get; set; }
        public string SourceKey { get; set; }
        public string TargetKey { get; set; }
        public int BatchID { get; set; }
        public string State { get; set; }
        public string LivyAppID { get; set; }
        public string LivyDriverlogUrl { get; set; }
        public string LivySparkUiUrl { get; set; }
        public int DayOfMonth { get; set; }
        public int HourOfDay { get; set; }
        public string TriggerKey { get; set; }
        public string TriggerBucket { get; set; }
        public string ExecutionGuid { get; set; }
        public int DatasetID { get; set; }
        public int SchemaID { get; set; }
        public int DatasetFileID { get; set; }
        public int DataFlowID { get; set; }
        public int DataFlowStepID { get; set; }
    }
}