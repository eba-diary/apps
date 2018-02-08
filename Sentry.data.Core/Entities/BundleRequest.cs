using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class BundleRequest : Request
    {                               
        public BundleRequest(Guid input)
        {
            //RequestGuid = Guid.NewGuid().ToString("D");
            RequestGuid = input.ToString();
            SourceKeys = new List<Tuple<string, string>>();
        }
        public string TargetFileLocation { get; set; }
        public string RequestGuid { get; set; }
        public int DatasetID { get; set; }
        public int DatasetFileConfigId { get; set; }
        public string Bucket { get; set; }
        public string SourceKeysFileLocation { get; set; }
        public string SourceKeysFileVersionId { get; set; }
        public List<Tuple<string, string>> SourceKeys { get; set; }
        public string TargetFileName { get; set; }
        public string FileExtension { get; set; }
    }
}
