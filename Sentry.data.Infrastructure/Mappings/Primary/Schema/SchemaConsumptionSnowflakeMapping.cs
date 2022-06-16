using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Type;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class SchemaConsumptionSnowflakeMapping: JoinedSubclassMapping<SchemaConsumptionSnowflake>
    {

        public SchemaConsumptionSnowflakeMapping()
        {
            Table("SchemaConsumptionSnowflake");

            Key(k =>
            {
                k.Column("SchemaConsumptionSnowflake_Id");
                k.ForeignKey("FK_SchemaConsumptionSnowflake_SchemaConsumption");
                k.NotNullable(true);
                k.OnDelete(OnDeleteAction.Cascade);
                k.Unique(true);
                k.Update(true);
            });

            Property(x => x.SnowflakeTable);
            Property(x => x.SnowflakeDatabase);
            Property(x => x.SnowflakeSchema);
            Property(x => x.SnowflakeStatus);
            Property(x => x.SnowflakeStage);
            Property(x => x.SnowflakeWarehouse); 
            Property(x => x.SnowflakeType,
                     attr => {
                     attr.Column("Snowflake_TYP");
                     attr.Type<EnumStringType<SnowflakeConsumptionType>>();
                     });
        }
    }
}
