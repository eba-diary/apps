using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    [Serializable]
    public class RetrieverJobOptions
    {
        private string _compressionOptions;
        private string _httpsOptions;

        public Boolean OverwriteDataFile { get; set; }
        public string TargetFileName { get; set; }
        public Boolean CreateCurrentFile { get; set; }
        public Boolean IsRegexSearch { get; set; }
        public string SearchCriteria { get; set; }
        public int TargetDataFlow { get; set; }
        public FtpPattern FtpPattern { get; set; }

        public virtual Compression CompressionOptions
        {
            get
            {
                if (String.IsNullOrEmpty(_compressionOptions))
                {
                    return null;
                }
                else
                {
                    Compression a = JsonConvert.DeserializeObject<Compression>(_compressionOptions);
                    return a;
                }
            }
            set
            {
                _compressionOptions = JsonConvert.SerializeObject(value);
            }
        }

        public virtual JavaOptions JavaAppOptions { get; set; }
        public virtual HttpsOptions HttpOptions
        {
            get
            {
                if (string.IsNullOrEmpty(_httpsOptions))
                {
                    return null;
                }
                else
                {
                    HttpsOptions a = JsonConvert.DeserializeObject<HttpsOptions>(_httpsOptions);
                    return a;
                }
            }
            set
            {
                _httpsOptions = JsonConvert.SerializeObject(value);
            }
        }

        //All options pertaining to compression\decompression logic
        [Serializable]
        public class Compression
        {
            public Boolean IsCompressed { get; set; }
            public string CompressionType { get; set; }
            public List<string> FileNameExclusionList { get; set; }
        }

        [Serializable]
        public class JavaOptions
        {
            public string[] Arguments { get; set; }
            public string ConfigurationParameters { get; set; }
            public string DriverMemory { get; set; }
            public int? DriverCores { get; set; }
            public string ExecutorMemory { get; set; }
            public int? ExecutorCores { get; set; }
            public int? NumExecutors { get; set; }
        }

        [Serializable]
        public class HttpsOptions
        {
            public string Body { get; set; }
            public HttpMethods RequestMethod { get; set; }
            public HttpDataFormat RequestDataFormat { get; set; }
            public PagingType PagingType { get; set; }
            public string PageParameterName { get; set; }
        }
    }
}
