﻿
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface ISecurityService
    {
        Task<string> RequestPermission(AccessRequest model);
        UserSecurity GetUserSecurity(ISecurable securable, IApplicationUser user);
        int GetGroupAccessCount(ISecurable securable);
        Task ApproveTicket(SecurityTicket ticket, string approveId);
        void CloseTicket(SecurityTicket ticket, string RejectorId, string rejectedReason, string status);

        /// <summary>
        /// Retrieve all the permissions granted to the provided <see cref="ISecurable"/>.
        /// </summary>
        IList<SecurablePermission> GetSecurablePermissions(ISecurable securable);
        SecurityTicket GetSecurableInheritanceTicket(ISecurable securable);
        void BuildS3RequestAssistance(SecurityTicket ticket);

    }
}
