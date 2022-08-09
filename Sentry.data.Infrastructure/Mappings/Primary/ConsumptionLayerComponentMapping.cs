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
    public class ConsumptionLayerComponentMapping : ClassMapping<ConsumptionLayerComponent>
    {
        public ConsumptionLayerComponentMapping()
        {
            this.Table("ConsumptionLayerComponent");

            this.Cache(c => c.Usage(CacheUsage.ReadOnly));

            this.Id(x => x.Id, m =>
            {
                m.Column("CLC_ID");
                m.Generator(Generators.Identity);
            });

            this.Property(x => x.DataAsset_Id, m => m.Column("DataAsset_ID"));
            this.Property(x => x.Type_Id, m => m.Column("CLType_ID"));

            this.ManyToOne(x => x.Type, m =>
            {
                m.Column("CLType_ID");
                m.ForeignKey("FK_ConsumptionLayerComponent_ConsumptionLayerType");
                m.Class(typeof(ConsumptionLayerType));
                m.Insert(false);
                m.Update(false);
            });

            this.Bag(x => x.ComponentElements, m =>
            {
                m.Cache(c => c.Usage(CacheUsage.ReadOnly));
                m.Inverse(true);
                m.Table("ComponentElement");
                m.Cascade(Cascade.All);
                m.Key(k =>
                {
                    k.Column("CLC_ID");
                    k.ForeignKey("FK_ComponentElement_ConsumptionLayerComponent");
                });
            }, map => map.OneToMany(a => a.Class(typeof(ComponentElement))));
        }
    }
}
