using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core.Entities.DataProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class SchemaMapActionMapping : SubclassMapping<SchemaMapAction>
    {
        public SchemaMapActionMapping()
        {
            DiscriminatorValue(DataActionType.SchemaMap.ToString());
        }
    }
}
