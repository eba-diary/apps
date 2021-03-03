using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sentry.Configuration;
using Sentry.data.Core.Exceptions;

namespace Sentry.data.Core
{
    public class SecurityService : ISecurityService
    {

        private readonly IDatasetContext _datasetContext;
        //BaseTicketProvider implementation is determined within Bootstrapper and could be either ICherwellProvider or IHPSMProvider
        private readonly IBaseTicketProvider _baseTicketProvider;

        public SecurityService(IDatasetContext datasetContext, IBaseTicketProvider baseTicketProvider)
        {
            _datasetContext = datasetContext;
            _baseTicketProvider = baseTicketProvider;
        }

        public string RequestPermission(AccessRequest model)
        {
            string ticketId = _baseTicketProvider.CreateChangeTicket(model);
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
            //Admins also get all the permissions.
            bool IsAdmin = user.IsAdmin;
            bool IsOwner = (user.AssociateId == securable?.PrimaryOwnerId || user.AssociateId == securable?.PrimaryContactId);
            List<string> userPermissions = new List<string>();

            //set the user based permissions based off obsidian and ownership
            UserSecurity us = new UserSecurity()
            {
                CanEditDataset = (user.CanModifyDataset && IsOwner) || IsAdmin,
                CanCreateDataset = user.CanModifyDataset || IsAdmin,
                CanEditReport = user.CanManageReports || IsAdmin,
                CanCreateReport = user.CanManageReports || IsAdmin,
                CanEditDataSource = (user.CanModifyDataset && IsOwner) || IsAdmin,
                CanCreateDataSource = user.CanModifyDataset || IsAdmin,
                ShowAdminControls = IsAdmin,
                CanCreateDataFlow = user.CanModifyDataset || IsAdmin,
                CanModifyDataflow = user.CanModifyDataset || IsOwner || IsAdmin
            };

            //if no tickets have been requested, then there should be no permission given.
            if (securable?.Security?.Tickets != null && securable.Security.Tickets.Count > 0)
            {
                //build a adGroupName and  List(of permissionCode) anonymous obj.
                var adGroups = securable.Security.Tickets.Select(x => new { adGroup = x.AdGroupName, permissions = x.Permissions.Where(y => y.IsEnabled).ToList() }).Where(x=> x.adGroup != null).ToList();
                //loop through the dictionary to see if the user is part of the group, if so grab the permissions.
                foreach (var item in adGroups)
                {
                    if (user.IsInGroup(item.adGroup))
                    {
                        userPermissions.AddRange(item.permissions.Select(x => x.Permission.PermissionCode).ToList());
                    }
                }

                //build a userId and  List(of permissionCode) anonymous obj.
                var userGroups = securable.Security.Tickets.Select(x => new { userId = x.GrantPermissionToUserId, permissions = x.Permissions.Where(y => y.IsEnabled).ToList() }).Where(x=> x.userId != null).ToList();
                //loop through the dictionary to see if the user is the user on the ticket, if so grab the permissions.
                foreach (var item in userGroups)
                {
                    if (item.userId == user.AssociateId)
                    {
                        userPermissions.AddRange(item.permissions.Select(x => x.Permission.PermissionCode).ToList());
                    }
                }
            }

            //if it is not secure, it should be wide open except for upload and notifications. call everything out for visibility.
            if (securable == null || securable.Security == null || !securable.IsSecured)
            {
                us.CanPreviewDataset = true;
                us.CanQueryDataset = true;
                us.CanViewFullDataset = true;
                us.CanUploadToDataset = IsOwner || IsAdmin;
                us.CanModifyNotifications = false;
                us.CanUseDataSource = true;
                //us.CanManageSchema = (userPermissions.Count > 0) ? ((user.CanModifyDataset && (userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_MANAGE_SCHEMA) || IsOwner)) || IsAdmin) : (IsOwner || IsAdmin);
                us.CanManageSchema = (userPermissions.Count > 0) ? userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_MANAGE_SCHEMA) || (IsOwner && user.CanModifyDataset) || IsAdmin : ((IsOwner && user.CanModifyDataset) || IsAdmin);
                return us;
            }

            //from the list of permissions, build out the security object.
            us.CanPreviewDataset = userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_PREVIEW_DATASET) || IsOwner || IsAdmin;
            us.CanViewFullDataset = userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_VIEW_FULL_DATASET) || IsOwner || IsAdmin;
            us.CanQueryDataset = userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_QUERY_DATASET) || IsOwner || IsAdmin;
            us.CanUploadToDataset = userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_UPLOAD_TO_DATASET) || IsOwner || IsAdmin;
            us.CanModifyNotifications = userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_MODIFY_NOTIFICATIONS) || IsOwner || IsAdmin;
            us.CanUseDataSource = userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_USE_DATA_SOURCE) || IsOwner || IsAdmin;
            //us.CanManageSchema = (user.CanModifyDataset && (userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_MANAGE_SCHEMA) || IsOwner)) || IsAdmin;
            us.CanManageSchema = userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_MANAGE_SCHEMA) || (IsOwner && user.CanModifyDataset) || IsAdmin;

            return us;
        }

        /// <summary>
        /// Returns count of ad groups with access to securable
        /// </summary>
        /// <param name="securable"></param>
        /// <returns></returns>
        public int GetGroupAccessCount(ISecurable securable)
        {
            if (securable.Security?.Tickets != null)
            {
                var groups = securable.Security.Tickets.Select(x => new { adGroup = x.AdGroupName, permissions = x.Permissions.Where(y => y.IsEnabled).ToList() }).ToList();
                return groups.Select(s => s.adGroup).Distinct().Count();
            }
            else
            {
                return 0;                
            }
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
