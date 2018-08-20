using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core.Entities.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class HiveTableMapping : ClassMapping<HiveTable>
    {
        public HiveTableMapping()
        {
            this.Table("Hive_Table");

            this.Cache((c) => c.Usage(CacheUsage.ReadWrite));

            this.Id((x) => x.Hive_ID, (m) =>
            {
                m.Column("Hive_ID");
                m.Generator(Generators.Identity);
            });

            this.Property((x) => x.Hive_NME, (m) => m.Column("Hive_NME"));
            this.Property((x) => x.Hive_DSC, (m) => m.Column("Hive_DSC"));
            this.Property((x) => x.HiveDatabase_NME, (m) => m.Column("HiveDatabase_NME"));
            this.Property((x) => x.IsPrimary, (m) => m.Column("IsPrimary"));
            this.Property((x) => x.Created_DTM, (m) => m.Column("Created_DTM"));
            this.Property((x) => x.Changed_DTM, (m) => m.Column("Changed_DTM"));

            this.ManyToOne(x => x.Schema, m =>
            {
                m.Column("Schema_ID");
                m.ForeignKey("FK_HiveTables_Schema");
                m.Cascade(Cascade.All);
                m.Class(typeof(Schema));
            });
        }
    }
}
