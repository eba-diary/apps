using System;

namespace Sentry.data.Core
{
    public class DatasetFile
    {
        public DatasetFile()
        {
        }

        public virtual int DatasetFileId { get; set; }

        public virtual string FileName { get; set; }

        public virtual Dataset Dataset { get; set; }
        public virtual SchemaRevision SchemaRevision { get; set; }

        public virtual FileSchema Schema { get; set; }

        public virtual DatasetFileConfig DatasetFileConfig { get; set; }

        public virtual string UploadUserName { get; set; }

        public virtual DateTime CreateDTM { get; set; }

        public virtual DateTime ModifiedDTM { get; set; }

        public virtual string FileLocation { get; set; }

        public virtual int? ParentDatasetFileId { get; set; }

        public virtual string VersionId { get; set; }

        public virtual Boolean IsBundled { get; set; }

        public virtual string Information { get; set; }

        public virtual long Size { get; set; }

        public virtual string FlowExecutionGuid { get; set; }

        public virtual string RunInstanceGuid { get; set; }
        public virtual string FileExtension { get; set; }
    }

}
