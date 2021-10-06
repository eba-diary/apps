using System.ComponentModel;

namespace Sentry.data.Core.GlobalEnums
{
    public enum IngestionType
    {
        [Description("User Push")]
        User_Push = 1,
        [Description("DSC Pull")]
        DSC_Pull = 2
    }
}
