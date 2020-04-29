using System.ComponentModel;

namespace Sentry.data.Core.GlobalEnums
{
    public enum DaleDestination
    {
        [Description("Table")]
        Table = 0,

        [Description("Column")]
        Column = 1,

        [Description("View")]
        View = 2
    }
}
