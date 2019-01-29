using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class SecurityMapping : ClassMapping<Security>
    {

        public SecurityMapping()
        {

            this.Table("Security");

            this.Cache(c => c.Usage(CacheUsage.ReadWrite));

            this.Id(x => x.SecurityId, m =>
            {
                m.Column("Security_ID");
                m.Generator(Generators.GuidComb);
            });

            this.Property((x) => x.ObjectType, (m) => m.Column("Object_TYP"));
            this.Property((x) => x.CreatedDate, (m) => m.Column("Created_DTM"));
            this.Property((x) => x.EnabledDate, (m) => m.Column("Enabled_DTM"));
            this.Property((x) => x.RemovedDate, (m) => m.Column("Removed_DTM"));
            this.Property((x) => x.UpdatedById, (m) => m.Column("UpdatedBy_ID"));

        }

    }
}
