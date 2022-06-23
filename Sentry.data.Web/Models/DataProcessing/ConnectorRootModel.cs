using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class ConnectorRootModel
    {
        public string ConnectorName { get; set; }

        public ConnectorStatusModel ConnectorStatus { get; set; }

        public ConnectorInfoModel ConnectorInfo { get; set; }
    }
}
