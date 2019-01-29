
using System;

namespace Sentry.data.Core
{
    public interface ISecurityService
    {

        void CheckHpsmTicketStatus();
        bool RequestPermission(RequestAccess model);
        void RemovePermissions(Guid hpsmTicketId);

        UserSecurity GetUserSecurity(Security security);
    }
}
