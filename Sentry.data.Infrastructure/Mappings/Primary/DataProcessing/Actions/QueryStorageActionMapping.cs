using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core.Entities.DataProcessing;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class QueryStorageActionMapping : SubclassMapping<QueryStorageAction>
    {
        public QueryStorageActionMapping()
        {
            DiscriminatorValue(DataActionType.QueryStorage.ToString());
        }
    }
}
