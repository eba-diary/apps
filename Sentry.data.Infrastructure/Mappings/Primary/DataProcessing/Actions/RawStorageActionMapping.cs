using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core.Entities.DataProcessing;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class RawStorageActionMapping : SubclassMapping<RawStorageAction>
    {
        public RawStorageActionMapping()
        {
            DiscriminatorValue(DataActionType.RawStorage.ToString());
        }
    }
}
