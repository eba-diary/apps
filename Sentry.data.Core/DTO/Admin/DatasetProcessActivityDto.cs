﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Core
{
    public class DatasetProcessActivityDto
    {
        public string DatasetName { get; set; }
        
        public int DatasetId { get; set; }

        public long FileCount { get; set; }

        public DateTime LastEventTime { get; set; }
    }
}