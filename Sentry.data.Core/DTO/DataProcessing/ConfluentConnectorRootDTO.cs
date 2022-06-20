using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class ConfluentConnectorRootDTO
    {
        public string name { get; set; }
        public ConfluentConnectorDTO connector { get; set; }
        public ConfluentConnectorTaskDTO[] tasks { get; set; }
        public string type { get; set; }
    }
}