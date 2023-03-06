using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;
using NHibernate;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class DataSourceTokenMapping : ClassMapping<DataSourceToken>
    {
        public DataSourceTokenMapping()
        {
            Table("DataSourceTokens");

            Id(x => x.Id, m =>
            {
                m.Column("Id");
                m.Generator(Generators.Identity);
            });

            this.ManyToOne(x => x.ParentDataSource, m =>
            {
                m.Column("ParentDataSource_Id");
                m.ForeignKey("FK_DataSourceTokens_DataSource");
                m.Class(typeof(DataSource));
            });

            Property(x => x.TokenName, m =>
            {
                m.Column("TokenName");
                m.NotNullable(false);
            });

            Property(x => x.RefreshToken, m =>
            {
                m.Column("RefreshToken");
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
                m.Table("AuthenticationClaims");
                m.Cascade(Cascade.All);
                m.Key((k) =>
                {
                    k.Column("Token_Id");
                    k.ForeignKey("FK_AuthenticationClaims_DataSourceTokens");
                });
            }, map => map.OneToMany(a => a.Class(typeof(OAuthClaim))));

            Property(x => x.Enabled, m =>
            {
                m.Column("Enabled");
                m.NotNullable(false);
            });
        }
    }
}
