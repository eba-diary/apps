using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class Subscription
    {

        public Subscription()
        {

        }

        public virtual int ID { get; set; }
        public virtual EventType EventType { get; set; }
        public virtual Interval Interval { get; set; }
        public virtual string SentryOwnerName { get; set; }




    }
}
