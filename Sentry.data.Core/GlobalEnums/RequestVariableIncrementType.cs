using System.ComponentModel;

namespace Sentry.data.Core
{
    public enum RequestVariableIncrementType
    {
        [Description("Select a Variable Increment Type")]
        None = 0,
        [Description("Increment By 1 Day")]
        Daily = 1
    }
}
