using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary.Retriever
{
    public class OAuthClaimTypeMapping : ClassMapping<OAuthClaim>
    {
        public OAuthClaimTypeMapping()
        {
            Table("AuthenticationClaims");

            Id(x => x.Id, m =>
            {
                m.Column("Id");
                m.Generator(Generators.Identity);
            });

            Property(x => x.Type, m => m.Column("Name"));
            Property(x => x.Value, m => m.Column("Value"));
        }
    }
}
