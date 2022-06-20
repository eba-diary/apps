using System.ComponentModel;
using System.Runtime.Serialization;

namespace Sentry.data.Core.GlobalEnums
{

    public enum ConnectorStateEnum
    { 
        [Description("UNASSIGNED")]
        [EnumMember(Value = "UNASSIGNED")]
        Unassigned = 1,
        [Description("RUNNING")]
        [EnumMember(Value = "RUNNING")]
        Running = 2,
        [Description("PAUSED")]
        [EnumMember(Value = "PAUSED")]
        Paused = 3,
        [Description("FAILED")]
        [EnumMember(Value = "FAILED")]
        Failed = 4
    }
}