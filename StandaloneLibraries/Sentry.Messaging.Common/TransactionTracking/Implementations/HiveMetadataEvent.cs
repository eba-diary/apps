﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.Messaging.Common
{
    public class HiveMetadataEvent : BaseEventMessage
    {
        string SchemaId { get; set; }
    }
}
