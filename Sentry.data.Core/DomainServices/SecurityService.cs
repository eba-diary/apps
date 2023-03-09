using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.GlobalEnums;
using Sentry.FeatureFlags;
using Sentry.data.Core.Interfaces.InfrastructureEventing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;
using Hangfire;
using Sentry.data.Core.DTO.Security;
using Sentry.data.Core.Entities.Jira;

namespace Sentry.data.Core
{
    public class SecurityService : ISecurityService
    {
        private readonly IDatasetContext _datasetContext;
        private readonly ITicketProvider _ticketProvider;
        private readonly IDataFeatures _dataFeatures;
        private readonly IInevService _inevService;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IObsidianService _obsidianService;
        private readonly IAdSecurityAdminProvider _adSecurityAdminProvider;
        private readonly IJiraService _jiraService;

        public SecurityService(IDatasetContext datasetContext,
                               ITicketProvider ticketProvider,
                               IDataFeatures dataFeatures,
                               IInevService inevService,
                               IBackgroundJobClient backgroundJobClient,
                               IObsidianService obsidianService,
                               IAdSecurityAdminProvider adSecurityAdminProvider,
                               IJiraService jiraService)
        {
            _datasetContext = datasetContext;
            _ticketProvider = ticketProvider;
            _dataFeatures = dataFeatures;
            _inevService = inevService;
            _backgroundJobClient = backgroundJobClient;
            _obsidianService = obsidianService;
            _adSecurityAdminProvider = adSecurityAdminProvider;
            _jiraService = jiraService;
        }

        public async Task<string> RequestPermission(AccessRequest model)
        {
            string ticketId = await _ticketProvider.CreateTicketAsync(model);
            if (!string.IsNullOrWhiteSpace(ticketId))
            {
                Security security = model.Scope.Equals(AccessScope.Asset) ? GetSecurityForAsset(model.SecurableObjectName) : _datasetContext.Security.FirstOrDefault(x => x.SecurityId == model.SecurityId);

                SecurityTicket ticket = model.IsAddingPermission ? BuildAddingPermissionTicket(ticketId, model, security) : BuildRemovingPermissionTicket(ticketId, model, security);

                security.Tickets.Add(ticket);

                await PublishDatasetPermissionsUpdatedInfrastructureEvent(ticket);

                _datasetContext.SaveChanges();

                return ticketId;
            }
            return string.Empty;
        }

        public virtual SecurityTicket BuildAddingPermissionTicket(string ticketId, AccessRequest model, Security security)
        {
            SecurityTicket ticket = new SecurityTicket()
            {
                TicketId = ticketId,
                AdGroupName = model.AdGroupName,
                GrantPermissionToUserId = model.PermissionForUserId,
                TicketStatus = ChangeTicketStatus.PENDING,
                RequestedById = model.RequestorsId,
                RequestedDate = model.RequestedDate,
                IsAddingPermission = model.IsAddingPermission,
                IsRemovingPermission = !model.IsAddingPermission,
                ParentSecurity = security,
                AddedPermissions = new List<SecurityPermission>(),
                RemovedPermissions = new List<SecurityPermission>(),
                AwsArn = model.AwsArn,
                SnowflakeAccount = model.SnowflakeAccount,
                IsSystemGenerated = model.IsSystemGenerated
            };

            foreach (Permission perm in model.Permissions)
            {
                ticket.AddedPermissions.Add(new SecurityPermission()
                {
                    AddedDate = DateTime.Now,
                    Permission = perm,
                    AddedFromTicket = ticket,
                    IsEnabled = false
                });
            }
            return ticket;
        }

        public SecurityTicket BuildRemovingPermissionTicket(string ticketId, AccessRequest model, Security security)
        {
            SecurityTicket ticket = new SecurityTicket()
            {
                TicketId = ticketId,
                AdGroupName = model.AdGroupName,
                GrantPermissionToUserId = model.PermissionForUserId,
                TicketStatus = ChangeTicketStatus.PENDING,
                RequestedById = model.RequestorsId,
                RequestedDate = model.RequestedDate,
                IsAddingPermission = model.IsAddingPermission,
                IsRemovingPermission = !model.IsAddingPermission,
                ParentSecurity = security,
                AddedPermissions = new List<SecurityPermission>(),
                RemovedPermissions = new List<SecurityPermission>(),
                AwsArn = model.AwsArn,
                SnowflakeAccount = model.SnowflakeAccount
            };

            foreach (Permission permission in model.Permissions)
            {
                SecurityTicket ticketForPermCode = security.Tickets.FirstOrDefault(t => t.AddedPermissions.Any(p => p.Permission.PermissionCode == permission.PermissionCode && p.IsEnabled && p.AddedFromTicket.TicketId == model.TicketId));
                SecurityPermission toRemove = ticketForPermCode.AddedPermissions.First(p => p.Permission.PermissionCode == permission.PermissionCode);
                toRemove.RemovedFromTicket = ticket;
                ticket.RemovedPermissions.Add(toRemove);
            }

            return ticket;
        }

