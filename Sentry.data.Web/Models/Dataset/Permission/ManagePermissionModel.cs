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
            if (!securablePermission.SecurityPermission.IsEnabled && !string.IsNullOrEmpty(securablePermission.TicketStatus))
            {
                switch (securablePermission.TicketStatus) 
                {
                    case GlobalConstants.ChangeTicketStatus.PENDING:
                        PermissionStatus = "Pending Approval";
                        break;
                    case GlobalConstants.ChangeTicketStatus.DbaTicketPending:
                        PermissionStatus = "DBA Ticket Pending Creation";
                        break;
                    case GlobalConstants.ChangeTicketStatus.DbaTicketAdded:
                        PermissionStatus = "DBA Ticket Created";
                        break;
                    case GlobalConstants.ChangeTicketStatus.DbaTicketApproved:
                        PermissionStatus = "DBA Ticket Approved";
                        break;
                    case GlobalConstants.ChangeTicketStatus.DbaTicketComplete:
                        PermissionStatus = "DBA Ticket Complete";
                        break;
                    default:
                        PermissionStatus = "Pending";
                        break;
                }
            }
            else if (securablePermission.SecurityPermission.IsEnabled)
            {
                PermissionStatus = "Active";
            }
            else
            {
                PermissionStatus = "Pending";
            }
            TicketId = securablePermission.TicketId;
            TicketIdUrl = Helpers.JSMHelper.GetJsmTicketUrl(securablePermission.TicketId);
            ExternalRequestId = securablePermission.ExternalRequestId;
            ExternalRequestIdUrl = Helpers.JiraHelper.GetJiraTicketUrl(securablePermission.ExternalRequestId);
            Code = securablePermission.SecurityPermission.Permission.PermissionCode;
            IsSystemGenerated = securablePermission.IsSystemGenerated;
        }

        public ManagePermissionModel() { }

        public string Scope { get; set; }
        public string Identity { get; set; }
        public string PermissionDescription { get; set; }
        public string PermissionStatus { get; set; }
        public string TicketId { get; set; }
        public string TicketIdUrl { get; set; }
        public string ExternalRequestId { get; set; }
        public string ExternalRequestIdUrl { get; set; }
        public string TicketStatus { get; set; }
        public string Code { get; set; }
        public bool IsSystemGenerated { get; set; }
    }
}