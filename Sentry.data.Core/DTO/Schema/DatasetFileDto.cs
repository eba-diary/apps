using Sentry.data.Core.GlobalEnums;
using System;

namespace Sentry.data.Core
{
    public class DatasetFileDto
    {
        public int DatasetFileId { get; set; }
        public string FileName { get; set; }
        public string FileKey { get; set; }
        public string FileBucket { get; set; }
        public string ETag { get; set; }
        public int Dataset { get; set; }
        public int SchemaRevision { get; set; }
        public int Schema { get; set; }
        public int DatasetFileConfig { get; set; }
        public string UploadUserName { get; set; }
        public DateTime CreateDTM { get; set; }
        public DateTime ModifiedDTM { get; set; }
        public string FileLocation { get; set; }
        public int? ParentDatasetFileId { get; set; }
        public string VersionId { get; set; }
        public string Information { get; set; }
        public long Size { get; set; }
        public string FlowExecutionGuid { get; set; }
        public string RunInstanceGuid { get; set; }
        public string FileExtension { get; set; }
        public ObjectStatusEnum ObjectStatus { get; set; }
    }
}
