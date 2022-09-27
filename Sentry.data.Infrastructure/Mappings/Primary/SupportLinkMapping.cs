using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class SupportLinkMapping : ClassMapping<SupportLink>
    {
        public SupportLinkMapping()
        {
            Table("SupportLink");
            Cache((c) => c.Usage(CacheUsage.ReadWrite));

            Id(x => x.SupportLinkId, x =>
            {
                x.Column("SupportLinkId");
                x.Generator(Generators.Identity);
            });
            Property(x => x.Name, x =>
            {
                x.Column("Name");
                x.NotNullable(true);
            });
            Property(x => x.Description, x =>
            {
                x.Column("Description");
                x.NotNullable(false);
            });
            Property(x => x.Url, x =>
            {
                x.Column("Url");
                x.NotNullable(true);
            });
        }
    }
}
