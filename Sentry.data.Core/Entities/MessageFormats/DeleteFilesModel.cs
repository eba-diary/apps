using Sentry.Messaging.Common;

namespace Sentry.data.Core
{
    public class DeleteFilesModel : BaseEventMessage
    {
        public DeleteFilesModel()
        {
            EventType = "FILE_DELETE_REQUEST";
        }
        public int SchemaID { get; set; }

        public string RequestGUID { get; set; }

        public string[] DatasetFileIdList { get; set; }
        
    }
}
