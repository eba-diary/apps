using Sentry.data.Core.Entities.DataProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Core
{ 
    public class DatasetFileProcessActivityDto
    {
        public string FileName { get; set; }
        public string FlowExecutionGuid { get; set; }
        public int LastFlowStep { get; set; }
        public DateTime LastEventTime { get; set; }
    }
}