        public Security GetSecurityForAsset(string keycode)
        {
            return _datasetContext.Assets.FirstOrDefault(a => a.SaidKeyCode.Equals(keycode)).Security;
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
                var adGroups = securable.Security.Tickets.Select(x => new { adGroup = x.AdGroupName, permissions = x.AddedPermissions.Where(y => y.IsEnabled).ToList() }).Where(x => x.adGroup != null).ToList();
                //loop through the dictionary to see if the user is part of the group, if so grab the permissions.
                foreach (var item in adGroups)
                {
                    if (user.IsInGroup(item.adGroup))
                    {
                        userPermissions.AddRange(item.permissions.Select(x => x.Permission.PermissionCode).ToList());
                    }
                }

                //build a userId and  List(of permissionCode) anonymous obj.
                var userGroups = securable.Security.Tickets.Select(x => new { userId = x.GrantPermissionToUserId, permissions = x.AddedPermissions.Where(y => y.IsEnabled).ToList() }).Where(x => x.userId != null).ToList();
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
                us.CanModifyNotifications = false;
                us.CanUseDataSource = true;
                us.CanManageSchema = (userPermissions.Count > 0) ? userPermissions.Contains(PermissionCodes.CAN_MANAGE_SCHEMA) || IsOwner || IsAdmin : (IsOwner || IsAdmin);
                us.CanUploadToDataset = us.CanManageSchema;
                us.CanViewData = true;
                return us;
            }

