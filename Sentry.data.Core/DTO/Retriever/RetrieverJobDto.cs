using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class RetrieverJobDto
    {
        public string Schedule { get; set; }
        public int SchedulePicker { get; set; } 
        public string RelativeUri { get; set; }
        public string HttpRequestBody { get; set; }
        public string SearchCriteria { get; set; }
        public string TargetFileName { get; set; }
        public bool CreateCurrentFile { get; set; }
        public int DataSourceId { get; set; }
        public HttpMethods RequestMethod { get; set; }
        public HttpDataFormat RequestDataFormat { get; set; }
        public FtpPattern FtpPatrn { get; set; }
    }
}
