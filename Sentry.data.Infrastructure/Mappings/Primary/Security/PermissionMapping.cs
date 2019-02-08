using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class PermissionMapping : ClassMapping<Permission>
    {

        public PermissionMapping()
        {

            this.Table("Permission");

            this.Cache(c => c.Usage(CacheUsage.ReadWrite));

            this.Id(x => x.PermissionId, m =>
            {
                m.Column("Permission_ID");
                m.Generator(Generators.Identity);
            });

            this.Property((x) => x.PermissionCode, (m) => m.Column("Permission_CDE"));
            this.Property((x) => x.PermissionDescription, (m) => m.Column("Permission_DSC"));
            this.Property((x) => x.PermissionName, (m) => m.Column("Permission_NME"));
            this.Property((x) => x.SecurableObject, (m) => m.Column("SecurableObject_TYP"));

        }

    }
}
