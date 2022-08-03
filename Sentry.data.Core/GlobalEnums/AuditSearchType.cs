using System.ComponentModel;

namespace Sentry.data.Core
{
    public enum AuditSearchType
    {
        [Description("Search Based On Date")]
        dataSelect = 0,
        [Description("Search For Specific File")]
        fileName = 1,
        [Description("Return All Files")]
        allFiles = 2
    }
}
