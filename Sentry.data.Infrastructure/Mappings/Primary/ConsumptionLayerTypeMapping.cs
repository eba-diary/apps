using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class ConsumptionLayerTypeMapping : ClassMapping<ConsumptionLayerType>
    {
        public ConsumptionLayerTypeMapping()
        {
            this.Table("ConsumptionLayerType");

            this.Cache(c => c.Usage(CacheUsage.ReadOnly));

            this.Id(x => x.Id, m =>
            {
                m.Column("CLType_ID");
                m.Generator(Generators.Identity);
            });

            this.Property(x => x.Name, m => m.Column("CLType_NME"));
            this.Property(x => x.ChildDescription, m => m.Column("Child_DSC"));
            this.Property(x => x.Code, m => m.Column("CLType_CDE"));
            this.Property(x => x.Tool, m => m.Column("Tool_URL"));
            this.Property(x => x.ToolDescription, m => m.Column("Tool_DSC"));
            this.Property(x => x.Color, m => m.Column("Color_NME"));
        }
    }
}
