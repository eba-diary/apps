using System.ComponentModel;

namespace Sentry.data.Core
{
    public enum PagingType
    {
        [Description("None")]
        None = 0,
        [Description("Page Number")]
        PageNumber = 1,
        [Description("Token")]
        Token = 2
    }
}
