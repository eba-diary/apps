using System.ComponentModel;

namespace Sentry.data.Core.GlobalEnums
{
    public enum DataClassificationType
    {
        [Description("None")]
        None = 0,
        //[Description("Restricted")]
        //Restricted = 1,
        [Description("Highly Sensitive")]
        HighlySensitive = 2,
        [Description("Internal Use Only")]
        InternalUseOnly = 3,
        [Description("Public")]
        Public = 4
    }
}
