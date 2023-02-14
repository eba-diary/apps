using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core.Entities.DataProcessing;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class CopyToParquetMapping : SubclassMapping<CopyToParquetAction>
    {
        public CopyToParquetMapping()
        {
            DiscriminatorValue(DataActionType.CopyToParquet.ToString());
        }
    }
}
