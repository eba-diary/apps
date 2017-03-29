using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class UserMapping : ClassMapping<DomainUser>
    {
        public UserMapping()
        {
            this.Table("[User]");

            this.Id((x) => x.Id, (m) =>
            {
                m.Access(Accessor.Field);
                m.Generator(Generators.Identity);
            });

            this.Property((x) => x.AssociateId, (m) => m.Access(Accessor.Field));

            //###  BEGIN Sentry.Data  A### - Code below is Sentry.Data-specific
            this.Version((x) => x.Version, (m) => m.Access(Accessor.Field));

            this.Property((x) => x.Created, (m) => m.Access(Accessor.Field));

            this.Property((x) => x.Ranking);

            //###  END Sentry.Data  ### - Code above is Sentry.Data-specific
        }
    }
}
