using System.ComponentModel;

namespace Sentry.data.Core.GlobalEnums
{
    public enum NotificationSubCategoryNews
    {
        [Description(GlobalConstants.EventType.NOTIFICATION_DSC_NEWS_DSC)]
        DSC = 1,

        [Description(GlobalConstants.EventType.NOTIFICATION_DSC_NEWS_TABLEAU)]
        Tableau = 2,

        [Description(GlobalConstants.EventType.NOTIFICATION_DSC_NEWS_PYTHON)]
        Python = 3,

        [Description(GlobalConstants.EventType.NOTIFICATION_DSC_NEWS_SAS)]
        SAS = 4,

        [Description(GlobalConstants.EventType.NOTIFICATION_DSC_NEWS_ANALYTICS)]
        Analytics = 5
    }
}
