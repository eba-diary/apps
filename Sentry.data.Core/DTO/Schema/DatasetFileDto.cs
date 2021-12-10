using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DatasetFileDto
    {
        public int DatasetFileId { get; set; }
        public string FileName { get; set; }
        public int Dataset { get; set; }
        public int SchemaRevision { get; internal set; }
        public int Schema { get; internal set; }
        public int DatasetFileConfig { get; internal set; }
        public string UploadUserName { get; internal set; }
        public DateTime CreateDTM { get; internal set; }
        public DateTime ModifiedDTM { get; internal set; }
        public string FileLocation { get; internal set; }
        public int? ParentDatasetFileId { get; internal set; }
        public string VersionId { get; internal set; }
        public string Information { get; internal set; }
        public long Size { get; internal set; }
        public string FlowExecutionGuid { get; internal set; }
        public string RunInstanceGuid { get; internal set; }
        public string FileExtension { get; internal set; }
    }
}
