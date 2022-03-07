using System.ComponentModel;

namespace Sentry.data.Core.GlobalEnums
{
    public enum NotificationSubCategoryReleaseNotes
    {
        [Description(GlobalConstants.EventType.NOTIFICATION_DSC_RELEASENOTES_DSC)]
        DSC = 1,

        [Description(GlobalConstants.EventType.NOTIFICATION_DSC_RELEASENOTES_CL)]
        CL = 2,

        [Description(GlobalConstants.EventType.NOTIFICATION_DSC_RELEASENOTES_PL)]
        PL = 3,

        [Description(GlobalConstants.EventType.NOTIFICATION_DSC_RELEASENOTES_LIFEANNUITY)]
        LifeAnnuity = 4,

        [Description(GlobalConstants.EventType.NOTIFICATION_DSC_RELEASENOTES_CLAIMS)]
        Claims = 5,

        [Description(GlobalConstants.EventType.NOTIFICATION_DSC_RELEASENOTES_CORPORATE)]
        Corporate = 6,
    }
}
