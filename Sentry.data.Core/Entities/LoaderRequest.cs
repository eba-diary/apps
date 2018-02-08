using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class LoaderRequest : Request
    {
        public string File { get; set; }
        public bool IsBundled { get; set; }
        public string RequestGuid { get; set; }
        public int DatasetID { get; set; }
        public int DatasetFileConfigId { get; set; }
        public string TargetBucket { get; set; }
        public string TargetKey { get; set; }
        public string TargetVersionId { get; set; }
        public string TargetETag { get; set; }
        public string TargetFileName { get; set; }
        public string EventID { get; set; }

        public LoaderRequest(Guid input)
        {
            RequestGuid = input.ToString();
        }
    }
}
