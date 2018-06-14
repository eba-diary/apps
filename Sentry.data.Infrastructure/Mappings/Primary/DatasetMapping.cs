using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class DatasetMapping : ClassMapping<Dataset>
    {
        public DatasetMapping()
        {
            this.Table("Dataset");

            this.Cache((c) => c.Usage(CacheUsage.ReadWrite));

            this.Id((x) => x.DatasetId, (m) =>
            {
                m.Column("Dataset_ID");
                m.Generator(Generators.Identity);
            });

            this.Property((x) => x.Category, (m) => m.Column("Category_CDE"));
            this.Property((x) => x.DatasetName, (m) => m.Column("Dataset_NME"));
            this.Property((x) => x.DatasetDesc, (m) => m.Column("Dataset_DSC"));
            this.Property((x) => x.CreationUserName, (m) => m.Column("FileCreator_NME"));
            this.Property((x) => x.SentryOwnerName, (m) => m.Column("SentryOwner_NME"));
            this.Property((x) => x.UploadUserName, (m) => m.Column("UploadedBy_NME"));
            this.Property((x) => x.OriginationCode, (m) => m.Column("Origination_CDE"));
            this.Property((x) => x.DatasetDtm, (m) => m.Column("Dataset_DTM"));
            this.Property((x) => x.ChangedDtm, (m) => m.Column("FileChanged_DTM"));
            this.Property((x) => x.S3Key, (m) => m.Column("S3_KEY"));
            this.Property((x) => x.IsSensitive, (m) => m.Column("IsSensitive_IND"));
            this.Property((x) => x.CanDisplay, (m) => m.Column("Display_IND"));
            this.Property((x) => x.DatasetInformation, (m) => m.Column("Information_DSC"));

            this.ManyToOne(x => x.DatasetCategory, m =>
            {
                m.Column("Category_ID");
                m.ForeignKey("FK_Dataset_Category");
                m.Class(typeof(Category));
            });

            this.Bag(x => x.DatasetFiles, (m) =>
            {
                m.Lazy(CollectionLazy.Lazy);
                m.Inverse(true);
                m.Table("DatasetFile");
                m.Cascade(Cascade.All);
                m.Cache(c => c.Usage(CacheUsage.ReadWrite));
                m.Key((k) =>
                {
                    k.Column("Dataset_ID");
                    k.ForeignKey("FK_DatasetFile_Dataset");
                });
            }, map => map.OneToMany(a => a.Class(typeof(DatasetFile))));

            this.Bag((x) => x.DatasetFileConfigs, (m) =>
            {
                m.Lazy(CollectionLazy.Lazy);
                m.Inverse(true);
                m.Table("DatasetFileConfigs");
                m.Cascade(Cascade.All);
                m.Cache(c => c.Usage(CacheUsage.ReadWrite));
                m.Key((k) =>
                {
                    k.Column("Dataset_ID");
                    k.ForeignKey("FK_DatasetFileConfigs_Dataset");
                });
            }, map => map.OneToMany(a => a.Class(typeof(DatasetFileConfig))));
        }
    }
}
