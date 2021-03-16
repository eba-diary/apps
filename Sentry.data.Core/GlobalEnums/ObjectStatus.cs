using System.ComponentModel;

namespace Sentry.data.Core.GlobalEnums
{
    public enum ObjectStatusEnum
    {
        [Description("Active")]
        Active = 0,
        [Description("Pending Delete")]
        Pending_Delete = 1,
        [Description("Deleted")]
        Deleted = 2
    }
}
