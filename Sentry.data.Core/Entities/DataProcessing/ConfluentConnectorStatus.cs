using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class ConfluentConnectorStatus
    {
        public string name { get; set; }
        public ConfluentConnectorStatusConnector connector { get; set; }
        public List<ConfluentConnectorStatusTask> tasks { get; set; }
        public string type { get; set; }
    }

    public class ConfluentConnectorStatusConnector
    {
        public string state { get; set; }
        public string worker_id { get; set; }
    }

    public class ConfluentConnectorStatusTask
    {
        public int id { get; set; }
        public string state { get; set; }
        public string worker_id { get; set; }
    }
}
