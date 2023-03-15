using Sentry.Messaging.Common;

namespace Sentry.data.Core
{
    public class ReprocessFilesRequestModel : BaseEventMessage
    {
        public ReprocessFilesRequestModel()
        {
            EventType = "FILE_REPROCESS_REQUEST";
        }
        public int SchemaID { get; set; }

        public string RequestGUID { get; set; } = System.DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");

        public int[] DatasetFileIdList { get; set; }
        public int[] DatasetFileDropIdList { get; set; }
    }
}
