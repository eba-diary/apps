using System;
using System.Collections.Generic;
using System.Linq;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core
{
    public class SecurityService : ISecurityService
    {

        private readonly IDatasetContext _datasetContext;
        //BaseTicketProvider implementation is determined within Bootstrapper and could be either ICherwellProvider or IHPSMProvider
        private readonly IBaseTicketProvider _baseTicketProvider;
        private readonly IDataFeatures _dataFeatures;

        public SecurityService(IDatasetContext datasetContext, IBaseTicketProvider baseTicketProvider, IDataFeatures dataFeatures)
        {
            _datasetContext = datasetContext;
            _baseTicketProvider = baseTicketProvider;
            _dataFeatures = dataFeatures;
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

        /// <summary>
        /// Get's a user's permissions for a securable entity
        /// </summary>
        /// <param name="securable">The securable entity we're going to check for permissions</param>
        /// <param name="user">The user whose permissions you want to check</param>
        public UserSecurity GetUserSecurity(ISecurable securable, IApplicationUser user)
        {
            // call different implementations based on the feature flag
            return _dataFeatures.CLA3861_RefactorGetUserSecurity.GetValue()
                ? GetUserSecurity_Internal(securable, user)
                : GetUserSecurity_Internal_Original(securable, user);
        }

        private UserSecurity GetUserSecurity_Internal_Original(ISecurable securable, IApplicationUser user)
        {
            //If the user is nothing for some reason, absolutly no permissions should be returned.
            if (user == null) { return new UserSecurity(); }

            //if the user is one of the primary owners or primary contact, they should have all permissions without even requesting it.
            //Admins also get all the permissions, except if the securable is sensitive (ie HR) 

            bool IsAdmin = user.IsAdmin;
            bool IsOwner = (user.AssociateId == securable?.PrimaryContactId) && user.CanModifyDataset;
            List<string> userPermissions = new List<string>();

            //set the user based permissions based off obsidian and ownership
            UserSecurity us = new UserSecurity()
            {
                CanEditDataset = IsOwner || IsAdmin,
                CanCreateDataset = user.CanModifyDataset || IsAdmin,
                CanEditReport = user.CanManageReports || IsAdmin,
                CanCreateReport = user.CanManageReports || IsAdmin,
                CanEditDataSource = IsOwner || IsAdmin,
                CanCreateDataSource = user.CanModifyDataset || IsAdmin,
                ShowAdminControls = IsAdmin,
                CanCreateDataFlow = user.CanModifyDataset || IsAdmin,
                CanModifyDataflow = user.CanModifyDataset || IsOwner || IsAdmin
            };

            //if no tickets have been requested, then there should be no permission given.
            if (securable?.Security?.Tickets != null && securable.Security.Tickets.Count > 0)
            {
                //build a adGroupName and  List(of permissionCode) anonymous obj.
                var adGroups = securable.Security.Tickets.Select(x => new { adGroup = x.AdGroupName, permissions = x.Permissions.Where(y => y.IsEnabled).ToList() }).Where(x => x.adGroup != null).ToList();
                //loop through the dictionary to see if the user is part of the group, if so grab the permissions.
                foreach (var item in adGroups)
                {
                    if (user.IsInGroup(item.adGroup))
                    {
                        userPermissions.AddRange(item.permissions.Select(x => x.Permission.PermissionCode).ToList());
                    }
                }

                //build a userId and  List(of permissionCode) anonymous obj.
                var userGroups = securable.Security.Tickets.Select(x => new { userId = x.GrantPermissionToUserId, permissions = x.Permissions.Where(y => y.IsEnabled).ToList() }).Where(x => x.userId != null).ToList();
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
                us.CanManageSchema = (userPermissions.Count > 0) ? userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_MANAGE_SCHEMA) || IsOwner || IsAdmin : (IsOwner || IsAdmin);
                return us;
            }

            //from the list of permissions, build out the security object.
            us.CanPreviewDataset = userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_PREVIEW_DATASET) || IsOwner || IsAdmin;
            us.CanViewFullDataset = userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_VIEW_FULL_DATASET) || IsOwner || (IsAdmin && !securable.IsSensitive);
            us.CanQueryDataset = userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_QUERY_DATASET) || IsOwner || IsAdmin;
            us.CanUploadToDataset = userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_UPLOAD_TO_DATASET) || IsOwner || IsAdmin;
            us.CanModifyNotifications = userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_MODIFY_NOTIFICATIONS) || IsOwner || IsAdmin;
            us.CanUseDataSource = userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_USE_DATA_SOURCE) || IsOwner || IsAdmin;
            //us.CanManageSchema = (user.CanModifyDataset && (userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_MANAGE_SCHEMA) || IsOwner)) || IsAdmin;
            us.CanManageSchema = userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_MANAGE_SCHEMA) || IsOwner || IsAdmin;

            return us;
        }

        /// <summary>
        /// Get's a user's permissions for a securable entity. Refactored to be more concise.
        /// </summary>
        /// <param name="securable">The securable entity we're going to check for permissions</param>
        /// <param name="user">The user whose permissions you want to check</param>
        private UserSecurity GetUserSecurity_Internal(ISecurable securable, IApplicationUser user)
        {
            //If the user is nothing for some reason, absolutly no permissions should be returned.
            if (user == null)
            {
                return new UserSecurity();
            }

            //if the user is one of the primary owners or primary contact, they should have all permissions without even requesting it.
            //Admins also get all the permissions, except if the securable is sensitive (ie HR) 
            bool IsAdmin = user.IsAdmin;
            bool IsOwner = (user.AssociateId == securable?.PrimaryContactId) && user.CanModifyDataset;
            List<string> userPermissions = new List<string>();

            //set the user based permissions based off obsidian and ownership
            UserSecurity us = new UserSecurity();
            BuildOutUserSecurityFromObsidian(IsAdmin, IsOwner, user, us);

            //if no tickets have been requested, then there should be no permission given.
            UserSecurity parentSecurity = null;
            if (securable?.Security?.Tickets != null && securable.Security.Tickets.Count > 0)
            {
                //does this securable have the "Inherit Parent Permissions" permission?
                var inheritanceTicket = securable.Security.Tickets.FirstOrDefault(t => t.Permissions.Any(p => p.IsEnabled && p.Permission.PermissionCode == PermissionCodes.INHERIT_PARENT_PERMISSIONS));
                if (inheritanceTicket != null && securable.Parent != null)
                {
                    parentSecurity = GetUserSecurity(securable.Parent, user);
                }

                //build a adGroupName and List(of permissionCode) anonymous obj.
                var adGroups = securable.Security.Tickets.Select(x => new { adGroup = x.AdGroupName, permissions = x.Permissions.Where(y => y.IsEnabled).ToList() }).Where(x => x.adGroup != null).ToList();
                //loop through the dictionary to see if the user is part of the group, if so grab the permissions.
                userPermissions.AddRange(adGroups.Where(g => user.IsInGroup(g.adGroup)).SelectMany(g => g.permissions).Select(p => p.Permission.PermissionCode));

                //build a userId and List(of permissionCode) anonymous obj.
                var userGroups = securable.Security.Tickets.Select(x => new { userId = x.GrantPermissionToUserId, permissions = x.Permissions.Where(y => y.IsEnabled).ToList() }).Where(x => x.userId != null).ToList();
                //loop through the dictionary to see if the user is the user on the ticket, if so grab the permissions.
                userPermissions.AddRange(userGroups.Where(g => g.userId == user.AssociateId).SelectMany(g => g.permissions).Select(p => p.Permission.PermissionCode));
            }

            //if it is not secure, it should be wide open except for upload and notifications
            if (securable == null || securable.Security == null || !securable.IsSecured)
            {
                BuildOutUserSecurityForUnsecuredEntity(IsAdmin, IsOwner, userPermissions, us, parentSecurity);
            }
            else
            {
                BuildOutUserSecurityForSecuredEntity(IsAdmin, IsOwner, userPermissions, us, parentSecurity, securable);
            }
            return us;
        }

        /// <summary>
        /// Populate the portions of the UserSecurity object based on Obsidian permissions
        /// </summary>
        /// <param name="IsAdmin">If the current user is a DSC admin</param>
        /// <param name="IsOwner">If the current user is the owner of the securable</param>
        /// <param name="user">The user object</param>
        /// <param name="us">The user security object to populate</param>
        internal static void BuildOutUserSecurityFromObsidian(bool IsAdmin, bool IsOwner, IApplicationUser user, UserSecurity us)
        {
            //set the user based permissions based off obsidian and ownership
            us.CanEditDataset = IsOwner || IsAdmin;
            us.CanCreateDataset = user.CanModifyDataset || IsAdmin;
            us.CanEditReport = user.CanManageReports || IsAdmin;
            us.CanCreateReport = user.CanManageReports || IsAdmin;
            us.CanEditDataSource = IsOwner || IsAdmin;
            us.CanCreateDataSource = user.CanModifyDataset || IsAdmin;
            us.ShowAdminControls = IsAdmin;
            us.CanCreateDataFlow = user.CanModifyDataset || IsAdmin;
            us.CanModifyDataflow = user.CanModifyDataset || IsOwner || IsAdmin;
        }

        /// <summary>
        /// This method should be called for a securable entity not marked as "Secure". It will grant most permissions 
        /// to all users, but some permissions are still based on DSC permissions.  Populates the permissions on the 
        /// <paramref name="us"/> object. Also merges any parent's permissions passed in <paramref name="parentSecurity"/>
        /// into the <paramref name="us"/> object.
        /// </summary>
        /// <param name="IsAdmin">If the current user is a DSC admin</param>
        /// <param name="IsOwner">If the current user is the owner of the securable</param>
        /// <param name="userPermissions">The list of permissions we've gathered for this user</param>
        /// <param name="us">The user security object to populate</param>
        /// <param name="parentSecurity">The user's security to this entity's parent</param>
        internal static void BuildOutUserSecurityForUnsecuredEntity(bool IsAdmin, bool IsOwner, List<string> userPermissions, UserSecurity us, UserSecurity parentSecurity)
        {
            //if it is not secure, it should be wide open except for upload and notifications. call everything out for visibility.
            us.CanPreviewDataset = true;
            us.CanQueryDataset = true;
            us.CanViewFullDataset = true;
            us.CanUploadToDataset = IsOwner || IsAdmin;
            us.CanModifyNotifications = false;
            us.CanUseDataSource = true;
            us.CanManageSchema = (userPermissions.Count > 0) ? userPermissions.Contains(PermissionCodes.CAN_MANAGE_SCHEMA) || IsOwner || IsAdmin : (IsOwner || IsAdmin);
            MergeParentSecurity(us, parentSecurity);
        }

        /// <summary>
        /// This method should be called for a securable entity that *IS* marked "Secure". It will only give permissions 
        /// that are explicitely granted. Populates the permissions on the <paramref name="us"/> object. Also merges any 
        /// parent's permissions passed in <paramref name="parentSecurity"/> into the <paramref name="us"/> object.
        /// </summary>
        /// <param name="IsAdmin">If the current user is a DSC admin</param>
        /// <param name="IsOwner">If the current user is the owner of the securable</param>
        /// <param name="userPermissions">The list of permissions we've gathered for this user</param>
        /// <param name="us">The user security object to populate</param>
        /// <param name="parentSecurity">The user's security to this entity's parent</param>
        /// <param name="securable">The securable entity itself</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S1541:Methods and properties should not be too complex", Justification = "Breaking this method up more actually makes it more difficult to understand")]
        internal static void BuildOutUserSecurityForSecuredEntity(bool IsAdmin, bool IsOwner, List<string> userPermissions, UserSecurity us, UserSecurity parentSecurity, ISecurable securable)
        {
            //from the list of permissions, build out the security object.
            us.CanPreviewDataset = userPermissions.Contains(PermissionCodes.CAN_PREVIEW_DATASET) || IsOwner || IsAdmin;
            us.CanViewFullDataset = userPermissions.Contains(PermissionCodes.CAN_VIEW_FULL_DATASET) || IsOwner || (IsAdmin && !securable.IsSensitive);
            us.CanQueryDataset = userPermissions.Contains(PermissionCodes.CAN_QUERY_DATASET) || IsOwner || IsAdmin;
            us.CanUploadToDataset = userPermissions.Contains(PermissionCodes.CAN_UPLOAD_TO_DATASET) || IsOwner || IsAdmin;
            us.CanModifyNotifications = userPermissions.Contains(PermissionCodes.CAN_MODIFY_NOTIFICATIONS) || IsOwner || IsAdmin;
            us.CanUseDataSource = userPermissions.Contains(PermissionCodes.CAN_USE_DATA_SOURCE) || IsOwner || IsAdmin;
            us.CanManageSchema = userPermissions.Contains(PermissionCodes.CAN_MANAGE_SCHEMA) || IsOwner || IsAdmin;
            MergeParentSecurity(us, parentSecurity);
        }

        /// <summary>
        /// If the <paramref name="parentSecurity"/> object is not null, merges its permissions into
        /// the <paramref name="us"/> object.
        /// </summary>
        /// <param name="us">The <see cref="UserSecurity"/> object to merge both permissions into</param>
        /// <param name="parentSecurity">The <see cref="UserSecurity"/> object to merge</param>
        internal static void MergeParentSecurity(UserSecurity us, UserSecurity parentSecurity)
        {
            //merge in the parent's security, if applicable
            if (parentSecurity != null)
            {
                us.CanPreviewDataset = us.CanPreviewDataset || parentSecurity.CanPreviewDataset;
                us.CanViewFullDataset = us.CanViewFullDataset || parentSecurity.CanViewFullDataset;
                us.CanQueryDataset = us.CanQueryDataset || parentSecurity.CanQueryDataset;
                us.CanUploadToDataset = us.CanUploadToDataset || parentSecurity.CanUploadToDataset;
                us.CanModifyNotifications = us.CanModifyNotifications || parentSecurity.CanModifyNotifications;
                us.CanUseDataSource = us.CanUseDataSource || parentSecurity.CanUseDataSource;
                us.CanManageSchema = us.CanManageSchema || parentSecurity.CanManageSchema;
            }
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
