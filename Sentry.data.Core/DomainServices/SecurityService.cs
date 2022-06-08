using Sentry.data.Core.Exceptions;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core.Interfaces.InfrastructureEventing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core
{
    public class SecurityService : ISecurityService
    {

        private readonly IDatasetContext _datasetContext;
        //BaseTicketProvider implementation is determined within Bootstrapper and could be either ICherwellProvider or IHPSMProvider
        private readonly IBaseTicketProvider _baseTicketProvider;
        private readonly IDataFeatures _dataFeatures;
        private readonly IInevService _inevService;

        public SecurityService(IDatasetContext datasetContext, IBaseTicketProvider baseTicketProvider, IDataFeatures dataFeatures, IInevService inevService)
        {
            _datasetContext = datasetContext;
            _baseTicketProvider = baseTicketProvider;
            _dataFeatures = dataFeatures;
            _inevService = inevService;
        }

        public async Task<string> RequestPermission(AccessRequest model)
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
                    IsAddingPermission = model.IsAddingPermission,
                    IsRemovingPermission = !model.IsAddingPermission,
                    ParentSecurity = security,
                    Permissions = new List<SecurityPermission>(),
                    AwsArn = model.AwsArn
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

                await PublishDatasetPermissionsUpdatedInfrastructureEvent(ticket);

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
                us.CanManageSchema = (userPermissions.Count > 0) ? userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_MANAGE_SCHEMA) || IsOwner || IsAdmin : (IsOwner || IsAdmin);
                us.CanViewData = true;
                return us;
            }

            //from the list of permissions, build out the security object.
            us.CanPreviewDataset = userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_PREVIEW_DATASET) || IsOwner || IsAdmin;
            us.CanViewFullDataset = userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_VIEW_FULL_DATASET) || IsOwner || IsAdmin;
            us.CanQueryDataset = userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_QUERY_DATASET) || IsOwner || IsAdmin;
            us.CanUploadToDataset = userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_UPLOAD_TO_DATASET) || IsOwner || IsAdmin;
            us.CanModifyNotifications = userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_MODIFY_NOTIFICATIONS) || IsOwner || IsAdmin;
            us.CanUseDataSource = userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_USE_DATA_SOURCE) || IsOwner || IsAdmin;
            us.CanManageSchema = userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_MANAGE_SCHEMA) || IsOwner || IsAdmin;
            us.CanViewData = userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_VIEW_FULL_DATASET) || IsOwner || (!securable.AdminDataPermissionsAreExplicit && IsAdmin);

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
                if (DoesSecurableInheritFromParent(securable))
                {
                    parentSecurity = GetUserSecurity(securable.Parent, user);
                }

                //get all enabled security tickets for this ISecurable
                var tickets = GetSecurityTicketsForSecurable(securable);
                //filter the list down to only those that apply to this user (either by group membership or by direct ID)
                userPermissions.AddRange(
                    tickets.Where(t => (t.AdGroupName != null && user.IsInGroup(t.AdGroupName)) || t.GrantPermissionToUserId == user.AssociateId)
                        .SelectMany(g => g.Permissions)
                        .Select(p => p.Permission.PermissionCode)
                );
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
        /// Gets inheritance SecurityTicket for a securable.
        /// Checks to see if there is a PENDING ticket first, and returns that.
        /// Otherwise, return enabled ticket for permission. 
        /// </summary>
        /// <param name="securable">The securable entity we're going to check for permissions</param>
        /// <returns></returns>
        public SecurityTicket GetSecurableInheritanceTicket(ISecurable securable)
        {
            SecurityTicket inheritanceTicket = securable.Security.Tickets.FirstOrDefault(t => t.Permissions.Any(p => p.Permission.PermissionCode == PermissionCodes.INHERIT_PARENT_PERMISSIONS) && t.TicketStatus.Equals(HpsmTicketStatus.PENDING));
            if (inheritanceTicket != null && inheritanceTicket.TicketId != null)
            {
                return inheritanceTicket;
            }
            inheritanceTicket = securable.Security.Tickets.FirstOrDefault(t => t.Permissions.Any(p => p.IsEnabled && p.Permission.PermissionCode == PermissionCodes.INHERIT_PARENT_PERMISSIONS));
            return inheritanceTicket;
        }

        /// <summary>
        /// Predicate function to determine if an <see cref="ISecurable"/> has an active 
        /// permission that allows it to inherit security from its parent
        /// </summary>
        internal static bool DoesSecurableInheritFromParent(ISecurable securable)
        {
            var inheritanceTicket = securable.Security.Tickets.FirstOrDefault(t => t.Permissions.Any(p => p.IsEnabled && p.Permission.PermissionCode == PermissionCodes.INHERIT_PARENT_PERMISSIONS));
            return (inheritanceTicket != null && securable.Parent != null);
        }

        /// <summary>
        /// Gets a list of all SecurityTickets / SecurityPermissions that have been granted to an <see cref="ISecurable"/>
        /// </summary>
        /// <param name="securable">The <see cref="ISecurable"/> that we want to get permissions for</param>
        /// <param name="includePending">If true, will include permissions that haven't been approved yet (but still won't included deleted permissions)</param>
        internal static IEnumerable<SecurityTicket> GetSecurityTicketsForSecurable(ISecurable securable, bool includePending = false)
        {
            var whereClause = includePending ? (Func<SecurityPermission, bool>)(y => y.RemovedDate == null) : (y => y.IsEnabled);
            var tickets = securable.Security.Tickets.Select(t => { t.Permissions = t.Permissions.Where(whereClause).ToList(); return t; }).Where(t => t.Permissions.Any());
            return tickets;
        }

        /// <summary>
        /// Retrieve all the permissions granted to the provided <see cref="ISecurable"/>.
        /// </summary>
        public IList<SecurablePermission> GetSecurablePermissions(ISecurable securable)
        {
            var results = new List<SecurablePermission>();

            if (securable?.Security?.Tickets != null && securable.Security.Tickets.Count > 0)
            {
                //if this securable have the "Inherit Parent Permissions" permission, get the parent's permissions
                IList<SecurablePermission> parentSecurity = new List<SecurablePermission>();
                if (DoesSecurableInheritFromParent(securable))
                {
                    parentSecurity = GetSecurablePermissions(securable.Parent);
                }

                //get this securable's tickets
                var tickets = GetSecurityTicketsForSecurable(securable, true);

                //map this securable's tickets to SecurablePermission classes
                //exclude the "Inherit Parent" permission
                results.AddRange(
                    tickets
                    .Where(t => !t.Permissions.Any(p => p.Permission.PermissionCode == PermissionCodes.INHERIT_PARENT_PERMISSIONS))
                    .SelectMany(t => t.Permissions.Select(p => new SecurablePermission
                    {
                        Scope = SecurablePermissionScope.Self,
                        ScopeSecurity = securable.Security,
                        Identity = t.Identity,
                        IdentityType = t.IdentityType,
                        SecurityPermission = p,
                        TicketId = t.TicketId
                    }))
                );

                //add in the parent permissions
                results.AddRange(
                    parentSecurity.Select(s => new SecurablePermission()
                    {
                        Scope = SecurablePermissionScope.Inherited,
                        ScopeSecurity = s.ScopeSecurity,
                        Identity = s.Identity,
                        IdentityType = s.IdentityType,
                        SecurityPermission = s.SecurityPermission,
                        TicketId = s.TicketId
                    })
                );
            }

            return results;
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
            us.CanViewData = true;
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
            us.CanPreviewDataset = userPermissions.Contains(PermissionCodes.CAN_PREVIEW_DATASET) || IsOwner || (IsAdmin);
            us.CanViewFullDataset = userPermissions.Contains(PermissionCodes.CAN_VIEW_FULL_DATASET) || IsOwner || IsAdmin;
            us.CanQueryDataset = userPermissions.Contains(PermissionCodes.CAN_QUERY_DATASET) || IsOwner || IsAdmin;
            us.CanUploadToDataset = userPermissions.Contains(PermissionCodes.CAN_UPLOAD_TO_DATASET) || IsOwner || IsAdmin;
            us.CanModifyNotifications = userPermissions.Contains(PermissionCodes.CAN_MODIFY_NOTIFICATIONS) || IsOwner || IsAdmin;
            us.CanUseDataSource = userPermissions.Contains(PermissionCodes.CAN_USE_DATA_SOURCE) || IsOwner || IsAdmin;
            us.CanManageSchema = userPermissions.Contains(PermissionCodes.CAN_MANAGE_SCHEMA) || IsOwner || IsAdmin;
            us.CanViewData = userPermissions.Contains(PermissionCodes.CAN_VIEW_FULL_DATASET) || IsOwner || (!securable.AdminDataPermissionsAreExplicit && IsAdmin);
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

        /// <summary>
        /// A ticket in Cherwell has been approved
        /// </summary>
        public async Task ApproveTicket(SecurityTicket ticket, string approveId)
        {
            ticket.ApprovedById = approveId;
            ticket.ApprovedDate = DateTime.Now;
            ticket.TicketStatus = GlobalConstants.HpsmTicketStatus.COMPLETED;

            ticket.Permissions.ToList().ForEach(x =>
            {
                x.IsEnabled = true;
                x.EnabledDate = DateTime.Now;
            });

            await PublishDatasetPermissionsUpdatedInfrastructureEvent(ticket);

        }

        private async Task PublishDatasetPermissionsUpdatedInfrastructureEvent(SecurityTicket ticket)
        {
            //If the SecurityTicket just approved includes dataset permissions
            if (_dataFeatures.CLA3718_Authorization.GetValue() &&
                ticket.Permissions.Any(p => p.Permission.SecurableObject == SecurableEntityName.DATASET))
            {
                //lookup the dataset this ticket is for
                var dataset = _datasetContext.Datasets.Where(d => d.Security.Tickets.Contains(ticket)).FirstOrDefault();
                if (dataset == null)
                {
                    throw new DatasetNotFoundException($"Could not find a dataset with SecurityTicket ID '{ticket.TicketId}' attached.");
                }
                //publish an Infrastructure Event that dataset permissions have changed
                await _inevService.PublishDatasetPermissionsUpdated(dataset, ticket, GetSecurablePermissions(dataset));
            }
        }


        /// <summary>
        /// A ticket in Cherwell has been denied
        /// </summary>
        public void CloseTicket(SecurityTicket ticket, string RejectorId, string rejectedReason, string status)
        {
            ticket.RejectedById = RejectorId;
            ticket.RejectedDate = DateTime.Now;
            ticket.RejectedReason = rejectedReason;
            ticket.TicketStatus = status;
        }
    }
}
