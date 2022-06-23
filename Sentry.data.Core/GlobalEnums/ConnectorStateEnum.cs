using System.ComponentModel;
using System.Runtime.Serialization;

namespace Sentry.data.Core.GlobalEnums
{

    public enum ConnectorStateEnum
    { 
        [Description("UNASSIGNED")]
        [EnumMember(Value = "UNASSIGNED")]
        UNASSIGNED = 1,
        [Description("RUNNING")]
        [EnumMember(Value = "RUNNING")]
        RUNNING = 2,
        [Description("PAUSED")]
        [EnumMember(Value = "PAUSED")]
        PAUSED = 3,
        [Description("FAILED")]
        [EnumMember(Value = "FAILED")]
        FAILED = 4
    }
}