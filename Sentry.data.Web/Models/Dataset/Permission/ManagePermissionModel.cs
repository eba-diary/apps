using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class ManagePermissionModel
    {
        public ManagePermissionModel(SecurablePermission securablePermission, string datasetName, string datasetSaidKeyCode)
        {
            Scope = securablePermission.Scope == SecurablePermissionScope.Self ? datasetName : datasetSaidKeyCode.ToUpper() + " (inherited)";
            Identity = securablePermission.Identity;
            PermissionDescription = securablePermission.SecurityPermission.Permission.PermissionDescription;
            PermissionStatus = securablePermission.SecurityPermission.IsEnabled ? "Active" : "Pending";
            TicketId = securablePermission.TicketId;
        }

        public string Scope { get; set; }
        public string Identity { get; set; }
        public string PermissionDescription { get; set; }
        public string PermissionStatus { get; set; }
        public string TicketId { get; set; }
    }
}