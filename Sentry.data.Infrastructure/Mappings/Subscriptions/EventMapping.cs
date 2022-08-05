using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class EventMapping : ClassMapping<Event>
    {
        public EventMapping()
        {


            this.Table("Event");

            this.Cache((c) => c.Usage(CacheUsage.ReadWrite));

            this.Id((x) => x.EventID, (m) =>
            {
                m.Column("Event_ID");
                m.Generator(Generators.Identity);
            });

            this.Property((x) => x.Reason, (m) => m.Column("Reason"));
            this.Property((x) => x.Search, (m) => m.Column("Search"));
            this.Property((x) => x.TimeCreated, (m) => m.Column("TimeCreated"));
            this.Property((x) => x.TimeNotified, (m) => m.Column("TimeNotified"));
            this.Property((x) => x.IsProcessed, (m) => m.Column("IsProcessed"));
            this.Property((x) => x.Parent_Event, (m) => m.Column("Parent_Event_ID"));
            this.Property((x) => x.DataAsset, (m) => m.Column("DataAsset_ID"));
            this.Property((x) => x.Dataset, (m) => m.Column("Dataset_ID"));
            this.Property((x) => x.DataFile, (m) => m.Column("DataFile_ID"));
            this.Property((x) => x.DataConfig, (m) => m.Column("DataConfig_ID"));
            this.Property((x) => x.Line_CDE, (m) => m.Column("Line_CDE"));
            this.Property((x) => x.UserWhoStartedEvent, (m) => m.Column("CreatedUser"));

            this.ManyToOne(x => x.EventType, m =>
            {
                m.Column("EventType");
                m.ForeignKey("FK_EventType");
                m.Class(typeof(EventType));
            });

            this.ManyToOne(x => x.Status, m =>
            {
                m.Column("StatusType");
                m.ForeignKey("FK_StatusType");
                m.Class(typeof(Status));
            });

            this.ManyToOne(x => x.Notification, m =>
            {
                m.Column("Notification_ID");
                m.ForeignKey("FK_Notifications");
                m.Class(typeof(Notification));
            });

            this.Property((x) => x.SchemaId, (m) => m.Column("Schema_Id"));

            this.Property((x) => x.DeleteDetail);
        }
    }
}
