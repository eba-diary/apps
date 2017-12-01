using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class IntervalTypeMapping : ClassMapping<Interval>
    {
        public IntervalTypeMapping()
        {
            this.Table("IntervalType");

            this.Cache((c) => c.Usage(CacheUsage.ReadWrite));

            this.Id((x) => x.Interval_ID, (m) =>
            {
                m.Column("Interval_ID");
                m.Generator(Generators.Identity);
            });

            this.Property((x) => x.Description, (m) => m.Column("Description"));
        }
    }
    
}
