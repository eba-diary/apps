using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core.Entities.Metadata;
using NHibernate.Mapping.ByCode;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class SchemaMapping : ClassMapping<Schema>
    {
        public SchemaMapping()
        {
            this.Table("Hive_Schema");

            this.Cache((c) => c.Usage(CacheUsage.ReadWrite));

            this.Id((x) => x.Schema_ID, (m) =>
            {
                m.Column("Schema_ID");
                m.Generator(Generators.Identity);
            });

            this.Property((x) => x.Schema_NME, (m) => m.Column("Schema_NME"));
            this.Property((x) => x.Schema_DSC, (m) => m.Column("Schema_DSC"));
            this.Property((x) => x.DataObject_ID, (m) => m.Column("DataObject_ID"));
            this.Property((x) => x.Revision_ID, (m) => m.Column("Revision_ID"));
            this.Property((x) => x.IsForceMatch, (m) => m.Column("IsForceMatch"));
            this.Property((x) => x.Created_DTM, (m) => m.Column("Created_DTM"));
            this.Property((x) => x.Changed_DTM, (m) => m.Column("Changed_DTM"));

            this.ManyToOne(x => x.DatasetFileConfig, m =>
            {
                m.Column("Config_ID");
                m.ForeignKey("FK_Schema_DatasetFileConfigs");
                m.Cascade(Cascade.All);
                m.Class(typeof(DatasetFileConfig));
            });

            this.Bag(x => x.DatasetFiles, (m) =>
            {
                m.Lazy(CollectionLazy.Lazy);
                m.Inverse(true);
                m.Table("DatasetFile");
                m.Cascade(Cascade.All);
                m.Cache(c => c.Usage(CacheUsage.ReadWrite));
                m.Key((k) =>
                {
                    k.Column("Schema_ID");
                    k.ForeignKey("FK_DatasetFile_Schema");
                });
            }, map => map.OneToMany(a => a.Class(typeof(DatasetFile))));

            this.Bag(x => x.HiveTables, (m) =>
            {
                m.Lazy(CollectionLazy.Lazy);
                m.Inverse(true);
                m.Table("DatasetFile");
                m.Cascade(Cascade.All);
                m.Cache(c => c.Usage(CacheUsage.ReadWrite));
                m.Key((k) =>
                {
                    k.Column("Schema_ID");
                    k.ForeignKey("FK_HiveTables_Schema");
                });
            }, map => map.OneToMany(a => a.Class(typeof(HiveTable))));

        }
    }
}
