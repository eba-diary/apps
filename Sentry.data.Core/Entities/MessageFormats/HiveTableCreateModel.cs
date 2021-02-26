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
            EventType = "HIVE-TABLE-CREATE-REQUESTED";
        }
        
        public string HiveStatus { get; set; }
        public int SchemaID { get; set; }
        public int RevisionID { get; set; }
        public string InitiatorID { get; set; }
        public string ChangeIND { get; set; }
        
        public void UpdateStatus(ConsumptionLayerTableStatusEnum status)
        {
            HiveStatus = status.ToString();
        }
    }
}
