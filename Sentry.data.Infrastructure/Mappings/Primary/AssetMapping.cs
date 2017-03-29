using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class AssetMapping : ClassMapping<Asset>
    {
        public AssetMapping()
        {

            this.Table("Asset");

            this.Id((x) => x.Id, (m) =>
            {
                m.Access(Accessor.Field);
                m.Generator(Generators.Identity);
            });

            this.Version((x) => x.Version, (m) => m.Access(Accessor.Field));
            this.Property((x) => x.Name, (m) => m.Access(Accessor.Field));
            this.Property((x) => x.Description, (m) => m.Access(Accessor.Field));
            //this.Property((x) => x.State, (m) => m.Access(Accessor.Field));

            this.Bag((x) => x.Categories, (c) =>
            {
                c.Access(Accessor.Field);
                c.Table("CategorizedAsset");
                c.Key((k) =>
                    {
                        k.Column("AssetId");
                        k.ForeignKey("FK_CategorizedAsset_Asset");
                    });
                c.BatchSize(10);
            },
            (m) =>
            {
                m.ManyToMany((a) =>
                {
                    a.Column("CategoryId");
                    a.ForeignKey("FK_CategorizedAsset_Category");
               });
            });
        }
    }
}
