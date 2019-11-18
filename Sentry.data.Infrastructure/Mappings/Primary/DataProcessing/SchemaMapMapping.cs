using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Type;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;


namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class SchemaMapMapping : ClassMapping<SchemaMap>
    {
        public SchemaMapMapping()
        {
            this.Table("DataStepToSchema");

            this.Id((x) => x.Id, (m) =>
            {
                m.Generator(Generators.Identity);
            });

            this.Property(x => x.SearchCriteria, m => m.Column("SearchCriteria"));

            this.ManyToOne(x => x.DataFlowStepId, m =>
            {
                m.Column("DataFlowStepId");
                m.Class(typeof(DataFlowStep));
            });

            this.ManyToOne(x => x.Dataset, m =>
            {
                m.Column("DatasetId");
                m.Class(typeof(Dataset));
            });

            this.ManyToOne(x => x.MappedSchema, m =>
            {
                m.Column("SchemaId");
                m.Class(typeof(FileSchema));
            });
        }
    }
}
