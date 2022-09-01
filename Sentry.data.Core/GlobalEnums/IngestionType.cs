using System.ComponentModel;

namespace Sentry.data.Core.GlobalEnums
{
    public enum IngestionType
    {
        [Description("DFS Drop")]
        DFS_Drop = 1,

        [Description("DSC Pull")]
        DSC_Pull = 2,

        [Description("S3 Drop")]
        S3_Drop = 3,

        [Description("Topic")]
        Topic = 4
    }
}
