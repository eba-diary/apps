using System.ComponentModel;

namespace Sentry.data.Core.GlobalEnums
{
    public enum DaleSensitive
    {
        [Description("SensitiveOnly")]
        SensitiveOnly = 0,

        [Description("SensitiveNone")]
        SensitiveNone = 1,

        [Description("SensitiveAll")]
        SensitiveAll = 2,
    }
}
