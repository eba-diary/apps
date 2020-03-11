using System.ComponentModel;

namespace Sentry.data.Core.GlobalEnums
{
    public enum NotificationSeverity
    {
        [Description("Critical")]
        Critical = 1,

        [Description("Warning")]
        Warning = 2,

        [Description("Info")]
        Info = 3
    }
}
