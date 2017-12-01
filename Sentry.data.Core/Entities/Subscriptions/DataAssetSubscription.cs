using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DataAssetSubscription : Subscription
    {

        public DataAssetSubscription()
        {

        }

        public DataAssetSubscription(DataAsset da, EventType et, Interval _interval, string _sentryOwnerName)
        {
            this.DataAsset = da;

            this.EventType = et;
            this.Interval = _interval;
            this.SentryOwnerName = _sentryOwnerName;
        }

        public virtual DataAsset DataAsset { get; set; }

    }
}
