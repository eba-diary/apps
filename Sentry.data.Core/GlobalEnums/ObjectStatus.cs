using System.ComponentModel;

namespace Sentry.data.Core.GlobalEnums
{
    public enum ObjectStatusEnum
    {
        [Description("Active")]
        Active = 1,
        [Description("Pending Delete")]
        Pending_Delete = 2,
        [Description("Deleted")]
        Deleted = 3
    }
}
