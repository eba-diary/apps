using System.ComponentModel;

namespace Sentry.data.Core.GlobalEnums
{
    public enum DatasetFileStatusType
    {
        [Description("All Files")]
        AllFiles = 0,
        [Description("Active Files")]
        ActiveFiles = 1,
        [Description("Non Deleted Files")]
        NonDeletedFiles = 2,
    }
}
