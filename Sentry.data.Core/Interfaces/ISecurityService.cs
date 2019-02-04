
using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface ISecurityService
    {

        void CheckHpsmTicketStatus();
        string RequestPermission(AccessRequest model);

        UserSecurity GetUserSecurity(ISecurable securable, IApplicationUser user);
    }
}
