using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core.Entities.DataProcessing;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class ClaimIQActionMapping : SubclassMapping<ClaimIQAction>
    {
        public ClaimIQActionMapping()
        {
            DiscriminatorValue(DataActionType.ClaimIq.ToString());
        }
    }
}
