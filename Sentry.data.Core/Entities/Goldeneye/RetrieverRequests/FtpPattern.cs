using System.ComponentModel;

namespace Sentry.data.Core
{
    public enum FtpPattern
    {
        none = -1,
        [Description("No Pattern")]
        NoPattern = 0,
        //[Description("Specific File")]
        //SpecificFileNoDelete = 1,
        [Description("Specific File(s)")]
        RegexFileNoDelete = 2,
        //[Description("Specific File ")]
        //SpecificFileArchive = 3,
        [Description("Specific File(s) Since Last Execution")]
        RegexFileSinceLastExecution = 4,
        [Description("All File(s) Since Last Execution")]
        NewFilesSinceLastexecution = 5
    }
}
