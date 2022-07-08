using System.IO;

namespace Sentry.data.Core
{
    public class UploadDatasetFileDto
    {
        public int DatasetId { get; set; }
        public int ConfigId { get; set; }
        public string FileName { get; set; }
        public Stream FileInputStream { get; set; }
    }
}
