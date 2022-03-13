using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class BusinessAreaSubscription : Subscription
    {
        public BusinessAreaSubscription(){}

        public BusinessAreaSubscription(BusinessAreaType bat,EventType et, Interval _interval, string _sentryOwnerName)
        {
            this.BusinessAreaType = bat;
            this.EventType = et;
            this.Interval = _interval;
            this.SentryOwnerName = _sentryOwnerName;
        }
        
        public virtual BusinessAreaType BusinessAreaType { get; set; }

        public virtual List<BusinessAreaSubscription> childBusinessAreaSubscriptions { get; set; }

    }
}
