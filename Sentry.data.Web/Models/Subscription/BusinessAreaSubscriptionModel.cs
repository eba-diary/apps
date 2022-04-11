﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public class BusinessAreaSubscriptionModel : SubscriptionModel
    {

        public BusinessAreaSubscriptionModel() { }

        public BusinessAreaSubscriptionModel(BusinessAreaType bat, EventType et, Interval _interval, string _sentryOwnerName)
        {
            this.BusinessAreaType = bat;
            this.EventType = et;
            this.Interval = _interval;
            this.SentryOwnerName = _sentryOwnerName;
        }

        public virtual EventType EventType { get; set; }
        public virtual Interval Interval { get; set; }


        public virtual BusinessAreaType BusinessAreaType { get; set; }


        public virtual List<BusinessAreaSubscriptionModel> Children { get; set; }

        public virtual IEnumerable<int> ChildrenSelections { get; set; }


        public virtual IEnumerable<SelectListItem> AllChildrenSelections 
        { 
            get { return GetAllChildrenSelections(); } 
        }

        public virtual IEnumerable<SelectListItem> GetAllChildrenSelections()
        {
            //SelectListItem bogusItem = new SelectListItem() { Text = EventType.DisplayName, Value = EventType.Type_ID.ToString(), Disabled = true };
            //List<SelectListItem> items = new List<SelectListItem>() { bogusItem };
            //List<SelectListItem> items2 = ChildBusinessAreaSubscriptions.Select(c => new SelectListItem { Text = c.EventType.DisplayName, Value = c.EventType.Type_ID.ToString() }).ToList();
            //items.AddRange(items2);

            IEnumerable<SelectListItem> real = Children.Select(c => new SelectListItem { Text = c.EventType.DisplayName, Value = c.EventType.Type_ID.ToString()});

                      

            return real;
        }

    }
}