using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class ConfluentConnectorTaskDTO
    {
        public int id { get; set; }
        public ConnectorStateEnum state { get; set; }
        public string worker_id { get; set; }
    }
}