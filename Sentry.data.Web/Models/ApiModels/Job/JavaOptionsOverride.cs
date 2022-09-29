using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Models.ApiModels.Job
{
    public class JavaOptionsOverride
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
        /// <summary>
        /// Url which job will be submitted too (Apache Livy Url for given EMR cluster)
        /// </summary>
        public string ClusterUrl { get; set; }
    }
}