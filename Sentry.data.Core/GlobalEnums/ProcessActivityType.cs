using System.ComponentModel;

namespace Sentry.data.Core
{
    public enum ProcessActivityType
    {
        [Description("Total Files")]
        TOTAL_FILES = 0,
        [Description("Failed Files")]
        FAILED_FILES = 1,
        [Description("In Flight Files")]
        IN_FLIGHT_FILES = 2

    }
}
