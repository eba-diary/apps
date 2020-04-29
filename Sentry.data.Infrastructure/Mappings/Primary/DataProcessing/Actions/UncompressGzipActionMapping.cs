using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core.Entities.DataProcessing;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class UncompressGzipActionMapping : SubclassMapping<UncompressGzipAction>
    {
        public UncompressGzipActionMapping()
        {
            DiscriminatorValue(DataActionType.UncompressGzip.ToString());
        }
    }
}
