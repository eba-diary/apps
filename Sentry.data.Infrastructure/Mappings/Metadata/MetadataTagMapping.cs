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
    public class MetadataTagMapping : ClassMapping<MetadataTag>
    {
        public MetadataTagMapping()
        {
            this.Lazy(false);

            this.Table("Tag");

            this.Id((x) => x.TagId, (m) =>
            {
                m.Column("TagId");
                m.Generator(Generators.Identity);
            });

            this.Property(x => x.Name);
            this.Property(x => x.Created);
            this.Property(x => x.CreatedBy);
            this.Bag(
            (x) => x.Datasets,
            (m) =>
                {
                    m.Table("ObjectTag");
                    m.Inverse(true);
                    m.Key((k) =>
                        {
                            k.Column("TagId");
                            k.ForeignKey("FK_ObjectTag_Tag");
                        });
                },
            map =>
                {
                    map.ManyToMany(a =>
                    {
                        a.Column("DatasetId");
                        a.ForeignKey("FK_ObjectTag_Dataset");
                    });
                }
            );
        }
    }
}
