using Sentry.data.Core;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class UploadDataFileModel
    {
        public int DatasetId { get; set; }
        public int ConfigId { get; set; }
        public string SchemaName { get; set; }
        public HttpPostedFileBase DataFile { get; set; }
    }
}