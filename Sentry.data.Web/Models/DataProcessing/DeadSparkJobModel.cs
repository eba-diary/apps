using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class DeadSparkJobModel
    {
        public DateTime SubmissionTime { get; set; }
        public string DatasetName { get; set; }
        public string SchemaName { get; set; }
        public string SourceKey { get; set; }
        public string FlowExecutionGuid { get; set; }
        public string RunInstanceGuid { get; set; }
        public bool ReprocessingRequired { get; set; }
        public int SubmissionID { get; set; }
        public string SourceBucketName { get; set; }
        public int BatchID { get; set; }
        public string LivyAppID { get; set; }
        public string LivyDriverlogUrl { get; set; }
        public string LivySparkUiUrl { get; set; }
        public int DatasetFileID { get; set; }
        public int DatasetFileDropID { get; set; }
        public int DataFlowStepID { get; set; }
    }
}
