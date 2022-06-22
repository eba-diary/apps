using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class ConfluetConnectorRoot
    {
        public string ConnectorName { get; set; }

        public ConfluentConnectorStatus ConfluetConnectorStatus { get; set; }

        public ConfluentConnectorInfo ConfluentConnectorInfo { get; set; }
    }
}
