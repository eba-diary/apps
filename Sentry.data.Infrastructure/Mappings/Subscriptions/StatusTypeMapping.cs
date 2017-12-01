using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class StatusTypeMapping : ClassMapping<Status>
    {

        public StatusTypeMapping()
        {

            this.Table("StatusType");

            this.Cache((c) => c.Usage(CacheUsage.ReadWrite));

            this.Id((x) => x.Status_ID, (m) =>
            {
                m.Column("Status_ID");
                m.Generator(Generators.Identity);
            });

            this.Property((x) => x.Description, (m) => m.Column("Description"));
        }
    }
}
