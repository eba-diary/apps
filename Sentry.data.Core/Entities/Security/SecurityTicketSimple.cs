using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core.Entities.Security
{
    public class SecurityTicketSimple
    {
        public SecurityTicketSimple() { }
        public SecurityTicketSimple(SecurityTicket ticket)
        {
            TicketId = ticket.TicketId;
            TicketStatus = ticket.TicketStatus;
            InheritanceActive = ticket.AddedPermissions != null ? ticket.AddedPermissions.Any(p => p.IsEnabled && p.Permission.PermissionCode == PermissionCodes.INHERIT_PARENT_PERMISSIONS) : false;
        }

        public string TicketId { get; set; }
        public string TicketStatus { get; set; }
        public bool InheritanceActive { get; set; }

    }
}
