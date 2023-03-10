using Sentry.ChangeManagement;
using Sentry.ChangeManagement.Sentry;
using Sentry.Common.Logging;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Infrastructure
{
    public class JsmTicketProvider : ITicketProvider
    {
        private readonly ISentryChangeManagementClient _changeManagementClient;

        public JsmTicketProvider(ISentryChangeManagementClient changeManagementClient)
        {
            _changeManagementClient = changeManagementClient;
        }

        public async Task<string> CreateTicketAsync(AccessRequest request)
        {
            try
            {
                DateTime end = DateTime.Now.AddDays(14);
                SentryChange newChange = new SentryChange
                {
                    Title = GetChangeTitle(request),
                    Description = GetChangeDescription(request),
                    PlannedStart = DateTime.Now,
                    PlannedEnd = end,
                    CompletionDate = end,
                    AssignedTeam = JsmAssignmentGroup.BI_PORTAL_ADMIN,
                    ImplementationNotes = "NA",
                    ImplementationPlan = "NA",
                    Approvers = new List<SentryApprover>
                    {
                        new SentryApprover { ApproverID = request.ApproverId }
                    }
                };

                SentryChange sentryChange = await _changeManagementClient.NewSentryChange(newChange);
                return sentryChange.ChangeID;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error attempting to create change in JSM", ex);
            }

            return string.Empty;
        }

        public async Task<ChangeTicket> RetrieveTicketAsync(string ticketId)
        {
            ChangeTicket changeTicket = null;

            try
            {
                SentryChange sentryChange = await _changeManagementClient.GetChange(ticketId);
                changeTicket = MapToChangeTicketFrom(sentryChange);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error attempting to get change {ticketId} from JSM", ex);
            }

            return changeTicket;
        }

        public async Task CloseTicketAsync(ChangeTicket ticket)
        {
            try
            {
                //move ticket to a closed state depending on what the status is
                //if pending, goes to cancelled
                //if approved, goes to completed
                //if denied or withdrawn, do nothing because these are "closed" statuses

                //CloseChange takes care of phase being available
                if (ticket.TicketStatus == ChangeTicketStatus.PENDING)
                {
                    await _changeManagementClient.CloseChange(ticket.TicketId, "Request cancelled", "cancelled");
                }
                else if (ticket.TicketStatus == ChangeTicketStatus.APPROVED)
                {
                    await _changeManagementClient.CloseChange(ticket.TicketId, "Request approved", "completed");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error attempting to close change {ticket.TicketId} in JSM", ex);
            }
        }

        #region Private
        private ChangeTicket MapToChangeTicketFrom(SentryChange sentryChange)
        {
            return new ChangeTicket
            {
                ApprovedById = sentryChange.Approvers?.Any() == true ? sentryChange.Approvers.First().ApproverID : null,
                TicketStatus = TranslateStatus(sentryChange.Status)
            };
        }

        private string TranslateStatus(string jsmStatus)
        {
            switch (jsmStatus)
            {
                case JsmChangeStatus.REVIEW:
                case JsmChangeStatus.INDIVIDUAL_AUTHORIZE:
                    return ChangeTicketStatus.PENDING;
                case JsmChangeStatus.DECLINED:
                    return ChangeTicketStatus.DENIED;
                case JsmChangeStatus.CANCELED:
                    return ChangeTicketStatus.WITHDRAWN;
                default:
                    return ChangeTicketStatus.APPROVED;
            }
        }

        private string GetChangeTitle(AccessRequest request)
        {
            switch (request.Type)
            {
                case AccessRequestType.AwsArn:
                    return $"Access {(request.IsAddingPermission ? "" : "Removal")} Request for AWS ARN {request.AwsArn}";
                case AccessRequestType.SnowflakeAccount:
                    return $"Access {(request.IsAddingPermission ? "" : "Removal")} Request for Snowflake Account {request.SnowflakeAccount}";
                case AccessRequestType.Inheritance:
                    return $"Inheritance {(request.IsAddingPermission ? "enable" : "disable")} request for {request.SecurableObjectName}";
                default:
                    if (request.AdGroupName != null)
                    {
                        return $"Access Request for AD Group {request.AdGroupName}";
                    }
                    else
                    {
                        return $"Access Request for user {request.PermissionForUserName}";
                    }
            }
        }

        private string GetChangeDescription(AccessRequest request)
        {
            //copy what is being created in CherwellProvider.BuildBodyByTemplate
            throw new NotImplementedException();
        }
        #endregion
    }
}
