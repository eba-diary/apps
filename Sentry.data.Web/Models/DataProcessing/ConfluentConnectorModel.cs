﻿using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class ConfluentConnectorModel
    {
        public ConnectorStateEnum state { get; set; }
        public string worker_id { get; set; }
    }
}
