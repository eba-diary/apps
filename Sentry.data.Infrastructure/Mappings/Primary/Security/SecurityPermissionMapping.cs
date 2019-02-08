using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class SecurityPermissionMapping : ClassMapping<SecurityPermission>
    {

        public SecurityPermissionMapping()
        {
            this.Table("SecurityPermission");

            this.Cache(c => c.Usage(CacheUsage.ReadWrite));

            this.Id(x => x.SecurityPermissionId, m =>
            {
                m.Column("SecurityPermission_ID");
                m.Generator(Generators.GuidComb);
            });

            this.Property((x) => x.IsEnabled, (m) => m.Column("IsEnabled_IND"));
            this.Property((x) => x.AddedDate, (m) => m.Column("Added_DTM"));
            this.Property((x) => x.EnabledDate, (m) => m.Column("Enabled_DTM"));
            this.Property((x) => x.RemovedDate, (m) => m.Column("Removed_DTM"));



            this.ManyToOne(x => x.AddedFromTicket, m =>
            {
                m.Column("AddedFromTicket_ID");
                m.ForeignKey("FK_AddedSecurityPermission_SecurityTicket");
                m.Class(typeof(SecurityTicket));
                m.Cascade(Cascade.All);
            });

            this.ManyToOne(x => x.RemovedFromTicket, m =>
            {
                m.Column("RemovedFromTicket_ID");
                m.ForeignKey("FK_RemovedSecurityPermission_SecurityTicket");
                m.Class(typeof(SecurityTicket));
                m.Cascade(Cascade.All);
            });

            this.ManyToOne(x => x.Permission, m =>
            {
                m.Column("Permission_ID");
                m.ForeignKey("FK_SecurityPermission_Permission");
                m.Class(typeof(Permission));
            });

        }
    }
}
