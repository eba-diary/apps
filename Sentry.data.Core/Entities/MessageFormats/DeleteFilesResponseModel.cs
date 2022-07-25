using Sentry.Messaging.Common;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class DeleteFilesResponseModel : BaseEventMessage
    {
        public DeleteFilesResponseModel()
        {
            EventType = "FILE_DELETE_RESPONSE";
        }

        public List<DeleteFilesResponseSingleStatusModel> DeleteProcessStatusPerID { get; set; }
    }
}
