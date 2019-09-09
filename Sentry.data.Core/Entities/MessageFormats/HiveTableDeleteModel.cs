using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.Messaging.Common;

namespace Sentry.data.Core
{
    public class HiveTableDeleteModel : BaseEventMessage
    {
        public HiveTableDeleteModel()
        {
            EventType = "HIVE-TABLE-DELETE-REQUESTED";
        }
        public int SchemaId { get; set; }
        public string HiveDatabase { get; set; }
        public string HiveTable { get; set; }
        public string HiveStatus { get; set; }
    }
}
