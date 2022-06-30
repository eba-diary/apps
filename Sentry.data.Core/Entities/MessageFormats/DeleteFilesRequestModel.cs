using Sentry.Messaging.Common;

namespace Sentry.data.Core
{
    public class DeleteFilesRequestModel : BaseEventMessage
    {
        public DeleteFilesRequestModel()
        {
            EventType = "FILE_DELETE_REQUEST";
        }
        public int SchemaID { get; set; }

        public string RequestGUID { get; set; }

        public int[] DatasetFileIdList { get; set; }
        
    }
}
