using System.ComponentModel;

namespace Sentry.data.Core.GlobalEnums
{
    public enum IngestionType
    {
        //[Description("User Push")]
        //User_Push = 1,

        [Description("DSC Pull")]
        DSC_Pull = 2,

        [Description("DFS Drop")]
        DFS_Drop = 3,

        [Description("S3 Drop")]
        S3_Drop = 4,

        [Description("Topic")]
        Topic = 5
    }
}
