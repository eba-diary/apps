using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class ConnectorStatusDto
    {
        public string Name { get; set; }

        public ConnectorStateEnum State { get; set; }

        public string WorkerId { get; set; }

        public string Type  { get; set; }

        public List<ConnectorTaskDto> ConnectorTasks { get; set; }
    }
}
