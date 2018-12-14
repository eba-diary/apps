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
        private string _javaAppOptions;

        //public RetrieverJobOptions()
        //{
        //    CompressionOptions = new Compression();
        //}

        public Boolean OverwriteDataFile { get; set; }
        public string TargetFileName { get; set; }
        public Boolean CreateCurrentFile { get; set; }
        public Boolean IsRegexSearch { get; set; }
        public string SearchCriteria { get; set; }
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

        //public virtual JavaOptions JavaAppOptions
        //{
        //    get
        //    {
        //        if (String.IsNullOrEmpty(_javaAppOptions))
        //        {
        //            return null;
        //        }
        //        else
        //        {
        //            JavaOptions a = JsonConvert.DeserializeObject<JavaOptions>(_javaAppOptions);
        //            return a;
        //        }
        //    }
        //    set
        //    {
        //        _javaAppOptions = JsonConvert.SerializeObject(value);
        //    }
        //}

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
            public string Arguments { get; set; }
            public string ConfigurationParameters { get; set; }
            public string DriverMemory { get; set; }
            public int? DriverCores { get; set; }
            public string ExecutorMemory { get; set; }
            public int? ExecutorCores { get; set; }
            public int? NumExecutors { get; set; }
        }
    }
}
