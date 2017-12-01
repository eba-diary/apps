using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class DataAssetSubscriptionMapping : ClassMapping<DataAssetSubscription>
    {
        public DataAssetSubscriptionMapping()
        {
            this.Table("DataAsset_Subscription");

            this.Cache((c) => c.Usage(CacheUsage.ReadWrite));

            this.Id((x) => x.ID, (m) =>
            {
                m.Column("Subscription_ID");
                m.Generator(Generators.Identity);
            });

            this.Property((x) => x.SentryOwnerName, (m) => m.Column("SentryOwner_NME"));

            this.ManyToOne(x => x.DataAsset, m =>
            {
                m.Column("DataAsset_ID");
                m.ForeignKey("FK_DataAsset_ID");
                m.Class(typeof(DataAsset));
            });

            this.ManyToOne(x => x.EventType, m =>
            {
                m.Column("EventType_ID");
                m.ForeignKey("FK_DataAsset_EventType");
                m.Class(typeof(EventType));
            });

            this.ManyToOne(x => x.Interval, m =>
            {
                m.Column("Interval_ID");
                m.ForeignKey("FK_DataAsset_IntervalType");
                m.Class(typeof(Interval));
            });


        }



    }
}

