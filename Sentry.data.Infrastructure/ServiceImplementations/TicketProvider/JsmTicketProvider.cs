using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class JsmTicketProvider : ITicketProvider
    {
        public string CreateTicket(AccessRequest request)
        {
            throw new NotImplementedException();
        }

        public ChangeTicket RetrieveTicket(string ticketId)
        {
            throw new NotImplementedException();
        }
        public void CloseTicket(string ticketId)
        {
            throw new NotImplementedException();
        }
    }
}
