using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IBaseTicketProvider
    {
        string CreateChangeTicket(AccessRequest model);
        HpsmTicket RetrieveTicket(string ticketId);
        void CloseTicket(string ticketId, bool wasTicketDenied = false);
    }
}
