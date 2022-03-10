using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class AssetMapping : ClassMapping<Asset>
    {
        public AssetMapping()
        {
            Table("Asset");

            Cache(c => c.Usage(CacheUsage.ReadWrite));

            Id(x => x.AssetId, m =>
            {
                m.Column("Asset_ID");
                m.Generator(Generators.Identity);
            });

            Property(x => x.SaidKeyCode, m => m.Column("SaidKey_CDE"));

            //ISecurable Mapping
            ManyToOne(x => x.Security, m =>
            {
                m.Column("Security_ID");
                m.ForeignKey("FK_Asset_Security");
                m.Class(typeof(Security));
                m.Cascade(Cascade.All);
            });

        }
    }
}
