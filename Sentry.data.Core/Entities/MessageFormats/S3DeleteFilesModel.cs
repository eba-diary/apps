using Sentry.Messaging.Common;

namespace Sentry.data.Core
{
    public class S3DeleteFilesModel : BaseEventMessage
    {
        public S3DeleteFilesModel()
        {
            EventType = "FILE_DELETE_REQUEST";
        }
        public int SchemaID { get; set; }

        public string RequestGUID { get; set; }

        public string[] DatasetFileIdList { get; set; }
        
    }
}
