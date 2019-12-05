using System;
using System.Collections.Generic;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core.Entities.DataProcessing;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class SchemaLoadActionMapping : SubclassMapping<SchemaLoadAction>
    {
        public SchemaLoadActionMapping()
        {
            DiscriminatorValue(DataActionType.SchemaLoad.ToString());
        }
    }
}
