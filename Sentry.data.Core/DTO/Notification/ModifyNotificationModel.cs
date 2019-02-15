using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class NotificationModel
    {
        public NotificationModel() { }



        public int NotificationId { get; set; }

        public NotificationSeverity MessageSeverity { get; set; }
        public DateTime ExpirationTime { get; set; }
        public DateTime StartTime { get; set; }
        public string CreateUser { get; set; }
        public int SeverityID { get; set; }
        public string Message { get; set; }
        public string MessageSeverityTag { get; set; }
        public int DataAssetId { get; set; }
        public Boolean IsActive { get; set; }


        public List<DataAsset> AllDataAssets { get; set; }
        public List<NotificationSeverity> AllSeverities { get; set; }


    }
}
