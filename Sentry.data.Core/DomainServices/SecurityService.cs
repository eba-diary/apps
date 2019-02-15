using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sentry.data.Core
{
    public class SecurityService : ISecurityService
    {

        private readonly IDatasetContext _datasetContext;
        private readonly IHpsmProvider _hpsmProvider;

        public SecurityService(IDatasetContext datasetContext, IHpsmProvider hpsmProvider)
        {
            _datasetContext = datasetContext;
            _hpsmProvider = hpsmProvider;
        }


        public string RequestPermission(AccessRequest model)
        {
            //Lets format the business reason here before passing it into the hpsm service.
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(model.AdGroupName))
            {
                sb.Append($"Please grant the Ad Group {model.AdGroupName} the following permissions for Data.sentry.com.{ Environment.NewLine}");
            }
            else
            {
                sb.Append($"Please grant user {model.PermissionForUserName} ({model.PermissionForUserId}) the following permissions for Data.sentry.com.{ Environment.NewLine}");
            }
            model.Permissions.ForEach(x => sb.Append($"{x.PermissionName} - {x.PermissionDescription} { Environment.NewLine}"));
            sb.Append($"Business Reason: {model.BusinessReason}{ Environment.NewLine}");
            sb.Append($"Requestor: {model.RequestorsId} - {model.RequestorsName}");

            model.BusinessReason = sb.ToString();

            string ticketId = _hpsmProvider.CreateHpsmTicket(model);
            if (!string.IsNullOrWhiteSpace(ticketId))
            {
                Security security = _datasetContext.Security.FirstOrDefault(x => x.SecurityId == model.SecurityId);

                SecurityTicket ticket = new SecurityTicket()
                {
                    TicketId = ticketId,
                    AdGroupName = model.AdGroupName,
                    GrantPermissionToUserId = model.PermissionForUserId,
                    TicketStatus = GlobalConstants.HpsmTicketStatus.PENDING,
                    RequestedById = model.RequestorsId,
                    RequestedDate = model.RequestedDate,
                    IsAddingPermission = true,
                    ParentSecurity = security,
                    Permissions = new List<SecurityPermission>()
                };

                foreach (Permission perm in model.Permissions)
                {
                    ticket.Permissions.Add(new SecurityPermission()
                    {
                        AddedDate = DateTime.Now,
                        Permission = perm,
                        AddedFromTicket = ticket,
                        IsEnabled = false
                    });
                }

                security.Tickets.Add(ticket);
                _datasetContext.SaveChanges();

                return ticketId;
            }
            return string.Empty;
        }


        public UserSecurity GetUserSecurity(ISecurable securable, IApplicationUser user)
        {
            //If the user is nothing for some reason, absolutly no permissions should be returned.
            if(user == null) { return new UserSecurity(); }

            //if the user is one of the primary owners or primary contact, they should have all permissions without even requesting it.
            bool IsOwner = (user.AssociateId == securable?.PrimaryOwnerId || user.AssociateId == securable?.PrimaryContactId);
            List<string> userPermissions = new List<string>();

            //set the user based permissions based off obsidian and ownership
            UserSecurity us = new UserSecurity()
            {
                CanEditDataset = user.CanModifyDataset && IsOwner,
                CanCreateDataset = user.CanModifyDataset,
                CanEditReport = user.CanManageReports && IsOwner,
                CanCreateReport = user.CanManageReports,
            };

            //if it is not secure, it should be wide open except for upload and notifications. call everything out for visibility.
            if (securable == null || securable.Security == null || !securable.IsSecured)
            {
                us.CanPreviewDataset = true;
                us.CanQueryDataset = true;
                us.CanViewFullDataset = true;
                us.CanUploadToDataset = IsOwner;
                us.CanModifyNotifications = false;
                return us;
            }

            //if no tickets have been requested, then there should be no permission given.
            if (securable?.Security?.Tickets != null && securable.Security.Tickets.Count > 0)
            {
                //build a adGroupName and  List(of permissionCode) anonymous obj.
                var adGroups = securable.Security.Tickets.Select(x => new { adGroup = x.AdGroupName, permissions = x.Permissions.Where(y => y.IsEnabled).ToList() }).ToList();
                //loop through the dictionary to see if the user is part of the group, if so grab the permissions.
                foreach (var item in adGroups)
                {
                    if (user.IsInGroup(item.adGroup))
                    {
                        userPermissions.AddRange(item.permissions.Select(x => x.Permission.PermissionCode).ToList());
                    }
                }

                //build a userId and  List(of permissionCode) anonymous obj.
                var userGroups = securable.Security.Tickets.Select(x => new { userId = x.GrantPermissionToUserId, permissions = x.Permissions.Where(y => y.IsEnabled).ToList() }).ToList();
                //loop through the dictionary to see if the user is the user on the ticket, if so grab the permissions.
                foreach (var item in userGroups)
                {
                    if (item.userId == user.AssociateId)
                    {
                        userPermissions.AddRange(item.permissions.Select(x => x.Permission.PermissionCode).ToList());
                    }
                }
            }

            //from the list of permissions, build out the security object.
            us.CanPreviewDataset = userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_PREVIEW_DATASET) || IsOwner;
            us.CanViewFullDataset = userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_VIEW_FULL_DATASET) || IsOwner;
            us.CanQueryDataset = userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_QUERY_DATASET) || IsOwner;
            us.CanUploadToDataset = userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_UPLOAD_TO_DATASET) || IsOwner;
            us.CanModifyNotifications = userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_MODIFY_NOTIFICATIONS) || IsOwner;

            return us;
        }


        public void ApproveTicket(SecurityTicket ticket, string approveId)
        {
            ticket.ApprovedById = approveId;
            ticket.ApprovedDate = DateTime.Now;
            ticket.TicketStatus = GlobalConstants.HpsmTicketStatus.COMPLETED;

            ticket.Permissions.ToList().ForEach(x =>
            {
                x.IsEnabled = true;
                x.EnabledDate = DateTime.Now;
            });
        }

        public void CloseTicket(SecurityTicket ticket, string RejectorId, string rejectedReason, string status)
        {
            ticket.RejectedById = RejectorId;
            ticket.RejectedDate = DateTime.Now;
            ticket.RejectedReason = rejectedReason;
            ticket.TicketStatus = status;
        }

    }
}
