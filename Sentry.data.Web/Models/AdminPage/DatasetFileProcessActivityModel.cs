using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Models.AdminPage
{
    public class DatasetFileProcessActivityModel
    {
        public string FileName { get; set; }
        public string FlowExecutionGuid { get; set; }
        public string LastFlowStep { get; set; }
        public DateTime LastEventTime { get; set; }
    }
}