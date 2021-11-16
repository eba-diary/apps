using System;
using System.Collections.Generic;
using Sentry.data.Core.GlobalEnums;

namespace Sentry.data.Core
{
    public class NotificationDto
    {
        public NotificationDto() { }



        public int NotificationId { get; set; }

        public NotificationSeverity MessageSeverity { get; set; }
        public string MessageSeverityDescription { get; set; }
        public DateTime ExpirationTime { get; set; }
        public DateTime StartTime { get; set; }
        public string CreateUser { get; set; }
        public string Message { get; set; }
        public string ObjectId { get; set; }
        public string ObjectName { get; set; }
        public Boolean IsActive { get; set; }
        public bool CanEdit { get; set; }
        public string NotificationType { get; set; }
        public string Title { get; set; }

        public List<DataAsset> AllDataAssets { get; set; } = new List<DataAsset>();
        public List<BusinessArea> AllBusinessAreas { get; set; } = new List<BusinessArea>();
        public NotificationCategory NotificationCategory { get; set; }



    }
}
