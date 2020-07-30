using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core.Entities.DataProcessing;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class FixedWidthActionMapping : SubclassMapping<FixedWidthAction>
    {
        public FixedWidthActionMapping()
        {
            DiscriminatorValue(DataActionType.FixedWidth.ToString());
        }
    }
}
