﻿using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class ConnectorModel
    {
        public string ConnectorName { get; set; }
        public ConnectorState ConnectorState { get; set; }
    }
}
