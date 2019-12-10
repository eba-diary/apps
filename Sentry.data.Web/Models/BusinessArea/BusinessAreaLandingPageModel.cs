using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class BusinessAreaLandingPageModel
    {
        public List<BusinessAreaTileRowModel> Rows { get; set; }
        public bool HasActiveNotification { get; set; }
        public SystemNotificationModel Notifications { get; set; }
    }
}