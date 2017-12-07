using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class Event
    {
        public Event()
        {

        }

        public virtual int EventID { get; set; }
        public virtual string UserWhoStartedEvent { get; set; }
        public virtual DateTime TimeCreated { get; set; }


        public virtual EventType EventType { get; set; }

        public virtual string EventType_Desc { get; set; }

        public virtual Status Status { get; set; }

        public virtual string Status_Desc { get; set; }

        public virtual string Reason { get; set; }
        public virtual bool IsProcessed { get; set; }
        public virtual DateTime TimeNotified { get; set; }


        public virtual Event Parent_Event { get; set; }
        public virtual DataAsset DataAsset { get; set; }
        public virtual Dataset Dataset { get; set; }
        public virtual DatasetFile DataFile { get; set; }
        public virtual DatasetFileConfig DataConfig { get; set; }

    }
}