            //from the list of permissions, build out the security object.
            us.CanPreviewDataset = userPermissions.Contains(PermissionCodes.CAN_PREVIEW_DATASET) || IsOwner || IsAdmin;
            us.CanViewFullDataset = userPermissions.Contains(PermissionCodes.CAN_VIEW_FULL_DATASET) || IsOwner || IsAdmin;
            us.CanQueryDataset = userPermissions.Contains(PermissionCodes.CAN_QUERY_DATASET) || IsOwner || IsAdmin;
            us.CanUploadToDataset = userPermissions.Contains(PermissionCodes.CAN_UPLOAD_TO_DATASET) || IsOwner || IsAdmin;
            us.CanModifyNotifications = userPermissions.Contains(PermissionCodes.CAN_MODIFY_NOTIFICATIONS) || IsOwner || IsAdmin;
            us.CanUseDataSource = userPermissions.Contains(PermissionCodes.CAN_USE_DATA_SOURCE) || IsOwner || IsAdmin;
            us.CanManageSchema = userPermissions.Contains(PermissionCodes.CAN_MANAGE_SCHEMA) || IsOwner || IsAdmin;
            us.CanViewData = userPermissions.Contains(PermissionCodes.CAN_VIEW_FULL_DATASET) || IsOwner || (!securable.AdminDataPermissionsAreExplicit && IsAdmin);
            us.CanDeleteDatasetFile = CanDeleteDatasetFile(us, _dataFeatures);

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
                        .SelectMany(g => g.AddedPermissions)
                        .Select(p => p.Permission.PermissionCode)
                );
            }

            //if it is not secure, it should be wide open except for upload and notifications
            if (securable == null || securable.Security == null || !securable.IsSecured)
            {
                BuildOutUserSecurityForUnsecuredEntity(IsAdmin, IsOwner, userPermissions, us, parentSecurity, _dataFeatures);
            }
            else
            {
                BuildOutUserSecurityForSecuredEntity(IsAdmin, IsOwner, userPermissions, us, parentSecurity, securable, _dataFeatures);
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
            SecurityTicket inheritanceTicket = securable.Security.Tickets.FirstOrDefault(t => t.TicketStatus.Equals(ChangeTicketStatus.PENDING) && (t.AddedPermissions.Any(p => p.Permission.PermissionCode == PermissionCodes.INHERIT_PARENT_PERMISSIONS) || t.RemovedPermissions.Any(p => p.Permission.PermissionCode == PermissionCodes.INHERIT_PARENT_PERMISSIONS)));
            if (inheritanceTicket != null && inheritanceTicket.TicketId != null)
            {
                return inheritanceTicket;
            }
            inheritanceTicket = securable.Security.Tickets.FirstOrDefault(t => t.AddedPermissions.Any(p => p.IsEnabled && p.Permission.PermissionCode == PermissionCodes.INHERIT_PARENT_PERMISSIONS));
            return inheritanceTicket;
        }

        /// <summary>
        /// Predicate function to determine if an <see cref="ISecurable"/> has an active 
        /// permission that allows it to inherit security from its parent
        /// </summary>
        internal static bool DoesSecurableInheritFromParent(ISecurable securable)
        {
            var inheritanceTicket = securable.Security.Tickets.FirstOrDefault(t => t.AddedPermissions.Any(p => p.IsEnabled && p.Permission.PermissionCode == PermissionCodes.INHERIT_PARENT_PERMISSIONS));
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
            var tickets = securable.Security.Tickets.Select(t => { t.AddedPermissions = t.AddedPermissions.Where(whereClause).ToList(); return t; }).Where(t => t.AddedPermissions.Any());
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
                    .Where(t => !t.AddedPermissions.Any(p => p.Permission.PermissionCode == PermissionCodes.INHERIT_PARENT_PERMISSIONS) && t.IsAddingPermission)
                    .SelectMany(t => t.AddedPermissions.Select(p => new SecurablePermission
                    {
                        Scope = SecurablePermissionScope.Self,
                        ScopeSecurity = securable.Security,
                        Identity = t.Identity,
                        IdentityType = t.IdentityType,
                        SecurityPermission = p,
                        TicketId = t.TicketId,
                        IsSystemGenerated = t.IsSystemGenerated
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
                        TicketId = s.TicketId,
                        IsSystemGenerated = s.IsSystemGenerated
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
            //us.CanModifyDataflow = user.CanModifyDataset || IsOwner || IsAdmin;
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
        internal static void BuildOutUserSecurityForUnsecuredEntity(bool IsAdmin, bool IsOwner, List<string> userPermissions, UserSecurity us, UserSecurity parentSecurity, IDataFeatures features)
        {
            //if it is not secure, it should be wide open except for upload and notifications. call everything out for visibility.
            us.CanPreviewDataset = true;
            us.CanQueryDataset = true;
            us.CanViewFullDataset = true;
            us.CanModifyNotifications = false;
            us.CanUseDataSource = true;
            us.CanViewData = true;
            us.CanManageSchema = (userPermissions.Count > 0) ? userPermissions.Contains(PermissionCodes.CAN_MANAGE_SCHEMA) || IsOwner || IsAdmin : (IsOwner || IsAdmin);
            us.CanUploadToDataset = us.CanManageSchema;
            us.CanModifyDataflow = IsOwner || IsAdmin;
            MergeParentSecurity(us, parentSecurity);

            us.CanDeleteDatasetFile = CanDeleteDatasetFile(us, features);
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
        internal static void BuildOutUserSecurityForSecuredEntity(bool IsAdmin, bool IsOwner, List<string> userPermissions, UserSecurity us, UserSecurity parentSecurity, ISecurable securable, IDataFeatures features)
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
            us.CanModifyDataflow = userPermissions.Contains(PermissionCodes.CAN_MANAGE_DATAFLOW) || IsOwner || IsAdmin;
            MergeParentSecurity(us, parentSecurity);

            us.CanDeleteDatasetFile = CanDeleteDatasetFile(us, features);
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

        private static bool CanDeleteDatasetFile(UserSecurity us, IDataFeatures features)
        {
            return us.CanManageSchema && features.CLA4049_ALLOW_S3_FILES_DELETE.GetValue();
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
                var groups = securable.Security.Tickets.Select(x => new { adGroup = x.AdGroupName, permissions = x.AddedPermissions.Where(y => y.IsEnabled).ToList() }).ToList();
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
        public virtual async Task ApproveTicket(SecurityTicket ticket, string approveId)
        {
            ticket.ApprovedById = approveId;
            ticket.ApprovedDate = DateTime.Now;
            ticket.TicketStatus = ChangeTicketStatus.COMPLETED;
            if (ticket.IsAddingPermission)
            {
                ticket.AddedPermissions.ToList().ForEach(x =>
                {
                    x.IsEnabled = true;
                    x.EnabledDate = DateTime.Now;
                });
            }
            else
            {
                List<SecurityPermission> toRemove = _datasetContext.SecurityPermission.Where(p => p.RemovedFromTicket == ticket).ToList();
                toRemove.ForEach(x =>
                {
                    x.IsEnabled = false;
                    x.RemovedDate = DateTime.Now;
                });
            }

            EvaluateApprovedTicketForS3Inheritance(ticket);

            EvaluateApprovedTicketForS3Access(ticket);

            await PublishDatasetPermissionsUpdatedInfrastructureEvent(ticket);

        }

        private void EvaluateApprovedTicketForS3Access(SecurityTicket ticket)
        {
            if (ticket.AddedPermissions.Any(p => p.Permission.PermissionCode == PermissionCodes.S3_ACCESS) || ticket.RemovedPermissions.Any(p => p.Permission.PermissionCode == PermissionCodes.S3_ACCESS))
            {
                try
                {
                    if (ticket.ParentSecurity.SecurableEntityName.Equals(SecurableEntityName.DATASET))
                    {
                        BuildS3RequestAssistance(ticket);
                    }
                    else //Asset
                    {
                        string keycode = _datasetContext.Assets.FirstOrDefault(a => a.Security.SecurityId.Equals(ticket.ParentSecurity.SecurityId)).SaidKeyCode;
                        List<Dataset> datasets = _datasetContext.Datasets.Where(ds => ds.Asset.SaidKeyCode.Equals(keycode) && ds.Security.Tickets.Any(t => t.AddedPermissions.Any(p => p.Permission.PermissionCode == PermissionCodes.INHERIT_PARENT_PERMISSIONS && p.IsEnabled))).ToList();
                        BuildS3RequestAssistance(datasets, ticket);
                    }
                }
                catch (Exception e)
                {
                    Sentry.Common.Logging.Logger.Fatal("Failed creating S3 Jira Request for Cherwell #" + ticket.TicketId, e);
                }
            }
        }

        private void EvaluateApprovedTicketForS3Inheritance(SecurityTicket ticket)
        {
            if (ticket.AddedPermissions.Any(p => p.Permission.PermissionCode == PermissionCodes.INHERIT_PARENT_PERMISSIONS) || ticket.RemovedPermissions.Any(p => p.Permission.PermissionCode == PermissionCodes.INHERIT_PARENT_PERMISSIONS))
            {
                //we can assume the securable here is a dataset currently, an asset doesn't have anything to inherit from
                Dataset dataset = _datasetContext.Datasets.FirstOrDefault(ds => ds.Security.SecurityId.Equals(ticket.ParentSecurity.SecurityId));
                Asset asset = _datasetContext.Assets.FirstOrDefault(a => a.SaidKeyCode.Equals(dataset.Asset.SaidKeyCode));
                List<SecurityTicket> inheritedTickets = _datasetContext.SecurityTicket.Where(t => t.ParentSecurity.SecurityId.Equals(asset.Security.SecurityId)).ToList();
                //fallback and catchup code
                bool inheritanceStatus = ticket.IsAddingPermission;

                foreach (SecurityTicket inheritedTicket in inheritedTickets)
                {
                    if (inheritedTicket.AddedPermissions.Any(p => p.Permission.PermissionCode == PermissionCodes.S3_ACCESS && p.IsEnabled))
                    {
                        BuildS3TicketForDatasetAndTicket(dataset, inheritedTicket, inheritanceStatus);
                    }
                }
            }
        }

        private async Task PublishDatasetPermissionsUpdatedInfrastructureEvent(SecurityTicket ticket)
        {
            //If the SecurityTicket just approved includes dataset/asset permissions
            if (_dataFeatures.CLA3718_Authorization.GetValue() && 
                (ticket.AddedPermissions.Any(p => p.Permission.SecurableObject == SecurableEntityName.DATASET || p.Permission.SecurableObject == SecurableEntityName.ASSET) ||
                ticket.RemovedPermissions.Any(p => p.Permission.SecurableObject == SecurableEntityName.DATASET || p.Permission.SecurableObject == SecurableEntityName.ASSET)))
            {
                if (ticket.ParentSecurity.SecurableEntityName.Equals(SecurableEntityName.DATASET))
                {
                    //lookup the dataset this ticket is for
                    var dataset = _datasetContext.Datasets.Where(d => d.Security.Tickets.Contains(ticket)).FirstOrDefault();
                    if (dataset == null)
                    {
                        throw new DatasetNotFoundException($"Could not find a dataset with SecurityTicket ID '{ticket.TicketId}' attached.");
                    }
                    //publish an Infrastructure Event that dataset permissions have changed
                    await _inevService.PublishDatasetPermissionsUpdated(dataset, ticket, GetSecurablePermissions(dataset), GetSecurablePermissions(dataset.Parent));
                }
                else
                {
                    string keycode = _datasetContext.Assets.FirstOrDefault(a => a.Security.SecurityId.Equals(ticket.ParentSecurity.SecurityId)).SaidKeyCode;
                    List<Dataset> datasets = _datasetContext.Datasets.Where(ds => ds.Asset.SaidKeyCode.Equals(keycode) && ds.Security.Tickets.Any(t => t.AddedPermissions.Any(p => p.Permission.PermissionCode == PermissionCodes.INHERIT_PARENT_PERMISSIONS && p.IsEnabled))).ToList();
                    foreach(Dataset dataset in datasets)
                    {
                        await _inevService.PublishDatasetPermissionsUpdated(dataset, ticket, GetSecurablePermissions(dataset), GetSecurablePermissions(dataset.Parent));
                    }
                }
            }
        }

        public void BuildS3RequestAssistance(SecurityTicket ticket)
        {
            var dataset = _datasetContext.Datasets.Where(d => d.Security.Tickets.Contains(ticket)).FirstOrDefault();
            BuildS3TicketForDatasetAndTicket(dataset, ticket);
        }

        private void BuildS3TicketForDatasetAndTicket(Dataset dataset, SecurityTicket ticket, bool isAddingPermission = true)
        {
            string project = "TIS";
            string summary = (ticket.IsAddingPermission && isAddingPermission ? "Create or Update" : "Remove") + " S3 Access Point with the following policy";
            StringBuilder sb = new StringBuilder();
            string issueType = "Support Request";

            //Build Description
            string account = Sentry.Configuration.Config.GetHostSetting("AwsAccountId");
            string name = "sentry-dlst-" + Sentry.Configuration.Config.GetHostSetting("EnvironmentName") + "-dataset-" + dataset.ShortName + "-ae2";

            sb.AppendLine("Account: " + account);
            sb.AppendLine("S3 Access Point Name: " + name);
            sb.AppendLine("Source Bucket: " + "sentry-dlst-" + Sentry.Configuration.Config.GetHostSetting("EnvironmentName") + "-dataset-ae2");
            sb.AppendLine("");

            sb.AppendLine("With the following policy:");
            sb.AppendLine("");

            sb.AppendLine("Principal AWS ARN: " + ticket.AwsArn);
            sb.AppendLine("Action: s3.*");
            foreach (DatasetFileConfig dsfc in dataset.DatasetFileConfigs)
            {
                sb.AppendLine("Schema: " + dsfc.Schema.Name);
                sb.AppendLine("Resource: " + "arn:aws:s3:us-east-2:" + account + ":accesspoint/" + name + "/object/" + dsfc.Schema.ParquetStoragePrefix + "/*");
                sb.AppendLine("Resource: " + "arn:aws:s3:us-east-2:" + account + ":accesspoint/" + name + "/object/" + dsfc.Schema.ParquetStoragePrefix);
                sb.AppendLine("");
            }
            sb.AppendLine("Action: S3:ListBucket");
            sb.AppendLine("Resource: " + "arn:aws:s3:us-east-2:" + account + ":accesspoint/" + name);


            List<JiraCustomField> customFields = new List<JiraCustomField>();

            JiraIssueCreateRequest jiraRequest = new JiraIssueCreateRequest();

            JiraTicket jiraTicket = new JiraTicket
            {
                Project = project,
                CustomFields = customFields,
                Reporter = ticket.RequestedById,
                IssueType = issueType,
                Summary = summary,
                Labels = new List<string> { "requestAssistance", "DSCAuthorization", "awspermissions" },
                Components = new List<string> { "ACID" },
                Description = sb.ToString()
            };

            jiraRequest.Tickets = new List<JiraTicket>() { jiraTicket };

            _jiraService.CreateJiraTickets(jiraRequest);
        }

        public void BuildS3RequestAssistance(IList<Dataset> datasets, SecurityTicket ticket)
        {
            foreach(Dataset dataset in datasets)
            {
                BuildS3TicketForDatasetAndTicket(dataset, ticket);
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

        /// <summary>
        /// Enqueues a Hangfire job that will create new AD security groups,
        /// and create the default Security Tickets in the database for them
        /// </summary>
        /// <param name="ds">The Dataset that was just created</param>
        public void EnqueueCreateDefaultSecurityForDataset(int datasetId)
        {
            _backgroundJobClient.Enqueue<SecurityService>(s => s.CreateDefaultSecurityForDataset(datasetId));
        }

        /// <summary>
        /// Enqueues datasets to run the create default security job. 
        /// </summary>
        /// <param name="datasetIdsList">List of datasets to enqueue</param>
        public void EnqueueCreateDefaultSecurityForDatasetList(int[] datasetIdList)
        {
            foreach(int datasetId in datasetIdList)
            {
                EnqueueCreateDefaultSecurityForDataset(datasetId);
            }
        }

        /// <summary>
        /// Enqueues a Hangfire job that will create the default Security
        /// Tickets in the database for them
        /// </summary>
        /// <param name="dataflowId">The Dataflow that was just created</param>
        public void EnqueueCreateDefaultSecurityForDataFlow(int dataflowId)
        {
            _backgroundJobClient.Enqueue<SecurityService>(s => s.CreateDefaultSecuityForDataflow(dataflowId));
        }

        public void EnqueueCreateDefaultSecurityForDataFlowList(int[] dataflowIdList)
        {
            foreach(int dataflowId in dataflowIdList)
            {
                EnqueueCreateDefaultSecurityForDataFlow(dataflowId);
            }
        }

        /// <summary>
        /// This method orchestrates creating new AD security groups,
        /// and creating the default SecurityTickets in the database for them.
        /// It is only run from the Goldeneye Service as a Hangfire job, 
        /// *not* from the web server directly 
        /// (web server won't have permissions to call SecBot API - called by 
        /// _adSecurityAdminProvider.CreateAdSecurityGroupAsync())
        /// </summary>
        /// <param name="ds">The dataset to create the groups for</param>

        [AutomaticRetry(Attempts = 5, DelaysInSeconds = new int[5] { 60, 60, 60, 60, 240 })]
        public Task CreateDefaultSecurityForDataset(int datasetId)
        {
            //lookup the dataset
            var ds = _datasetContext.GetById<Dataset>(datasetId);
            if (ds == null)
            {
                throw new ArgumentOutOfRangeException(nameof(datasetId), $"Dataset with ID \"{datasetId}\" could not be found.");
            }

            if (ds.ObjectStatus != ObjectStatusEnum.Active)
            {
                Common.Logging.Logger.Info($"Dataset is not active ({datasetId}), default security will not be created");
                return Task.CompletedTask;
            }

            // This "wrapping" of the async portion of this method is required so that the error checking above runs immediately.
            // See https://sonarqube.sentry.com/coding_rules?open=csharpsquid%3AS4457&rule_key=csharpsquid%3AS4457
            return CreateDefaultSecurityForDatasetAsync();
            async Task CreateDefaultSecurityForDatasetAsync()
            {
                //enumerate the 4 AD groups we're going to create
                var groups = GetDefaultSecurityGroupDtos(ds);

                //get the list of permissions consumers and producers will be granted
                var permissions = GetDefaultPermissions();

                //get the list of existing security tickets for this dataset and asset
                var datasetTickets = GetSecurityTicketsForSecurable(ds, true);
                var assetTickets = GetSecurityTicketsForSecurable(ds.Asset, true);

                //actually create the AD groups
                await CreateDefaultSecurityForDataset_Internal(ds, groups, permissions, datasetTickets, assetTickets);
            }
        }

        public Task CreateDefaultSecuityForDataflow(int dataflowId)
        {
            var df = _datasetContext.GetById<DataFlow>(dataflowId);
            if (df == null)
            {
                throw new ArgumentOutOfRangeException(nameof(dataflowId), $"Dataflow with ID \"{dataflowId}\" could not be found.");
            }

            var ds = _datasetContext.GetById<Dataset>(df.DatasetId);
            if (ds == null)
            {
                throw new ArgumentOutOfRangeException(nameof(dataflowId), $"Dataset with ID \"{df.DatasetId}\" could not be found.");
            }

            if (ds.ObjectStatus != ObjectStatusEnum.Active || df.ObjectStatus != ObjectStatusEnum.Active)
            {
                Common.Logging.Logger.Info($"Dataflow is not active ({dataflowId}), default security will not be created");
                return Task.CompletedTask;
            }

            return CreateDefaultSecurityForDataflowAsync();
            async Task CreateDefaultSecurityForDataflowAsync()
            {
                //enumerate the dataset producer AD group, excluding asset level
                var groups = GetDefaultSecurityGroupDtos(ds).Where(w => !w.IsAssetLevelGroup() && w.GroupType == AdSecurityGroupType.Prdcr).ToList();

                //get the list of permissions consumers and producers will be granted
                var permissions = GetDefaultPermissions();

                //get the list of existing security tickets for this dataset
                var datasetTickets = GetSecurityTicketsForSecurable(ds, true);
                await CreateDefaultSecurityForDataflow_Internal(df, groups, permissions, datasetTickets);
            }
        }

        internal async Task CreateDefaultSecurityForDataset_Internal(Dataset ds, List<AdSecurityGroupDto> groups, DefaultPermissions permissions, IEnumerable<SecurityTicket> datasetTickets, IEnumerable<SecurityTicket> assetTickets)
        {
            foreach (var group in groups)
            {
                //create the group in AD (if it doesn't already exist)
                if (!_obsidianService.DoesGroupExist(group.GetGroupName()))
                {
                    await _adSecurityAdminProvider.CreateAdSecurityGroupAsync(group);
                }

                //create the security ticket that grants this group consumer or producer permissions (if it doesn't already exist)
                if (((group.GroupType == AdSecurityGroupType.Cnsmr && permissions.ConsumerPermissions != null)
                    || (group.GroupType == AdSecurityGroupType.Prdcr && permissions.ProducerPermissions != null)) 
                    && !datasetTickets.Any(t => t.AdGroupName == group.GetGroupName() && t.AddedPermissions.Any(p => p.Permission.PermissionCode == PermissionCodes.CAN_PREVIEW_DATASET)) 
                    && !assetTickets.Any(t => t.AdGroupName == group.GetGroupName() && t.AddedPermissions.Any(p => p.Permission.PermissionCode == PermissionCodes.CAN_PREVIEW_DATASET)))
                {
                    var accessRequest = new AccessRequest()
                    {
                        AdGroupName = group.GetGroupName(),
                        SecurityId = group.IsAssetLevelGroup() ? ds.Asset.Security.SecurityId : ds.Security.SecurityId,
                        RequestorsId = Environment.UserName,
                        RequestedDate = DateTime.Now,
                        IsAddingPermission = true,
                        Permissions = group.GroupType == AdSecurityGroupType.Cnsmr ? permissions.ConsumerPermissions : permissions.ProducerPermissions,
                        IsSystemGenerated = true
                    };
                    var securityTicket = BuildAndAddPermissionTicket(accessRequest, group.IsAssetLevelGroup() ? ds.Asset.Security : ds.Security, "DEFAULT_SECURITY");
                    await ApproveTicket(securityTicket, Environment.UserName); //approving the ticket will also publish the Infrastructure Event
                    _datasetContext.SaveChanges();
                }

                //create the security ticket that grants this group Snowflake permissions (if it doesn't already exist)
                if (permissions.SnowflakePermissions != null
                    && !datasetTickets.Any(t => t.AdGroupName == group.GetGroupName() && t.AddedPermissions.Any(p => p.Permission.PermissionCode == PermissionCodes.SNOWFLAKE_ACCESS))
                    && !assetTickets.Any(t => t.AdGroupName == group.GetGroupName() && t.AddedPermissions.Any(p => p.Permission.PermissionCode == PermissionCodes.SNOWFLAKE_ACCESS)))
                {
                    var accessRequest = new AccessRequest()
                    {
                        AdGroupName = group.GetGroupName(),
                        SecurityId = group.IsAssetLevelGroup() ? ds.Asset.Security.SecurityId : ds.Security.SecurityId,
                        RequestorsId = Environment.UserName,
                        RequestedDate = DateTime.Now,
                        IsAddingPermission = true,
                        Permissions = permissions.SnowflakePermissions,
                        IsSystemGenerated = true
                    };
                    var securityTicket = BuildAndAddPermissionTicket(accessRequest, group.IsAssetLevelGroup() ? ds.Asset.Security : ds.Security, "DEFAULT_SECURITY");
                    //don't auto-approve Snowflake permissions - they will be approved when the Cherwell ticket that the DBA portal creates is approved
                    //ApproveTicket() would call PublishDatasetPermissionsUpdatedInfrastructureEvent() - so we call it explicitely here
                    await PublishDatasetPermissionsUpdatedInfrastructureEvent(securityTicket);
                    _datasetContext.SaveChanges();
                }
            }

            // This new dataset should inherit it's parent's permissions, unless it's Highly Sensitive or Restricted
            if(!(ds.DataClassification == DataClassificationType.HighlySensitive || ds.IsSecured))
            {
                var accessRequest = new AccessRequest()
                {
                    SecurityId = ds.Security.SecurityId,
                    RequestorsId = Environment.UserName,
                    RequestedDate = DateTime.Now,
                    IsAddingPermission = true,
                    Permissions = new List<Permission>() { _datasetContext.Permission.FirstOrDefault(p => p.PermissionCode.Equals(PermissionCodes.INHERIT_PARENT_PERMISSIONS))},
                    IsSystemGenerated = true
                };
                var securityTicket = BuildAndAddPermissionTicket(accessRequest, ds.Security, "DEFAULT_SECURITY_INHERITANCE");                
                await ApproveTicket(securityTicket, Environment.UserName);
                _datasetContext.SaveChanges();
            }
        }

        internal async Task CreateDefaultSecurityForDataflow_Internal(DataFlow df, List<AdSecurityGroupDto> groups, DefaultPermissions permissions, IEnumerable<SecurityTicket> datasetTickets)
        {
            foreach(var group in groups)
            {
                //create the security ticket that grants this group producer permissions (if it doesn't already exist)
                if ((group.GroupType == AdSecurityGroupType.Prdcr && permissions.DataflowPermissions != null)
                    && !datasetTickets.Any(t => t.AdGroupName == group.GetGroupName() && t.AddedPermissions.Any(p => p.Permission.PermissionCode == PermissionCodes.CAN_MANAGE_DATAFLOW)))
                {
                    var accessRequest = new AccessRequest()
                    {
                        AdGroupName = group.GetGroupName(),
                        SecurityId = df.Security.SecurityId,
                        RequestorsId = Environment.UserName,
                        RequestedDate = DateTime.Now,
                        IsAddingPermission = true,
                        Permissions = permissions.DataflowPermissions,
                        IsSystemGenerated = true
                    };
                    var securityTicket = BuildAndAddPermissionTicket(accessRequest, df.Security, "DEFAULT_SECURITY");
                    await ApproveTicket(securityTicket, Environment.UserName);
                    _datasetContext.SaveChanges();
                }
            }
        }

        private SecurityTicket BuildAndAddPermissionTicket(AccessRequest accessRequest, Security security, string ticketId)
        {
            var securityTicket = BuildAddingPermissionTicket(ticketId, accessRequest, security);
            security.Tickets.Add(securityTicket);
            _datasetContext.SaveChanges();
            return securityTicket;
        }

        /// <summary>
        /// Get a list of what the Default Security Groups should be for this dataset
        /// </summary>
        public List<AdSecurityGroupDto> GetDefaultSecurityGroupDtos(Dataset ds)
        {
            var envType = ds.NamedEnvironmentType == NamedEnvironmentType.Prod ? AdSecurityGroupEnvironmentType.P : AdSecurityGroupEnvironmentType.NP;
            return new List<AdSecurityGroupDto> {
                AdSecurityGroupDto.NewDatasetGroup(ds.Asset.SaidKeyCode, ds.ShortName, AdSecurityGroupType.Cnsmr, envType),
                AdSecurityGroupDto.NewDatasetGroup(ds.Asset.SaidKeyCode, ds.ShortName, AdSecurityGroupType.Prdcr, envType),
                AdSecurityGroupDto.NewAssetGroup(ds.Asset.SaidKeyCode, ds.ShortName, AdSecurityGroupType.Cnsmr, envType),
                AdSecurityGroupDto.NewAssetGroup(ds.Asset.SaidKeyCode, ds.ShortName, AdSecurityGroupType.Prdcr, envType)
            };
        }

        internal class DefaultPermissions
        {
            public DefaultPermissions(List<Permission> producerPermissions, List<Permission> consumerPermissions, List<Permission> snowflakePermissions, List<Permission> dataflowPermissions)
            {
                ProducerPermissions = producerPermissions;
                ConsumerPermissions = consumerPermissions;
                SnowflakePermissions = snowflakePermissions;
                DataflowPermissions = dataflowPermissions;

            }
            public List<Permission> ProducerPermissions { get; }
            public List<Permission> ConsumerPermissions { get; }
            public List<Permission> SnowflakePermissions { get; }
            public List<Permission> DataflowPermissions { get; }
        }

        internal DefaultPermissions GetDefaultPermissions()
        {
            return new DefaultPermissions(GetProducerPermissions(), GetConsumerPermissions(), GetSnowflakePermissions(), GetDataflowPermissions());
        }

        internal List<Permission> GetProducerPermissions()
        {
            var producerPermissionCodes = new List<string>() {
                PermissionCodes.CAN_UPLOAD_TO_DATASET,
                PermissionCodes.CAN_MANAGE_SCHEMA
            };
            var producerPermissions = _datasetContext.Permission.Where(x => producerPermissionCodes.Contains(x.PermissionCode) &&
                                                             x.SecurableObject == SecurableEntityName.DATASET).ToList();
            producerPermissions.AddRange(GetConsumerPermissions());
            return producerPermissions;
        }

        internal List<Permission> GetConsumerPermissions()
        {
            var consumerPermissionCodes = new List<string>() {
                PermissionCodes.CAN_PREVIEW_DATASET,
                PermissionCodes.CAN_VIEW_FULL_DATASET,
            };
            return _datasetContext.Permission.Where(x => consumerPermissionCodes.Contains(x.PermissionCode) &&
                                                         x.SecurableObject == SecurableEntityName.DATASET).ToList();
        }

        internal List<Permission> GetSnowflakePermissions()
        {
            var consumerPermissionCodes = new List<string>() {
                PermissionCodes.SNOWFLAKE_ACCESS
            };
            return _datasetContext.Permission.Where(x => consumerPermissionCodes.Contains(x.PermissionCode) &&
                                                         x.SecurableObject == SecurableEntityName.DATASET).ToList();
        }

        internal List<Permission> GetDataflowPermissions()
        {
            var dataflowPermissionCodes = new List<string>()
            {
                PermissionCodes.CAN_MANAGE_DATAFLOW
            };
            return _datasetContext.Permission.Where(x => dataflowPermissionCodes.Contains(x.PermissionCode) &&
                                                        x.SecurableObject == SecurableEntityName.DATAFLOW).ToList();
        }
    }
}
