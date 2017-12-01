using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class EventType
    {
        public EventType()
        {

        }

        public virtual int Type_ID { get; set; }

        public virtual string Description { get; set; }

        public virtual int Severity { get; set; }
    }
}
