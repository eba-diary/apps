using System.Web;

namespace Sentry.data.Web
{
    public class UploadDatasetFileModel
    {
        public int DatasetId { get; set; }
        public int ConfigId { get; set; }
        public string SchemaName { get; set; }
        public HttpPostedFileBase DatasetFile { get; set; }
    }
}