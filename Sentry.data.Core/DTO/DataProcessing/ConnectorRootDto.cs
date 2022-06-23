using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class ConnectorRootDto
    {
        public string ConnectorName { get; set; }

        public ConnectorStatusDto ConnectorStatus { get; set; }

        public ConnectorInfoDto ConnectorInfo { get; set; }
    }
}
