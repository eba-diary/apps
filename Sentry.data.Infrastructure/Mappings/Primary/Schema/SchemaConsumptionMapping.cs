using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class SchemaConsumptionMapping: ClassMapping<SchemaConsumption>
    {
        public SchemaConsumptionMapping()
        {
            Table("SchemaConsumption");

            Id(x => x.SchemaConsumptionId, (m) =>
            {
                m.Column("SchemaConsumption_Id");
                m.Generator(Generators.Identity);
            });

            Discriminator(x => x.Column("SchemaConsumption_TYP"));
        }
    }
}
