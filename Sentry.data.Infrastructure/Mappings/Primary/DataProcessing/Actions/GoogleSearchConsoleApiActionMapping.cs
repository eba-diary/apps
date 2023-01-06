using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core.Entities.DataProcessing;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class GoogleSearchConsoleApiActionMapping : SubclassMapping<GoogleSearchConsoleApiAction>
    {
        public GoogleSearchConsoleApiActionMapping()
        {
            DiscriminatorValue(DataActionType.GoogleSearchConsoleApi.ToString());
        }
    }
}
