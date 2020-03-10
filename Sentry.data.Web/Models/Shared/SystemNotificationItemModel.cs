using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Core.GlobalEnums;

namespace Sentry.data.Web
{
    public class SystemNotificationItemModel
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string NotificationDate { get; set; }
        public string MessageSeverity { get; set; }
        public string Color => GetNotificationColor();

        public string GetNotificationColor()
        {
            string c;

            if (MessageSeverity == NotificationSeverity.Critical.ToString())
            {
                c = "red";
            }
            else if (MessageSeverity == NotificationSeverity.Warning.ToString())
            {
                c = "orange";
            }
            else
            {
                c = "blue";
            }

           return c;
        }
    }
}