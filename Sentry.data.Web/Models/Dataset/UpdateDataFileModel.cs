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