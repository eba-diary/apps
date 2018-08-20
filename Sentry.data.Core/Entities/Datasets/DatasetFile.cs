using Sentry.Core;
using Sentry.data.Core.Entities.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public virtual Schema Schema { get; set; }

        public virtual DatasetFileConfig DatasetFileConfig { get; set; }

        public virtual string UploadUserName { get; set; }

        public virtual DateTime CreateDTM { get; set; }

        public virtual DateTime ModifiedDTM { get; set; }

        public virtual string FileLocation { get; set; }

        public virtual int? ParentDatasetFileId { get; set; }

        public virtual string VersionId { get; set; }

        public virtual Boolean IsBundled { get; set; }

        public virtual Boolean IsUsable { get; set; }

        public virtual string Information { get; set; }

        public virtual long Size { get; set; }
    }

}
