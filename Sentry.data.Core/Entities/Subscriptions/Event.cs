using System;

namespace Sentry.data.Core
{
    public class Event
    {
        public virtual int EventID { get; set; }
        public virtual string UserWhoStartedEvent { get; set; }
        public virtual DateTime TimeCreated { get; set; } = DateTime.Now;
        public virtual EventType EventType { get; set; }
        public virtual string EventType_Desc { get; set; }
        public virtual Status Status { get; set; }
        public virtual string Status_Desc { get; set; }
        public virtual string Reason { get; set; }
        public virtual bool IsProcessed { get; set; }
        public virtual DateTime TimeNotified { get; set; } = DateTime.Now;
        public virtual string Parent_Event { get; set; }
        public virtual int? DataAsset { get; set; }
        public virtual int? Dataset { get; set; }
        public virtual int? DataFile { get; set; }
        public virtual int? DataConfig { get; set; }
        public virtual string Line_CDE { get; set; }
        public virtual string Search { get; set; }
        public virtual Notification Notification { get; set; }
        public virtual int? SchemaId { get; set; }
        public virtual string DeleteDetail { get; set; }
    }
}
