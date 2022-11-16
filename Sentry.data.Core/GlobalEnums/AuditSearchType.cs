using System.ComponentModel;

namespace Sentry.data.Core
{
    public enum AuditSearchType
    {
        [Description("Search Based On Date")]
        dateSelect = 0,
        [Description("Search Based On File")]
        fileName = 1
    }
}
