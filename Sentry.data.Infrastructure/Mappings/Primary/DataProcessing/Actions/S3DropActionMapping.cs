using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core.Entities.DataProcessing;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class S3DropActionMapping : SubclassMapping<S3DropAction>
    {
        public S3DropActionMapping()
        {
            DiscriminatorValue(DataActionType.S3Drop.ToString());
        }
    }
}
