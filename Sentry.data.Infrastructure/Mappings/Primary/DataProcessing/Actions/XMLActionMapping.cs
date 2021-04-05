using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core.Entities.DataProcessing;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class XMLActionMapping : SubclassMapping<XMLAction>
    {
        public XMLActionMapping()
        {
            DiscriminatorValue(DataActionType.XML.ToString());
        }
    }
}
