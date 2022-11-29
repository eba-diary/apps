using System.ComponentModel;

namespace Sentry.data.Core
{
    [EnumDisplayName("Variable Increment Type")]
    public enum RequestVariableIncrementType
    {
        [Description("None")]
        None = 0,
        [Description("Increment By 1 Day")]
        Daily = 1
    }
}
