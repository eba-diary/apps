using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class BusinessAreaSubscriptionMapping : ClassMapping<BusinessAreaSubscription>
    {
        public BusinessAreaSubscriptionMapping()
        {
            this.Table("BusinessArea_Subscription");

            this.Cache((c) => c.Usage(CacheUsage.ReadWrite));

            this.Id((x) => x.ID, (m) =>
            {
                m.Column("Subscription_ID");
                m.Generator(Generators.Identity);
            });

            this.Property((x) => x.SentryOwnerName, (m) => m.Column("SentryOwner_NME"));

            //NOTE!! BusinessArea_ID in the table actually matches to the BusinessAreaType enum (if table shows 1 personal lines then that matches the BusinessAreaType Enum)
            //always need to remember that when adding a new BusinessArea, need to modify the enum as well
            //this seems some what strange but for now assume that thsi is the way too move forward
            this.Property(x => x.BusinessAreaType, m => m.Column("BusinessArea_ID"));

            this.ManyToOne(x => x.EventType, m =>
            {
                m.Column("EventType_ID");
                m.ForeignKey("FK_BusinessArea_EventType");
                m.Class(typeof(EventType));
            });

            this.ManyToOne(x => x.Interval, m =>
            {
                m.Column("Interval_ID");
                m.ForeignKey("FK_BusinessArea_IntervalType");
                m.Class(typeof(Interval));
            });


        }



    }
}
