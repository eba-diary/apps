using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class EventType
    {
        public EventType()
        {
            //default any new event type display indicator to true
            Display = true;
        }

        public virtual int Type_ID { get; set; }

        public virtual string Description { get; set; }

        public virtual int Severity { get; set; }

        public virtual Boolean Display { get; set; }                            //INDICATE WHETHER TO EXPOSE EVENTTYPE TO SpamFactory

        public virtual string Group { get; set; }

        public virtual string DisplayName { get; set; }                         //USED TO DISPLAY IN SUBSCRIPTION UI

        public virtual string ParentDescription { get; set; }                   //INDICATE WHAT PARENT THIS EVENT BELONGS TO

        public virtual List<EventType> ChildEventTypes { get; set; }            //LIST OF CHILD EVENTTYPES

    }
}
