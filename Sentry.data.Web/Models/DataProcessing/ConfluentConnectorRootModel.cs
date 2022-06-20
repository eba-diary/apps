using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class ConfluentConnectorRootModel
    {
        public string name { get; set; }
        public ConfluentConnectorModel connector { get; set; }
        public List<ConfluentConnectorTaskModel> tasks { get; set; }
        public string type { get; set; }
    }
}