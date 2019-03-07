using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class ImageDto
    {
        public int ImageId { get; set; }
        public int DatasetId { get; set; }
        public string ContentType { get; set; }
        public string FileExtension { get; set; }
        public string FileName { get; set; }
        public int FileLength { get; set; }
        public string StorageBucketName { get; set; }
        public virtual string StoragePrefix { get; set; }
        public string StorageKey { get; set; }
        public string StorageETag { get; set; }
        public DateTime UploadDate { get; set; }
        public byte[] Data { get; set; }
    }
}
