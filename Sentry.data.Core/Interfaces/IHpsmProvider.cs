
using System;

namespace Sentry.data.Core
{
    public interface IHpsmProvider
    {

        string CreateHpsmTicket(AccessRequest model);
        HpsmTicket RetrieveTicket(string hpsmChangeId);
        void CloseHpsmTicket(string hpsmChangeId, bool wasTicketDenied = false);

    }
}
