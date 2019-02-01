using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class DatasetFunctionMapping : ClassMapping<DatasetFunction>
    {
        public DatasetFunctionMapping()
        {
            this.Table("DatasetFunction");

            this.Cache(c => c.Usage(CacheUsage.ReadOnly));

            this.Id(x => x.Id, m =>
            {
                m.Column("Id");
                m.Generator(Generators.Identity);
            });

            this.Property(x => x.Name, m => m.Column("Name"));
            this.Property(x => x.Sequence, m => m.Column("Sequence"));
        }
    }
}