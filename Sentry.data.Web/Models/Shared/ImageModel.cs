using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class ImageModel
    {
        public int ImageId { get; set; }
        public string StorageKey { get; set; }
        public int sortOrder { get; set; }
        public bool deleteImage { get; set; }
        public HttpPostedFileBase ImageFileData { get; set; }
        public string ContentType { get; set; }
        public string FileExtension { get; set; }
        public string FileName { get; set; }
        public string StorageBucketName { get; set; }
        public virtual string StoragePrefix { get; set; }
    }
}