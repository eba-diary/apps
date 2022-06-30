using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    /// <summary>
    /// A class used as the return type for <see cref="SecurityService.GetSecurablePermissions(ISecurable)"/>.
    /// </summary>
    public class SecurablePermission
    {
        public SecurablePermissionScope Scope { get; set; }
        public Security ScopeSecurity { get; set; }
        public string Identity { get; set; }
        public string IdentityType { get; set; }
        public SecurityPermission SecurityPermission { get; set; }
        public string TicketId { get; set; }
        public bool IsSystemGenerated { get; set; }
    }
}
