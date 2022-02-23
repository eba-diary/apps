using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class DatasetAssetMapping : ClassMapping<DatasetAsset>
    {
        public DatasetAssetMapping()
        {
            Table("DatasetAsset");

            Cache(c => c.Usage(CacheUsage.ReadWrite));

            Id(x => x.DatasetAssetId, m =>
            {
                m.Column("DatasetAsset_ID");
                m.Generator(Generators.Identity);
            });

            Property(x => x.SaidKeyCode, m => m.Column("SaidKey_CDE"));

            //ISecurable Mapping
            ManyToOne(x => x.Security, m =>
            {
                m.Column("Security_ID");
                m.ForeignKey("FK_DataAsset_Security");
                m.Class(typeof(Security));
                m.Cascade(Cascade.All);
            });

        }
    }
}
