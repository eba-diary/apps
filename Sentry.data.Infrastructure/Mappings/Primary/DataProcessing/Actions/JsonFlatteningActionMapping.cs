using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core.Entities.DataProcessing;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class JsonFlatteningActionMapping : SubclassMapping<JsonFlatteningAction>
    {
        public JsonFlatteningActionMapping()
        {
            DiscriminatorValue(DataActionType.JsonFlattening.ToString());
        }
    }
}
