using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;
using NHibernate;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class DataSourceMapping : ClassMapping<DataSource>
    {
        public DataSourceMapping()
        {
            Table("DataSource");

            Id(x => x.Id, m =>
            {
                m.Column("DataSource_Id");
                m.Generator(Generators.Identity);
            });

            Discriminator(x => x.Column("SourceType_IND"));

            Property(x => x.Name, m =>
            {
                m.Column("Source_NME");
                m.NotNullable(true);
            });

            Property(x => x.Description, m => 
            {
                m.Column("Source_DSC");
                m.NotNullable(false);
            });

            Property(x => x.BaseUri, m => 
            {
                m.Column("BaseUri");
                m.NotNullable(true);
            });

            Property(x => x.IsUriEditable, m =>
            {
                m.Column("IsUriEditable_IND");
                m.NotNullable(true);
            });

            Property(x => x.KeyCode, m =>
            {
                m.Column("KeyCode_CDE");
                m.NotNullable(true);
            });

            Property(x => x.Created, m =>
            {
                m.Column("Created_DTM");
                m.NotNullable(true);
            });

            Property(x => x.Modified, m =>
            {
                m.Column("Modified_DTM");
                m.NotNullable(true);
            });

            Property(x => x.Bucket, m =>
            {
                m.Column("Bucket_NME");
                m.NotNullable(false);
            });

            Property(x => x.PortNumber, m =>
            {
                m.Column("PortNumber");
                m.NotNullable(false);
            });

            Property(x => x.HostFingerPrintKey, m =>
            {
                m.Column("HostFingerPrintkey");
                m.NotNullable(false);
            });

            Property(x => x.IsUserPassRequired, m =>
            {
                m.Column("IsUserPassRequired");
                m.NotNullable(false);
            });

            this.ManyToOne(x => x.SourceAuthType, m =>
            {
                m.Column("SourceAuth_ID");
                m.ForeignKey("FK_DataSource_AuthenticationType");
                m.Class(typeof(AuthenticationType));
            });
            this.Property((x) => x.PrimaryContactId, (m) => m.Column("PrimaryContact_ID"));

            //ISecurable Mapping
            this.Property((x) => x.IsSecured, (m) => m.Column("IsSecured_IND"));
            this.ManyToOne(x => x.Security, m =>
            {
                m.Column("Security_ID");
                m.ForeignKey("FK_DataSource_Security");
                m.Class(typeof(Security));
                m.Cascade(Cascade.All);
            });


        }

        //http://notherdev.blogspot.com/2012/01/mapping-by-code-inheritance.html
    }

    public class DfsSourceMapping : SubclassMapping<DfsSource>
    {
        public DfsSourceMapping()
        {
            DiscriminatorValue(GlobalConstants.DataSourceDiscriminator.DFS_SOURCE);
        }
    }

    public class DfsBasicMapping : SubclassMapping<DfsBasic>
    {
        public DfsBasicMapping()
        {
            DiscriminatorValue(GlobalConstants.DataSourceDiscriminator.DEFAULT_DROP_LOCATION);
        }
    }

    public class DfsDataFlowBasicMapping : SubclassMapping<DfsDataFlowBasic>
    {
        public DfsDataFlowBasicMapping()
        {
            DiscriminatorValue(GlobalConstants.DataSourceDiscriminator.DEFAULT_DATAFLOW_DFS_DROP_LOCATION);
        }
    }

    public class DfsCustomMapping : SubclassMapping<DfsCustom>
    {
        public DfsCustomMapping()
        {
            DiscriminatorValue(GlobalConstants.DataSourceDiscriminator.DFS_CUSTOM);
        }
    }

    public class FtpSourceMapping : SubclassMapping<FtpSource>
    {
        public FtpSourceMapping()
        {
            DiscriminatorValue(GlobalConstants.DataSourceDiscriminator.FTP_SOURCE);
        }
    }

    public class S3SourceMapping : SubclassMapping<S3Source>
    {
        public S3SourceMapping()
        {
            DiscriminatorValue(GlobalConstants.DataSourceDiscriminator.S3_SOURCE);
        }
    }

    public class S3BasicMapping : SubclassMapping<S3Basic>
    {
        public S3BasicMapping()
        {
            DiscriminatorValue(GlobalConstants.DataSourceDiscriminator.DEFAULT_S3_DROP_LOCATION);
        }
    }

    public class SFtpSourceMapping : SubclassMapping<SFtpSource>
    {
        public SFtpSourceMapping()
        {
            DiscriminatorValue(GlobalConstants.DataSourceDiscriminator.SFTP_SOURCE);
        }
    }

    public class HTTPSSourceMapping : SubclassMapping<HTTPSSource>
    {
        public HTTPSSourceMapping()
        {
            DiscriminatorValue(GlobalConstants.DataSourceDiscriminator.HTTPS_SOURCE);


            Property(x => x.AuthenticationHeaderName, m =>
            {
                m.Column("AuthHeaderName");
                m.NotNullable(false);
            });

            Property(x => x.AuthenticationTokenValue, m =>
            {
                m.Column("AuthHeaderValue");
                m.NotNullable(false);
            });

            Property(x => x.IVKey, m =>
            {
                m.Column("IVKey");
                m.NotNullable(false);
            });

            Property(x => x.RequestHeaders, m =>
            {
                m.Column("RequestHeaders");
                m.Access(Accessor.Field);
            });

            Property(x => x.ClientId, m =>
            {
                m.Column("ClientId");
                m.NotNullable(false);
            });

            Property(x => x.ClientPrivateId, m =>
            {
                m.Column("ClientPrivateId");
                m.NotNullable(false);
            });

            Property(x => x.Scope, m =>
            {
                m.Column("Scope");
                m.NotNullable(false);
            });

            Property(x => x.TokenUrl, m =>
            {
                m.Column("TokenUrl");
                m.NotNullable(false);
            });

            Property(x => x.TokenExp, m =>
            {
                m.Column("TokenExp");
                m.NotNullable(false);
            });

            Property(x => x.CurrentToken, m =>
            {
                m.Column("CurrentToken");
                m.NotNullable(false);
            });

            Property(x => x.CurrentTokenExp, m =>
            {
                m.Column("CurrentTokenExp");
                m.NotNullable(false);
            });

            this.Bag(x => x.Claims, (m) =>
            {
                m.Inverse(true);
                m.Table("AuthenticationClaims");
                m.Key((k) =>
                {
                    k.Column("DataSource_Id");
                    k.ForeignKey("FK_AuthenticationClaims_DataSource");
                });
            }, map => map.OneToMany(a => a.Class(typeof(OAuthClaim))));
        }
    }

    public class JavaAppSourceMapping : SubclassMapping<JavaAppSource>
    {
        public JavaAppSourceMapping()
        {
            DiscriminatorValue(GlobalConstants.DataSourceDiscriminator.JAVA_APP_SOURCE);

            Property(x => x.Options, m =>
            {
                m.Column("Options");
                m.Access(Accessor.Field);
                //http://geekswithblogs.net/lszk/archive/2011/07/11/nhibernatemapping-a-string-field-as-nvarcharmax-in-sql-server-using.aspx
                m.Type(NHibernateUtil.StringClob);
            });
        }
    }

    public class DfsBasicHszMapping : SubclassMapping<DfsBasicHsz>
    {
        public DfsBasicHszMapping()
        {
            DiscriminatorValue(GlobalConstants.DataSourceDiscriminator.DEFAULT_HSZ_DROP_LOCATION);
        }
    }

    public class GoogleApiMapping : SubclassMapping<GoogleApiSource>
    {
        public GoogleApiMapping()
        {
            DiscriminatorValue(GlobalConstants.DataSourceDiscriminator.GOOGLE_API_SOURCE);
        }
    }

    public class GoogleBigQueryApiMapping : SubclassMapping<GoogleBigQueryApiSource>
    {
        public GoogleBigQueryApiMapping()
        {
            DiscriminatorValue(GlobalConstants.DataSourceDiscriminator.GOOGLE_BIG_QUERY_API_SOURCE);
        }
    }
}
