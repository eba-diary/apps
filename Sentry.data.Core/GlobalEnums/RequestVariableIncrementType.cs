using System.ComponentModel;

namespace Sentry.data.Core
{
    public enum RequestVariableIncrementType
    {
        [Description("Select How To Increment Variable")]
        None = 0,
        [Description("By 1 Day - Exclude Today")]
        DailyExcludeToday = 1
    }
}
