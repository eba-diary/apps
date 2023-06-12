using Sentry.Messaging.Common;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class DeleteFilesResponseSingleStatusModel : BaseEventMessage
    {
        public int DatasetFileId { get; set; }
        public string DatasetFileDropIdDeleteStatus { get; set; }
        public List<DeleteFilesResponseDeletedFileModel> DeletedFiles { get; set; }
    }
}
