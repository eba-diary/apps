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
    }
}