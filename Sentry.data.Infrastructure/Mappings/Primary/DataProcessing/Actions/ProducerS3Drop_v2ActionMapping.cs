using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core.Entities.DataProcessing;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class ProducerS3Drop_v2ActionMapping : SubclassMapping<ProducerS3Drop_v2Action>
    {
        public ProducerS3Drop_v2ActionMapping()
        {
            DiscriminatorValue(DataActionType.ProducerS3Drop_v2.ToString());
        }
    }
}
