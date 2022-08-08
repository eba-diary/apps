using System.ComponentModel;

namespace Sentry.data.Core.GlobalEnums
{

    public enum ObjectStatusEnum
    {
        //Any adjustements to this enum need to be reflected
        //  within Sentry.data.Core\Scripts\Post-Deploy\StaticData\ObjectStatus.sql
        [Description("Active")]
        Active = 1,
        [Description("Pending Delete")]
        Pending_Delete = 2,
        [Description("Deleted")]
        Deleted = 3,
        [Description("Disabled")]
        Disabled = 4,
        [Description("Pending Delete Failure")]
        Pending_Delete_Failure = 5
    }
}
