using System;

namespace Sentry.data.Core
{
    public class Image
    {
        public virtual int ImageId { get; set; }
        public virtual Dataset ParentDataset { get; set; }
        public virtual string ContentType { get; set; }
        public virtual string FileExtension { get; set; }
        public virtual string FileName { get; set; }
        public virtual string StorageBucketName { get; set; }
        public virtual string StoragePrefix { get; set; }
        public virtual string StorageKey { get; set; }
        public virtual DateTime UploadDate { get; set; }
        public virtual int Sort { get; set; }
    }
}