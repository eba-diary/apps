using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.Security
{
    public class SecurityTicketSimple
    {
        public SecurityTicketSimple() { }
        public SecurityTicketSimple(SecurityTicket ticket)
        {
            TicketId = ticket.TicketId;
            TicketStatus = ticket.TicketStatus;
        }

        public string TicketId { get; set; }
        public string TicketStatus { get; set; }

    }
}
