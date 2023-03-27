using System;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public abstract class BaseDatasetFileV2
    {
        public virtual string FileNME { get; set; }
        public virtual string FlowExecutionGUID { get; set; }
        public virtual string ObjectBucket { get; set; }
        public virtual string ObjectKey { get; set; }
        public virtual string ObjectVersionID { get; set; }
        public virtual string ObjectETag { get; set; }
        public virtual int ObjectSizeAMT { get; set; }
        public virtual int DatasetID { get; set; }
        public virtual int SchemaId { get; set; }
        public virtual DateTime CreateDTM { get; set; }
    }
}
