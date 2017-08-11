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
    public class ComponentElementMapping : ClassMapping<ComponentElement>
    {
        public ComponentElementMapping()
        {
            this.Table("ComponentElement");

            this.Cache(c => c.Usage(CacheUsage.ReadOnly));

            this.Id(x => x.Id, m =>
            {
                m.Column("Element_ID");
                m.Generator(Generators.Identity);
            });

            this.Property(x => x.ParentId, m => m.Column("Parent_ID"));
            this.Property(x => x.Name, m => m.Column("Element_NME"));
            this.Property(x => x.Link, m => m.Column("Link_URL"));
            this.Property(x => x.CLC_Id, m => m.Column("CLC_ID"));

            this.Bag(x => x.Elements, m =>
            {
                m.Cache(c => c.Usage(CacheUsage.ReadOnly));
                m.Inverse(true);
                m.Table("ComponentElement");
                m.Cascade(Cascade.All);
                m.Key(k =>
                {
                    k.Column("Parent_ID");
                    k.ForeignKey("FK_ComponentElement_ComponentElement");
                });
            }, map => map.OneToMany(a => a.Class(typeof(ComponentElement))));
        }
    }
}
