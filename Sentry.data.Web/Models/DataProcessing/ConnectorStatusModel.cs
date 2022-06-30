using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class ConnectorStatusModel
    {
        public string Name { get; set; }
        public ConnectorStateEnum State { get; set; }
        public string WorkerId { get; set; }
        public string Type  { get; set; }
        public List<ConnectorTaskModel> ConnectorTasks { get; set; }
    }
}
