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
    public class TagGroupMapping : ClassMapping<TagGroup>
    {
        public TagGroupMapping()
        {
            this.Table("TagGroup");

            this.Id((x) => x.TagGroupId, (m) => {
                m.Column("TagGroupId");
                m.Generator(Generators.Identity);
            });

            this.Property((x) => x.Name);
            this.Property(x => x.Description);
            this.Property((x) => x.Created);
            this.Property((x) => x.Modified);
        }
    }
}
