using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class SubscriptionDto
    {

        public EventTypeGroup group { get; set; }                        

        public int datasetID { get; set; }

        public int businessAreaID { get; set; }

        public string SentryOwnerName { get; set; }

        public List<DatasetSubscription> CurrentSubscriptions { get; set; }

        public List<BusinessAreaSubscription> CurrentSubscriptionsBusinessArea { get; set; }

    }
}
