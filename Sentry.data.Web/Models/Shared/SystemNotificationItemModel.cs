using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class SystemNotificationItemModel
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string NotificationDate { get; set; }
        public string MessageSeverity { get; set; }
    }
}