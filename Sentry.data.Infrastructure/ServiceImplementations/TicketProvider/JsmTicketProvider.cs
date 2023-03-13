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
                //if closing a ticket that is still pending, it's being cancelled
                if (ticket.TicketStatus == ChangeTicketStatus.PENDING)
                {
                    await _changeManagementClient.CloseChange(ticket.TicketId, "Request cancelled", CloseStatus.CANCELLED);
                }
                //if closeing a ticket that is approved, it's completed
                else if (ticket.TicketStatus == ChangeTicketStatus.APPROVED)
                {
                    await _changeManagementClient.CloseChange(ticket.TicketId, "Request approved", CloseStatus.COMPLETED);
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
            switch (sentryChange.Status)
            {
                case JsmChangeStatus.AWAITING_IMPLEMENTATION:
                case JsmChangeStatus.IMPLEMENTING:
                case JsmChangeStatus.COMPLETED:

                    return new ChangeTicket
                    {
                        ApprovedById = GetApprover(sentryChange),
                        TicketStatus = ChangeTicketStatus.APPROVED
                    };

                case JsmChangeStatus.DECLINED:

                    return new ChangeTicket
                    {
                        RejectedById = GetApprover(sentryChange),
                        RejectedReason = "Approver declined",
                        TicketStatus = ChangeTicketStatus.DENIED
                    };

                case JsmChangeStatus.CANCELED:
                case JsmChangeStatus.FAILED:

                    return new ChangeTicket
                    {
                        RejectedReason = "Ticket cancelled",
                        TicketStatus = ChangeTicketStatus.DENIED
                    };

                default:

                    return new ChangeTicket
                    {
                        TicketStatus = ChangeTicketStatus.PENDING
                    };
            }            
        }

        private string GetApprover(SentryChange sentryChange)
        {
            return sentryChange.Approvers?.Any() == true ? sentryChange.Approvers.First().ApproverID : null;
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
