using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class ConfluentConnectorStatus
    {
        public string Name { get; set; }
        public List<ConfluentConnectorStatusTask> Tasks { get; set; }
    }
}
