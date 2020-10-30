using System.ComponentModel;

namespace Sentry.data.Core.GlobalEnums
{
    public enum DaleDestiny
    {
        [Description("Object")]
        Object = 0,

        [Description("Column")]
        Column = 1,

        [Description("SAID")]
        SAID = 2,

        [Description("Server")]
        Server = 3,

        [Description("Database")]
        Database = 4
    }
}
