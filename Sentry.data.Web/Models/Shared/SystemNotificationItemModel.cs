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

        //This property used by PL Popover to decode prior to display
        public System.Web.HtmlString MessageDecoded
        {
            get
            {
                return new System.Web.HtmlString(System.Net.WebUtility.HtmlDecode(Message));
            }
        }

        //This property used by PL Popover to decode prior to display
        public System.Web.HtmlString TitleDecoded
        {
            get
            {
                return new System.Web.HtmlString(System.Net.WebUtility.HtmlDecode(Title));
            }
        }

    }
}