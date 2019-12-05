using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core.Entities.DataProcessing;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class ConvertToParquetMapping : SubclassMapping<ConvertToParquetAction>
    {
        public ConvertToParquetMapping()
        {
            DiscriminatorValue(DataActionType.ConvertParquet.ToString());
        }
    }
}
