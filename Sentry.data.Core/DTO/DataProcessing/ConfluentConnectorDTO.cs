using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class ConfluentConnectorDTO
    {
        public ConnectorStateEnum state { get; set; }
        public string worker_id { get; set; }
    }
}
