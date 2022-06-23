using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class ConfluentConnectorInfo
    {
        public string name { get; set; }

        public string type { get; set; }

        [JsonProperty("config")]
        public ConfluentConnectorInfoConfig confluentConnectorInfoConfig { get; set; }


    }
}
