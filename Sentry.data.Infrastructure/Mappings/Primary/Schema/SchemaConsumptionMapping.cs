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

            ManyToOne(x => x.Schema, m =>
            {
                m.Column("Schema_Id");
                m.ForeignKey("FK_SchemaConsumption_Schema");
                m.Class(typeof(FileSchema));
            });
        }
    }
}
