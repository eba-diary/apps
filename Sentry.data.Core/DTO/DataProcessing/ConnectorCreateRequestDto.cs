using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class ConnectorCreateRequestDto
    {
        public string name { get; set; }

        public ConnectorCreateRequestConfigDto config { get; set; }

    }
}
