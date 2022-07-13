using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class ConfluentConnector
    {
        public string ConnectorName { get; set; }
        public List<ConfluentConnectorTask> Tasks { get; set; }
    }
}
