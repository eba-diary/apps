using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class SecurityTicketMapping : ClassMapping<SecurityTicket>
    {

        public SecurityTicketMapping()
        {

            this.Table("SecurityTicket");

            this.Cache(c => c.Usage(CacheUsage.ReadWrite));

            this.Id(x => x.SecurityTicketId, m =>
            {
                m.Column("SecurityTicket_ID");
                m.Generator(Generators.GuidComb);
            });

            this.Property((x) => x.AdGroupName, (m) => m.Column("AdGroup_NME"));
            this.Property((x) => x.ApprovedById, (m) => m.Column("ApprovedBy_ID"));
            this.Property((x) => x.ApprovedDate, (m) => m.Column("Approved_DTM"));
            this.Property((x) => x.IsAddingPermission, (m) => m.Column("IsAddingPermission_IND"));
            this.Property((x) => x.IsRemovingPermission, (m) => m.Column("IsRemovingPermission_IND"));
            this.Property((x) => x.RejectedById, (m) => m.Column("RejectedBy_ID"));
            this.Property((x) => x.RejectedDate, (m) => m.Column("Rejected_DTM"));
            this.Property((x) => x.RequestedById, (m) => m.Column("RequestedBy_ID"));
            this.Property((x) => x.RequestedDate, (m) => m.Column("Requested_DTM"));
            this.Property((x) => x.TicketId, (m) => m.Column("Ticket_ID"));
            this.Property((x) => x.TicketStatus, (m) => m.Column("TicketStatus_DSC"));
            this.Property((x) => x.RejectedReason, (m) => m.Column("Rejected_DSC"));
            this.Property((x) => x.GrantPermissionToUserId, (m) => m.Column("GrantPermissionToUser_ID"));
            this.Property((x) => x.AwsArn);

            this.Bag(x => x.AddedPermissions, (m) =>
            {
                m.Inverse(true);
                m.Table("SecurityPermission");
                m.Cascade(Cascade.All);
                m.Fetch(CollectionFetchMode.Select);
                m.Key((k) =>
                {
                    k.Column("AddedFromTicket_ID");
                    k.ForeignKey("FK_AddedSecurityPermission_SecurityTicket");
                });
            }, map => map.OneToMany(a => a.Class(typeof(SecurityPermission))));
            
            this.Bag(x => x.RemovedPermissions, (m) =>
            {
                m.Inverse(true);
                m.Table("SecurityPermission");
                m.Cascade(Cascade.All);
                m.Fetch(CollectionFetchMode.Select);
                m.Key((k) =>
                {
                    k.Column("RemovedFromTicket_ID");
                    k.ForeignKey("FK_RemovedSecurityPermission_SecurityTicket");
                });
            }, map => map.OneToMany(a => a.Class(typeof(SecurityPermission))));

            this.ManyToOne(x => x.ParentSecurity, m =>
            {
                m.Column("Security_ID");
                m.ForeignKey("FK_SecurityTicket_Security");
                m.Class(typeof(Security));
            });
        }

    }
}
