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

            this.Property((x) => x.SecurableEntityName, (m) => m.Column("SecurableEntity_NME"));
            this.Property((x) => x.CreatedDate, (m) => m.Column("Created_DTM"));
            this.Property((x) => x.EnabledDate, (m) => m.Column("Enabled_DTM"));
            this.Property((x) => x.RemovedDate, (m) => m.Column("Removed_DTM"));
            this.Property((x) => x.UpdatedById, (m) => m.Column("UpdatedBy_ID"));
            this.Property((x) => x.CreatedById, (m) => m.Column("CreatedBy_ID"));

            this.Set(x => x.Tickets, (m) =>
            {
                m.Inverse(true);
                m.Table("SecurityTicket");
                m.Cascade(Cascade.All);
                m.Key((k) =>
                {
                    k.Column("Security_ID");
                    k.ForeignKey("FK_SecurityTicket_Security");
                });
            }, map => map.OneToMany(a => a.Class(typeof(SecurityTicket))));

        }

    }
}
