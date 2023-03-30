using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Type;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;

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

            this.Property(x => x.GlobalDatasetId, m => m.NotNullable(false));
            this.Property((x) => x.DatasetName, (m) => m.Column("Dataset_NME"));
            this.Property((x) => x.ShortName, (m) => m.Column("Short_NME"));
            this.Property((x) => x.DatasetDesc, (m) => m.Column("Dataset_DSC"));
            this.Property((x) => x.CreationUserName, (m) => m.Column("FileCreator_NME"));
            this.Property((x) => x.UploadUserName, (m) => m.Column("UploadedBy_NME"));
            this.Property((x) => x.OriginationCode, (m) => m.Column("Origination_CDE"));
            this.Property((x) => x.DatasetDtm, (m) => m.Column("Dataset_DTM"));
            this.Property((x) => x.ChangedDtm, (m) => m.Column("FileChanged_DTM"));
            this.Property((x) => x.S3Key, (m) => m.Column("S3_KEY"));
            this.Property((x) => x.CanDisplay, (m) => m.Column("Display_IND"));
            this.Property((x) => x.DatasetInformation, (m) => m.Column("Information_DSC"));
            this.Property((x) => x.DatasetType, (m) => m.Column("Dataset_TYP"));
            this.Property((x) => x.DataClassification, (m) => m.Column("DataClassification_CDE"));
            this.Property((x) => x.ObjectStatus, (m) => m.Column("ObjectStatus"));
            this.Property((x) => x.DeleteInd, (m) => m.Column("DeleteInd"));
            this.Property((x) => x.DeleteIssuer, (m) => m.Column("DeleteIssuer"));
            this.Property((x) => x.DeleteIssueDTM, (m) => m.Column("DeleteIssueDTM"));
            this.Property(x => x.NamedEnvironment);
            this.Property(x => x.NamedEnvironmentType, attr => attr.Type<EnumStringType<NamedEnvironmentType>>());
            this.Property((x) => x.AlternateContactEmail);

            Property(x => x.Metadata, m =>
            {
                m.Column("Metadata");
                m.Access(Accessor.Field);

                //http://geekswithblogs.net/lszk/archive/2011/07/11/nhibernatemapping-a-string-field-as-nvarcharmax-in-sql-server-using.aspx
                m.Type(NHibernateUtil.StringClob);
            });

            //new mapping
            this.Bag<Category>(x => x.DatasetCategories, (b) =>
            {
                b.Table("DatasetCategory");
                b.Inverse(false);
                //b.Cascade(Cascade.DeleteOrphans);
                b.Key((k) =>
                {
                    k.Column("Dataset_Id");
                    k.ForeignKey("FK_DatasetCategory_Dataset");
                });
            },
            map => map.ManyToMany(n =>
            {
                n.Column("Category_Id");
                n.ForeignKey("FK_DatasetCategory_Category");
            }));

            this.Bag<BusinessUnit>(x => x.BusinessUnits, (b) =>
            {
                b.Table("DatasetBusinessUnit");
                b.Inverse(false);
                b.Key((k) =>
                {
                    k.Column("Dataset_Id");
                    k.ForeignKey("FK_DatasetBusinessUnit_Dataset");
                });
            },
            map => map.ManyToMany(n =>
            {
                n.Column("BusinessUnit_Id");
                n.ForeignKey("FK_DatasetBusinessUnit_BusinessUnit");
            }));

            this.Bag<DatasetFunction>(x => x.DatasetFunctions, (b) =>
            {
                b.Table("Dataset_DatasetFunction");
                b.Inverse(false);
                b.Key((k) =>
                {
                    k.Column("Dataset_Id");
                    k.ForeignKey("FK_Dataset_Function_Dataset");
                });
            },
            map => map.ManyToMany(n =>
            {
                n.Column("Function_Id");
                n.ForeignKey("FK_Dataset_Function_DatasetFunction");
            }));


            this.Bag(x => x.DatasetFiles, (m) =>
            {
                m.Lazy(CollectionLazy.Lazy);
                m.Inverse(true);
                m.Table("DatasetFile");
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

            this.Bag(
            (x) => x.Tags,
            (m) =>
                {
                    m.Table("ObjectTag");
                    m.Inverse(false);
                    m.Key((k) =>
                        {
                            k.Column("DatasetId");
                            k.ForeignKey("FK_ObjectTag_Dataset");
                        });
                },
            map =>
                {
                    map.ManyToMany(a =>
                        {
                            a.Column("TagId");
                            a.ForeignKey("FK_ObjectTag_Tag");
                        });
                }
            );

            this.Bag(x => x.Favorities, (m) =>
            {
                m.Lazy(CollectionLazy.Lazy);
                m.Inverse(true);
                m.Table("Favorites");
                m.Cascade(Cascade.None);
                m.Key((k) =>
                {
                    k.Column("DatasetId");
                    k.ForeignKey("FK_DatasetFile_Dataset");
                });
            }, map => map.OneToMany(a => a.Class(typeof(Favorite))));

            this.Bag(x => x.Images, (m) =>
            {
                m.Lazy(CollectionLazy.Lazy);
                m.Inverse(true);
                m.Table("Image");
                m.Cascade(Cascade.DeleteOrphans);
                m.Key((k) =>
                {
                    k.Column("ParentDataset");
                    k.ForeignKey("FK_Image_Dataset");
                });
            }, map => map.OneToMany(a => a.Class(typeof(Image))));

            ManyToOne(x => x.Asset, m =>
            {
                m.Column("Asset_ID");
                m.ForeignKey("FK_Dataset_Asset");
                m.Class(typeof(Asset));
                m.Cascade(Cascade.All);
            });

            //ISecurable Mapping
            this.Property((x) => x.IsSecured, (m) => m.Column("IsSecured_IND"));
            this.Property((x) => x.PrimaryContactId, (m) => m.Column("PrimaryContact_ID"));
            this.ManyToOne(x => x.Security, m =>
            {
                m.Column("Security_ID");
                m.ForeignKey("FK_Dataset_Security");
                m.Class(typeof(Security));
                m.Cascade(Cascade.All);
            });

        }
    }
}
