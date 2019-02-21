using System;
using System.Collections.Generic;
using Sentry.data.Core.GlobalEnums;

namespace Sentry.data.Core
{
    public class NotificationModel
    {
        public NotificationModel() { }



        public int NotificationId { get; set; }

        public NotificationSeverity MessageSeverity { get; set; }
        public string MessageSeverityDescription { get; set; }
        public DateTime ExpirationTime { get; set; }
        public DateTime StartTime { get; set; }
        public string CreateUser { get; set; }
        public string Message { get; set; }
        public int DataAssetId { get; set; }
        public string DataAssetName { get; set; }
        public Boolean IsActive { get; set; }
        public bool CanEdit { get; set; }

        public List<DataAsset> AllDataAssets { get; set; } = new List<DataAsset>();

    }
}
