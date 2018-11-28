using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class CategoryMapping : ClassMapping<Category>
    {
        public CategoryMapping()
        {

            this.Table("Category");

            this.Cache((c) =>
                         c.Usage(CacheUsage.ReadWrite));

            this.Id((x) => x.Id, (m) => {
                m.Column("Id");
                m.Generator(Generators.Identity);
            });

            this.Property((x) => x.Name);
            this.Property(x => x.Color);
            this.Property((x) => x.ObjectType, (m) => m.Column("Object_TYP"));
            this.Property((x) => x.AbbreviatedName);

        }
    }
}
