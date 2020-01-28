﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public class SubscriptionModel
    {
        public EventTypeGroup group { get; set; }                        //this will identify whether we are dealing with either DATASET=1 or BUSINESSAREA=2 EventTypes

        public int datasetID { get; set; }
        
        public int businessAreaID { get; set; }

        public string SentryOwnerName { get; set; }

        public List<DatasetSubscription> CurrentSubscriptions { get; set; }
       
        public List<BusinessAreaSubscription> CurrentSubscriptionsBusinessArea { get; set; }

        public IEnumerable<SelectListItem> AllEventTypes { get; set; }

        public IEnumerable<SelectListItem> AllIntervals { get; set; }
    }
}