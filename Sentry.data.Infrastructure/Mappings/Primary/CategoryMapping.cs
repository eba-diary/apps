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
                m.Access(Accessor.Field);
                m.Generator(Generators.Identity);
            });

            this.Property((x) => x.Name);

            this.ManyToOne((x) => x.ParentCategory, (m) => {
                m.Access(Accessor.Field);
                m.ForeignKey("FK_Category_Category");
            });

            this.Bag((x) => x.SubCategories,
                (c) =>
                {
                    c.Inverse(true);
                    c.Cascade(Cascade.All);
                    c.Access(Accessor.Field);
                    //c.Cache(Sub(h)
                    //            h.Usage(CacheUsage.ReadWrite)
                    //            h.Region(QueryCacheRegion.MediumTerm.ToString)
                    //        End Sub)
                },
                (m) =>
                    m.OneToMany());



        }
    }
}
