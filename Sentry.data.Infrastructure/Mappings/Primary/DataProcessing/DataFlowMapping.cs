using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core.Entities.DataProcessing;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class DataFlowMapping : ClassMapping<DataFlow>
    {
        public DataFlowMapping()
        {
            this.Table("DataFlow");

            this.Id(x => x.Id, m =>
            {
                m.Column("DataFlow_ID");
                m.Generator(Generators.Identity);
            });

            this.Property(x => x.Name, m => m.Column("Name"));
            this.Property(x => x.CreatedDTM, m => m.Column("Created_DTM"));
            this.Property(x => x.CreatedBy, m => m.Column("CreatedBy"));

            this.Bag(x => x.Steps, (m) =>
            {
                m.Inverse(true);
                m.Table("DataFlowStep");
                m.Cascade(Cascade.All);
                m.Cache(c => c.Usage(CacheUsage.ReadWrite));
                m.Key((k) =>
                {
                    k.Column("DataFlow_ID");
                });
            }, map => map.OneToMany(a => a.Class(typeof(DataFlowStep))));
        }
    }
}
