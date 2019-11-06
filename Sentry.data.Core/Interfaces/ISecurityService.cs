
using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface ISecurityService
    {
        string RequestPermission(AccessRequest model);
        UserSecurity GetUserSecurity(ISecurable securable, IApplicationUser user);
        int GetGroupAccessCount(ISecurable securable);
        void ApproveTicket(SecurityTicket ticket, string approveId);
        void CloseTicket(SecurityTicket ticket, string RejectorId, string rejectedReason, string status);
    }
}
