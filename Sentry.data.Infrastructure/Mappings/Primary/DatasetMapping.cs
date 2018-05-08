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
            this.Property((x) => x.DatasetDtm, (m) => m.Column("Dataset_DTM"));
            this.Property((x) => x.ChangedDtm, (m) => m.Column("FileChanged_DTM"));
            this.Property((x) => x.S3Key, (m) => m.Column("S3_KEY"));
            this.Property((x) => x.IsSensitive, (m) => m.Column("IsSensitive_IND"));
            this.Property((x) => x.CanDisplay, (m) => m.Column("Display_IND"));
            //this.Property((x) => x.DatafilesToKeep, (m) => m.Column("DatafilesToKeep_NBR"));
            this.Property((x) => x.DropLocation, (m) => m.Column("DropLocation"));
            this.Property((x) => x.DatasetInformation, (m) => m.Column("Information_DSC"));

            this.ManyToOne(x => x.DatasetCategory, m =>
            {
                m.Column("Category_ID");
                m.ForeignKey("FK_Dataset_Category");
                m.Class(typeof(Category));
            });


            
            //this.ManyToOne(x => x.DatasetScopeType, m =>
            //{
            //    m.Column("DatasetScopeType_ID");
            //    m.ForeignKey("FK_Dataset_DatasetScopeTypes");
            //    m.Class(typeof(DatasetScopeType));
            //});

            //this.Bag((x) => x.RawMetadata, (m) =>
            //{
            //    //m.Lazy(CollectionLazy.Lazy);
            //    m.Inverse(true);
            //    m.Table("DatasetMetadata");
            //    m.Cascade(Cascade.All);
            //    m.Cache(c => c.Usage(CacheUsage.ReadWrite));
            //    //m.Access(Accessor.Field);
            //    //m.Key((k) => k.Column("Dataset_ID"));
            //    m.Key((k) =>
            //    {
            //        k.Column("Dataset_ID");
            //        k.ForeignKey("FK_DatasetMetadata_Dataset");
            //    });
            //}, map => map.OneToMany(a => a.Class(typeof(DatasetMetadata))));

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
