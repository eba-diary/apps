using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface ITicketProvider
    {
        Task<string> CreateTicketAsync(AccessRequest request);
        Task<ChangeTicket> RetrieveTicketAsync(string ticketId);
        Task CloseTicketAsync(string ticketId);
    }
}
