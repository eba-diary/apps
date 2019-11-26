﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.Messaging.Common;

namespace Sentry.data.Core
{
    public class HiveTableCreateModel : BaseEventMessage
    {
        public HiveTableCreateModel()
        {
            EventType = "HIVE-TABLE-CREATE-REQUESTED";
        }
        
        public string HiveStatus { get; set; }
        public int SchemaID { get; set; }
        
        public void UpdateStatus(HiveTableStatusEnum status)
        {
            HiveStatus = status.ToString();
        }
    }
}
