using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class AuthenticationTypeMapping : ClassMapping<AuthenticationType>
    {
        public AuthenticationTypeMapping()
        {
            Table("AuthenticationType");

            Id(x => x.AuthID, m =>
            {
                m.Column("Auth_Id");
                m.Generator(Generators.Identity);
            });

            Discriminator(x => x.Column("AuthType_CDE"));

            Property(x => x.AuthName, m =>
            {
                m.Column("Display_NME");
                m.NotNullable(true);
            });

            Property(x => x.Description, m =>
            {
                m.Column("Description");
                m.NotNullable(true);
            });
        }
    }

    //http://notherdev.blogspot.com/2012/01/mapping-by-code-inheritance.html

    public class AnonymousAuthenticationMapping : SubclassMapping<AnonymousAuthentication>
    {
        public AnonymousAuthenticationMapping()
        {
            DiscriminatorValue(@"AnonAuth");
        }
    }

    public class BasicAuthenticationMapping : SubclassMapping<BasicAuthentication>
    {
        public BasicAuthenticationMapping()
        {
            DiscriminatorValue(@"BasicAuth");
        }
    }
}
