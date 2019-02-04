﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sentry.data.Core.DomainServices
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


        /// <summary>
        /// re-occuring service call that will check if the hpsm ticket has been approved.
        /// </summary>
        public void CheckHpsmTicketStatus()
        {
            List<SecurityTicket> tickets = _datasetContext.HpsmTickets.Where(x => x.TicketStatus == GlobalConstants.HpsmTicketStatus.PENDING).ToList();

            foreach (SecurityTicket ticket in tickets)
            {
                HpsmTicket st = _hpsmProvider.RetrieveTicket(ticket.TicketId);
                switch (st.TicketStatus)
                {
                    case GlobalConstants.HpsmTicketStatus.APPROVED:
                        EnablePermissions(ticket.Permissions.ToList());
                        _hpsmProvider.CloseHpsmTicket(ticket.TicketId);
                        ticket.ApprovedById = st.ApprovedById;
                        ticket.ApprovedDate = DateTime.Now;
                        ticket.TicketStatus = GlobalConstants.HpsmTicketStatus.COMPLETED;
                        break;
                    case GlobalConstants.HpsmTicketStatus.REJECTED: //or Denied?  find out those statuses.
                        _hpsmProvider.CloseHpsmTicket(ticket.TicketId, true);
                        ticket.RejectedById = st.RejectedById;
                        ticket.RejectedDate = DateTime.Now;
                        ticket.TicketStatus = GlobalConstants.HpsmTicketStatus.REJECTED;
                        break;
                    default:
                        break;  //do nothing, we will check again in 15 min.
                }
            }

        }


        public string RequestPermission(AccessRequest model)
        {
            //Lets format the business reason here before passing it into the hpsm service.
            StringBuilder sb = new StringBuilder();
            sb.Append($"Please grant the Ad Group {model.AdGroupName} the following permissions for Data.sentry.com.");
            model.Permissions.ForEach(x => sb.Append($"{x.PermissionName} - {x.PermissionDescription} |"));
            sb.Append($"Business Reason: {model.BusinessReason}");
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
                    TicketStatus = GlobalConstants.HpsmTicketStatus.PENDING,
                    RequestedById = model.RequestorsId,
                    RequestedDate = model.RequestedDate,
                    IsAddingPermission = true,
                    ParentSecurity = security,
                    Permissions = new HashSet<SecurityPermission>()
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
            //if the user is one of the primary owners or primary contact, they should have all permissions without even requesting it.
            bool IsOwner = (user.AssociateId == securable.PrimaryOwnerId || user.AssociateId == securable.SecondaryOwnerId);
            List<string> userPermissions = new List<string>();

            //set the user based permissions based off obsidian and ownership
            UserSecurity us = new UserSecurity()
            {
                CanEditDataset = user.CanModifyDataset && IsOwner,
                CanCreateDataset = user.CanModifyDataset,
                CanEditReport = user.CanManageReports && IsOwner,
                CanCreateReport = user.CanManageReports
            };

            //if it is not secure, it should be wide open except for upload.
            if (!securable.IsSecured)
            {
                us.CanPreviewDataset = true;
                us.CanQueryDataset = true;
                us.CanViewFullDataset = true;
                us.CanUploadToDataset = IsOwner;
                return us;
            }

            //if no tickets have been requested, then there should be no permission given.
            if (securable?.Security?.Tickets != null && securable.Security.Tickets.Count > 0)
            {
                //build a dictionary of AD group and Permissions.
                Dictionary<string, List<SecurityPermission>> dic = securable.Security.Tickets.ToDictionary(x => x.AdGroupName, y => y.Permissions.Where(x => x.IsEnabled).Select(z => z).ToList());

                //loop through the dictionary to see if the user is part of the group, if so grab only the enabled permissions.
                foreach (var item in dic)
                {
                    if (user.IsInGroup(item.Key))
                    {
                        userPermissions.AddRange(item.Value.Select(x => x.Permission.PermissionCode).ToList());
                    }
                }
            }

            //from the list of permissions, build out the security object.
            us.CanPreviewDataset = userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_PREVIEW_DATASET) || IsOwner;
            us.CanViewFullDataset = userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_VIEW_FULL_DATASET) || IsOwner;
            us.CanQueryDataset = userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_QUERY_DATASET) || IsOwner;
            us.CanUploadToDataset = userPermissions.Contains(GlobalConstants.PermissionCodes.CAN_UPLOAD_TO_DATASET) || IsOwner;

            return us;
        }


        private void EnablePermissions(List<SecurityPermission> permissions)
        {
            permissions.ForEach(x =>
            {
                x.IsEnabled = true;
                x.EnabledDate = DateTime.Now;
            });

            _datasetContext.SaveChanges();
        }


    }
}
