using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core.Entities.DataProcessing;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class GoogleBigQueryApiActionMapping : SubclassMapping<GoogleBigQueryApiAction>
    {
        public GoogleBigQueryApiActionMapping()
        {
            DiscriminatorValue(DataActionType.GoogleBigQueryApi.ToString());
        }
    }
}
