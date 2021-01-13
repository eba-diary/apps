using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core.Entities.DataProcessing;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class ProducerS3DropActionMapping : SubclassMapping<ProducerS3DropAction>
    {
        public ProducerS3DropActionMapping()
        {
            DiscriminatorValue(DataActionType.ProducerS3Drop.ToString());
        }
    }
}
