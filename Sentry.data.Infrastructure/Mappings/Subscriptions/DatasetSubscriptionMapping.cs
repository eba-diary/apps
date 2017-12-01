using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class DatasetSubscriptionMapping : ClassMapping<DatasetSubscription>
    {
        public DatasetSubscriptionMapping()
        {
            this.Table("Dataset_Subscription");

            this.Cache((c) => c.Usage(CacheUsage.ReadWrite));

            this.Id((x) => x.ID, (m) =>
            {
                m.Column("Subscription_ID");
                m.Generator(Generators.Identity);
            });

            this.Property((x) => x.SentryOwnerName, (m) => m.Column("SentryOwner_NME"));  

            this.ManyToOne(x => x.Dataset, m =>
            {
                m.Column("Dataset_ID");
                m.ForeignKey("FK_Dataset_ID");
                m.Class(typeof(Dataset));
            });

            this.ManyToOne(x => x.EventType, m =>
            {
                m.Column("EventType_ID");
                m.ForeignKey("FK_Dataset_EventType");
                m.Class(typeof(EventType));
            });

            this.ManyToOne(x => x.Interval, m =>
            {
                m.Column("Interval_ID");
                m.ForeignKey("FK_Dataset_IntervalType");
                m.Class(typeof(Interval));
            });


        }



    }
}
