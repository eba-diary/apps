using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class SystemNotificationModel
    {
        public List<SystemNotificationItemModel> CriticalNotifications { get; set; }
        public List<SystemNotificationItemModel> StandardNotifications { get; set; }
    }
}