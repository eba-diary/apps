using System.ComponentModel;

namespace Sentry.data.Core.GlobalEnums
{

    public enum ConnectorState
    { 
        [Description("UNASSIGNED")]
        Unassigned = 1,
        [Description("RUNNING")]
        Running = 2,
        [Description("PAUSED")]
        Paused = 3,
        [Description("FAILED")]
        Failed = 4
    }
}