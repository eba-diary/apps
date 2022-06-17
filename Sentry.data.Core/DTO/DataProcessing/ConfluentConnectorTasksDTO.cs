using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class ConfluentConnectorTaskDTO
    {
        public int Id { get; set; }
        public ConnectorState State { get; set; }
    }
}