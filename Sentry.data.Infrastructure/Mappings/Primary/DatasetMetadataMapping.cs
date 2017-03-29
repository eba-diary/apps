using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class DatasetMetadataMapping : ClassMapping<DatasetMetadata>
    {
        public DatasetMetadataMapping()
        {
            this.Table("DatasetMetadata");

            this.Cache((c) => c.Usage(CacheUsage.ReadWrite));

            this.Id((x) => x.DatasetMetadataId, (m) => {
                m.Column("DatasetMetadata_ID");
                m.Generator(Generators.Identity);
            });
            //this.Property((x) => x.DatasetId, (m) => m.Column("Dataset_ID"));
            this.Property((x) => x.Name, (m) => m.Column("Metadata_NME"));
            this.Property((x) => x.Name, (m) => m.Column("Metadata_VAL"));
            this.Property((x) => x.Name, (m) => m.Column("IsColumn_IND"));
            this.ManyToOne((x) => x.Parent, map =>
            {
                map.Column("Dataset_ID");
                map.ForeignKey("FK_DatasetMetadata_Dataset");
                map.Class(typeof(Dataset));
            });
        }
    }
}
