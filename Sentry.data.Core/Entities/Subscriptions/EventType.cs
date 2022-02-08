using System;

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

        /// <summary>
        /// Indicates whether to display this event type
        /// </summary>
        public virtual Boolean Display { get; set; }

        public virtual string Group { get; set; }

        public virtual string DisplayName { get; set; }         //USED TO DISPLAY IN SUBSCRIPTION UI

    }
}
