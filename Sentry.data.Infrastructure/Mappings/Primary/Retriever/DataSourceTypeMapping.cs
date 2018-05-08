using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class DataSourceTypeMapping : ClassMapping<DataSourceType>
    {
        public DataSourceTypeMapping()
        {
            Table("DataSourceType");

            Property(x => x.Name, m =>
            {
                m.Column("Name");
                m.NotNullable(true);
            });

            Property(x => x.Description, m =>
            {
                m.Column("Description");
                m.NotNullable(true);
            });

            Property(x => x.DiscrimatorValue, m =>
            {
                m.Column("DiscrimatorValue");
                m.NotNullable(true);
            });
        }
    }
}
