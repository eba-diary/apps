
namespace Sentry.data.Core
{
    public interface IHpsmProvider
    {

        string CreateHpsmTicket(RequestAccess model);
        HpsmTicket RetrieveTicket(string hpsmChangeId);
        bool CloseHpsmTicket(string hpsmChangeId, bool wasTicketDenied = false);

    }
}
