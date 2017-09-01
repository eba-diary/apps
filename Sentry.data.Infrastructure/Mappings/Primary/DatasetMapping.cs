using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class DatasetMapping : ClassMapping<Dataset>
    {
        public DatasetMapping()
        {
            // Lazy(true);

            this.Table("Dataset");

            this.Cache((c) => c.Usage(CacheUsage.ReadWrite));

            this.Id((x) => x.DatasetId, (m) =>
            {
                m.Column("Dataset_ID");
                m.Generator(Generators.Identity);
            });

            //this.Version((x) => x.Version, (m) => m.Access(Accessor.Field));

            this.Property((x) => x.Category, (m) => m.Column("Category_CDE"));
            this.Property((x) => x.DatasetName, (m) => m.Column("Dataset_NME"));
            this.Property((x) => x.DatasetDesc, (m) => m.Column("Dataset_DSC"));
            this.Property((x) => x.CreationUserName, (m) => m.Column("FileCreator_NME"));
            this.Property((x) => x.SentryOwnerName, (m) => m.Column("SentryOwner_NME"));
            this.Property((x) => x.UploadUserName, (m) => m.Column("UploadedBy_NME"));
            this.Property((x) => x.OriginationCode, (m) => m.Column("Origination_CDE"));
            //this.Property((x) => x.FileExtension,  (m) => m.Column("xxx"));
            this.Property((x) => x.DatasetDtm, (m) => m.Column("Dataset_DTM"));
            this.Property((x) => x.ChangedDtm, (m) => m.Column("FileChanged_DTM"));
            this.Property((x) => x.UploadDtm, (m) => m.Column("FileUploaded_DTM"));
            this.Property((x) => x.CreationFreqDesc, (m) => m.Column("CreationFreq_DSC"));
            this.Property((x) => x.FileSize, (m) => m.Column("FileSize_CNT"));
            this.Property((x) => x.RecordCount, (m) => m.Column("Row_CNT"));
            this.Property((x) => x.S3Key, (m) => m.Column("S3_KEY"));
            this.Property((x) => x.IsSensitive, (m) => m.Column("IsSensitive_IND"));
            this.Property((x) => x.CanDisplay, (m) => m.Column("Display_IND"));

            this.ManyToOne(x => x.DatasetCategory, m =>
            {
                m.Column("Category_ID");
                m.ForeignKey("FK_Dataset_Category");
                m.Class(typeof(Category));
            });

            this.Bag((x) => x.RawMetadata, (m) =>
            {
                //m.Lazy(CollectionLazy.Lazy);
                m.Inverse(true);
                m.Table("DatasetMetadata");
                m.Cascade(Cascade.All);
                m.Cache(c => c.Usage(CacheUsage.ReadWrite));
                //m.Access(Accessor.Field);
                //m.Key((k) => k.Column("Dataset_ID"));
                m.Key((k) =>
                {
                    k.Column("Dataset_ID");
                    k.ForeignKey("FK_DatasetMetadata_Dataset");
                });
            }, map => map.OneToMany(a => a.Class(typeof(DatasetMetadata))));

            //(m) =>
            //{
            //    m.OneToMany((a) =>
            //    {
            //        a.Column("CategoryId");
            //        a.ForeignKey("FK_CategorizedAsset_Category");
            //    });
            //});
        }
    }
}
