﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Models.AdminPage
{
    public class SchemaProcessActivityModel
    {
        public string SchemaName { get; set; }

        public int SchemaId { get; set; }
        
        public int DatasetId { get; set; }

        public long FileCount { get; set; }

        public DateTime LastEventTime { get; set; }
    }
}