using Sentry.ChangeManagement;
using Sentry.ChangeManagement.Sentry;
using Sentry.data.Core;
using System;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class JsmTicketProvider : ITicketProvider
    {
        private readonly ISentryChangeManagementClient _changeManagementClient;

        public JsmTicketProvider(ISentryChangeManagementClient changeManagementClient)
        {
            _changeManagementClient = changeManagementClient;
        }

        public async Task<string> CreateTicketAsync(AccessRequest request)
        {
            //create ticket

            //move phase

            //return ticket id
            throw new NotImplementedException();
        }

        public async Task<ChangeTicket> RetrieveTicketAsync(string ticketId)
        {
            //get ticket
            SentryChange change = await _changeManagementClient.GetChange(ticketId);

            //translate to ChangeTicket

            //return ChangeTicket
            throw new NotImplementedException();

        }

        public async Task CloseTicketAsync(ChangeTicket ticket)
        {
            //move phase to closed
            throw new NotImplementedException();
        }

        #region Private
        
        #endregion
    }
}
