using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class HpsmTicket
    {

        public string TicketStatus { get; set; }
        public string ApprovedById { get; set; }
        public string RejectedById { get; set; }

    }
}
