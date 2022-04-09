using System.ComponentModel;

namespace Sentry.data.Core.GlobalEnums
{
    public enum IntervalType
    {
        [Description("Instant")]
        Instant = 1,

        [Description("Hourly")]
        Hourly = 2,

        [Description("Daily")]
        Daily = 3,

        [Description("Weekly")]
        Weekly = 4,

        [Description("Never")]
        Never = 5,
    }
}
