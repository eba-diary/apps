using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.DTO.Job
{
    public class JavaOptionsOverrideDto
    {
        public string[] Arguments { get; set; }
        public string ConfigurationParameters { get; set; }
        public string DriverMemory { get; set; }
        public int? DriverCores { get; set; }
        public string ExecutorMemory { get; set; }
        public int? ExecutorCores { get; set; }
        public int? NumExecutors { get; set; }
        public string FlowExecutionGuid { get; set; }
        public string RunInstanceGuid { get; set; }
        public string ClusterUrl { get; set; }
    }
}
