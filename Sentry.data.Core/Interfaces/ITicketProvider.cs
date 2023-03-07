using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface ITicketProvider
    {
        string CreateTicket(AccessRequest request);
        ChangeTicket RetrieveTicket(string ticketId);
        void CloseTicket(string ticketId);
    }
}
