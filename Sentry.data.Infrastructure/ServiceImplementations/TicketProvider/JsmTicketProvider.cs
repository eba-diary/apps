using Sentry.ChangeManagement;
using Sentry.data.Core;
using System;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class JsmTicketProvider : ITicketProvider
    {
        private readonly ChangeManagementClient _changeManagementClient;

        public JsmTicketProvider(ChangeManagementClient changeManagementClient)
        {
            _changeManagementClient = changeManagementClient;
        }

        public async Task<string> CreateTicketAsync(AccessRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<ChangeTicket> RetrieveTicketAsync(string ticketId)
        {
            throw new NotImplementedException();
        }

        public async Task CloseTicketAsync(string ticketId)
        {
            throw new NotImplementedException();
        }
    }
}
