using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Type;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class MigrationHistoryDetailMapping : ClassMapping<MigrationHistoryDetail>
    {
        public MigrationHistoryDetailMapping()
        {
            this.Table("MigrationHistoryDetail");

            this.Cache((c) => c.Usage(CacheUsage.ReadWrite));

            this.Id((x) => x.MigrationHistoryDetailId, (m) =>
            {
                m.Column("MigrationHistoryDetailId");
                m.Generator(Generators.Identity);
            });

            this.Property(x => x.MigrationHistoryId);

            this.Property(x => x.SourceDatasetId);
            this.Property(x => x.SourceSchemaId);

            this.Property(x => x.IsDatasetMigrated);
            this.Property(x => x.DatasetId);
            this.Property(x => x.DatasetName);
            this.Property(x => x.DatasetMigrationMessage);

            this.Property(x => x.IsDataFlowMigrated);
            this.Property(x => x.DataFlowId);
            this.Property(x => x.DataFlowName);
            this.Property(x => x.DataFlowMigrationMessage);
            
            this.Property(x => x.IsSchemaMigrated);
            this.Property(x => x.SchemaId);
            this.Property(x => x.SchemaName);
            this.Property(x => x.SchemaMigrationMessage);

            this.Property(x => x.IsSchemaRevisionMigrated);
            this.Property(x => x.SchemaRevisionId);
            this.Property(x => x.SchemaRevisionName);
            this.Property(x => x.SchemaRevisionMigrationMessage);
        }
    }
}
