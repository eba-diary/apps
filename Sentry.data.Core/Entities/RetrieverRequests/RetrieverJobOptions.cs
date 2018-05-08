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
        //public RetrieverJobOptions()
        //{
        //    CompressionOptions = new Compression();
        //}

        public Boolean OverwriteDataFile { get; set; }
        public string TargetFileName { get; set; }
        public Boolean CreateCurrentFile { get; set; }
        public Boolean IsRegexSearch { get; set; }
        public string SearchCriteria { get; set; }
        private string _compressionOptions;

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

        //All options pertaining to compression\decompression logic
        [Serializable]
        public class Compression
        {
            public Boolean IsCompressed { get; set; }
            public string CompressionType { get; set; }
            public List<string> FileNameExclusionList { get; set; }
        }
    }
}
