using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class ConfluentConnectorDTO
    {
        public string Name { get; set; }
        public ConnectorState State { get; set; }
        public List<ConfluentConnectorTaskDTO> Task { get; set; }
    }
}
