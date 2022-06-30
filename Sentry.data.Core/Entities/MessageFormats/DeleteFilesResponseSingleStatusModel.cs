using Sentry.Messaging.Common;

namespace Sentry.data.Core
{
    public class DeleteFilesResponseSingleStatusModel : BaseEventMessage
    {
        public DeleteFilesResponseSingleStatusModel()
        {
            
        }

        public int DatasetFileId { get; set; }
        public string DatasetFileIdDeleteStatus { get; set; }

    }
}
