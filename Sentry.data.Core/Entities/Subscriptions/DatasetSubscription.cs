using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DatasetSubscription : Subscription
    {
        public DatasetSubscription()
        {

        }

        public DatasetSubscription(Dataset ds, EventType et, Interval _interval, string _sentryOwnerName)
        {
            this.Dataset = ds;

            this.EventType = et;
            this.Interval = _interval;
            this.SentryOwnerName = _sentryOwnerName;
        }
        public virtual Dataset Dataset { get; set; }
    }
}
