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
    public class MediaTypeExtensionMapping : ClassMapping<MediaTypeExtension>
    {
        public MediaTypeExtensionMapping()
        {
            Table("MediaTypeExtension");

            Id(x => x.Id, m =>
            {
                m.Column("Id");
                m.Generator(Generators.Identity);
            });

            this.Property((x) => x.Key, (m) => m.Column("MediaType"));
            this.Property((x) => x.Value, (m) => m.Column("FileExtension"));
        }
    }
}
