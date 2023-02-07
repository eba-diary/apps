using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Type;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class MigrationHistoryMapping : ClassMapping<MigrationHistory>
    {
        public MigrationHistoryMapping()
        {
            this.Table("MigrationHistory");

            this.Cache((c) => c.Usage(CacheUsage.ReadWrite));

            this.Id((x) => x.MigrationHistoryId, (m) =>
            {
                m.Column("MigrationHistoryId");
                m.Generator(Generators.Identity);
            });
            
            this.Property(x => x.CreateDateTime);
            
            this.Bag(x => x.MigrationHistoryDetails, (m) =>
            {
                m.Lazy(CollectionLazy.Lazy);
                m.Inverse(true);
                m.Table("MigrationHistoryDetail");
                m.Cascade(Cascade.None);
                m.Key((k) =>
                {
                    k.Column("MigrationHistoryId");
                    k.ForeignKey("FK_MigrationHistoryDetail_MigrationHistory_MigrationHistoryId");
                });
            }, map => map.OneToMany(a => a.Class(typeof(MigrationHistoryDetail))));
        }
    }
}
