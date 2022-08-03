using System.ComponentModel;

namespace Sentry.data.Core
{
    public enum AuditType
    {
        [Description("Find All Non-Parquet Files")]
        NonParquetFiles = 0,
        [Description("Compare Rawquery and Rarquet Row Count")]
        RowCountCompare = 1
    }
}
