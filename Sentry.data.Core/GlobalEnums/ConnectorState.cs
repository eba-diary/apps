﻿using System.ComponentModel;
using System.Runtime.Serialization;

namespace Sentry.data.Core.GlobalEnums
{

    public enum ConnectorState
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
        FAILED = 4,
        [Description("DEGRADED")]
        [EnumMember(Value = "DEGRADED")]
        DEGRADED = 5
    }
}