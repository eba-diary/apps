using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public class SubscriptionModel
    {
        public int datasetID { get; set; }

        public string SentryOwnerName { get; set; }

        public List<DatasetSubscription> CurrentSubscriptions { get; set; }

        //public List<DatasetSubscription> InactiveSubscriptions { get; set; }

        public IEnumerable<SelectListItem> AllEventTypes { get; set; }

        public IEnumerable<SelectListItem> AllIntervals { get; set; }

    }
}