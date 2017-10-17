using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class DatasetScopeTypeMapping : ClassMapping<DatasetScopeType>
    {

        public DatasetScopeTypeMapping()
        {
            this.Table("DatasetScopeTypes");

            this.Cache((c) => c.Usage(CacheUsage.ReadWrite));

            this.Id((x) => x.ScopeTypeId, (m) =>
            {
                m.Column("ScopeType_ID");
                m.Generator(Generators.Identity);
            });

            this.Property((x) => x.Name, (m) => m.Column("Name"));
            this.Property((x) => x.Description, (m) => m.Column("Type_DSC"));
            this.Property((x) => x.IsEnabled, (m) => m.Column("IsEnabled_IND"));

        }
    }
}
