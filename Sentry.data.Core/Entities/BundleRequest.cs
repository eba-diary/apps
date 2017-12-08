using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class BundleRequest
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
        /// <summary>
        /// Email of orignial requestor
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Target location for reqeust response.  Typically, the dataset bundle drop location for dataset loader.
        /// </summary>
        public string DatasetDropLocation { get; set; }
        /// <summary>
        /// Associate ID of request initiator
        /// </summary>
        public string RequestInitiatorId { get; set; }
        public string EventId { get; set; }

    }
}
