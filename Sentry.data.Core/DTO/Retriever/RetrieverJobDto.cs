using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class RetrieverJobDto
    {
        public int JobId { get; set; }
        public string Schedule { get; set; }
        public int SchedulePicker { get; set; } 
        public string RelativeUri { get; set; }
        public string HttpRequestBody { get; set; }
        public string SearchCriteria { get; set; }
        public string TargetFileName { get; set; }
        public bool CreateCurrentFile { get; set; }
        public int DataSourceId { get; set; }
        public string DataSourceType { get; set; }
        public int FileSchema { get; set; }
        public int DatasetFileConfig { get; set; }
        public int DataFlow { get; set; }
        public HttpMethods? RequestMethod { get; set; }
        public HttpDataFormat? RequestDataFormat { get; set; }
        public FtpPattern? FtpPattern { get; set; }
        public bool IsCompressed { get; set; }
        public string CompressionType { get; set; }
        public List<string> FileNameExclusionList { get; set; }
        public string ReadableSchedule { get; set; }
        public ObjectStatusEnum ObjectStatus { get; set; }
        public string DeleteIssuer { get; set; }
        public DateTime DeleteIssueDTM { get; set; }
        public Dictionary<string, string> ExecutionParameters { get; set; }
        public PagingType PagingType { get; set; }
        public string PageTokenField { get; set; }
        public string PageParameterName { get; set; }
    }
}
