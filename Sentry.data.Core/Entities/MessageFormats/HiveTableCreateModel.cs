using System;
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
            EventType = "HIVE-TABLE-REQUESTED";
        }

        public SchemaModel Schema { get; set; }

        public void UpdateStatus(HiveTableStatusEnum status)
        {
            this.Schema.HiveStatus = status.ToString();
        }
    }
}
