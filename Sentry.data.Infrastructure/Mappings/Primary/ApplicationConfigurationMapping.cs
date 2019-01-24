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
    public class ApplicationConfigurationMapping : ClassMapping<ApplicationConfiguration>
    {
        public ApplicationConfigurationMapping()
        {
            this.Table("ApplicationConfiguration");

            this.Id((x) => x.Id, (m) =>
            {
                m.Column("AppConfig_ID");
                m.Generator(Generators.Identity);
            });
            this.Property((x) => x.Application, m =>
            {
                m.Column("Application");
            });

            this.Property((x) => x.Options, m =>
            {
                m.Column("Options");
            });

            this.Property((x) => x.Created, m =>
            {
                m.Column("Created");
            });

            this.Property((x) => x.Modified, m =>
            {
                m.Column("Modified");
            });
        }
    }
}
