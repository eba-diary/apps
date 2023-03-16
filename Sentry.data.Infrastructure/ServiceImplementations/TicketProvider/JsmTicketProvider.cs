using Sentry.ChangeManagement;
using Sentry.ChangeManagement.Sentry;
using Sentry.Common.Logging;
using Sentry.Configuration;
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
                await _changeManagementClient.MovePhase(sentryChange.ChangeID, JsmChangePhase.READY_APPROVAL);

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
                //if closing a ticket that is approved, it's completed
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
            ChangeTicket changeTicket = new ChangeTicket 
            { 
                TicketId = sentryChange.ChangeID
            };

            switch (sentryChange.Status)
            {
                case JsmChangeStatus.AWAITING_IMPLEMENTATION:
                case JsmChangeStatus.IMPLEMENTING:
                case JsmChangeStatus.COMPLETED:

                    changeTicket.ApprovedById = GetApprover(sentryChange);
                    changeTicket.TicketStatus = ChangeTicketStatus.APPROVED;
                    break;

                case JsmChangeStatus.DECLINED:

                    changeTicket.RejectedById = GetApprover(sentryChange);
                    changeTicket.RejectedReason = "Approver declined";
                    changeTicket.TicketStatus = ChangeTicketStatus.DENIED;
                    break;

                case JsmChangeStatus.CANCELED:
                case JsmChangeStatus.FAILED:

                    changeTicket.RejectedReason = "Ticket cancelled";
                    changeTicket.TicketStatus = ChangeTicketStatus.WITHDRAWN;
                    break;

                default:
                    changeTicket.TicketStatus = ChangeTicketStatus.PENDING;
                    break;
            } 
            
            return changeTicket;
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
                    return $"Access {(request.IsAddingPermission ? "" : "Removal ")}request for AWS ARN {request.AwsArn}";
                case AccessRequestType.SnowflakeAccount:
                    return $"Access {(request.IsAddingPermission ? "" : "Removal ")}request for Snowflake Account {request.SnowflakeAccount}";
                case AccessRequestType.Inheritance:
                    return $"Inheritance {(request.IsAddingPermission ? "enable" : "disable")} request for {request.SecurableObjectName}";
                default:
                    if (request.AdGroupName != null)
                    {
                        return $"Access request for AD Group {request.AdGroupName}";
                    }
                    else
                    {
                        return $"Access request for user {request.PermissionForUserName}";
                    }
            }
        }

        private string GetChangeDescription(AccessRequest request)
        {
            Markdown markdown = new Markdown();

            switch (request.Type)
            {
                case AccessRequestType.AwsArn:
                    BuildAwsArnDescription(request, markdown);
                    break;
                case AccessRequestType.Inheritance:
                    BuildInheritanceDescription(request, markdown);
                    break;
                case AccessRequestType.SnowflakeAccount:
                    BuildSnowflakeAccountDescription(request, markdown);
                    break;
                default:
                    BuildDefaultDescription(request, markdown);
                    break;
            }

            markdown.AddBold("Business Reason:");
            markdown.AddLine(" " + request.BusinessReason);
            markdown.AddBold("Requestor:");
            markdown.AddLine($" {request.RequestorsId} - {request.RequestorsName}");
            markdown.AddBold($"DSC Environment:");
            markdown.Add(" " + Config.GetHostSetting("WebApiUrl").Replace("http://", ""));

            return markdown.ToString();
        }

        private void BuildAwsArnDescription(AccessRequest request, Markdown markdown)
        {
            if (request.IsAddingPermission)
            {
                markdown.AddLine($"Please grant the AWS ARN *{request.AwsArn}* the following permissions to {GetAccessor(request)} data.", false);
            }
            else
            {
                markdown.AddLine($"Please remove the following permissions for the AWS ARN *{request.AwsArn}* from {GetAccessor(request)} data.", false);
            }

            markdown.AddList(request.Permissions);
            markdown.AddBreak();
        }

        private void BuildSnowflakeAccountDescription(AccessRequest request, Markdown markdown)
        {
            if (request.IsAddingPermission)
            {
                markdown.AddLine($"Please grant the Snowflake Account *{request.SnowflakeAccount}* the following permissions to {GetAccessor(request)} data.", false);
            }
            else
            {
                markdown.AddLine($"Please remove the following permissions for the Snowflake Account *{request.SnowflakeAccount}* from {GetAccessor(request)} data.", false);
            }

            markdown.AddList(request.Permissions);
            markdown.AddBreak();
        }

        private void BuildInheritanceDescription(AccessRequest request, Markdown markdown)
        {
            markdown.AddLine($"Please {(request.IsAddingPermission ? "enable" : "disable")} inheritance for dataset *{request.SecurableObjectName}* from Data.Sentry.com. {(request.IsAddingPermission ? "Enabling" : "Disabling")} inheritance will {(request.IsAddingPermission ? "allow" : "prevent")} the dataset {(request.IsAddingPermission ? "to" : "from")} {(request.IsAddingPermission ? "inherit" : "inheriting")} permissions from its parent asset {request.SaidKeyCode}. When approved, users with access to {request.SaidKeyCode} in Data.Sentry.com {(request.IsAddingPermission ? "will" : "will not")} have access to {request.SecurableObjectName} data.", false);
            markdown.AddList(request.Permissions);
            markdown.AddBreak();
            markdown.Add($"For more information on Authorization in DSC - ");
            markdown.AddLink("Auth Guide", "https://confluence.sentry.com/pages/viewpage.action?pageId=361734893");
            markdown.AddBreak();
            markdown.AddBreak();
            markdown.AddBold("Said Asset:");
            markdown.AddLine(" " + request.SaidKeyCode);
        }

        private void BuildDefaultDescription(AccessRequest request, Markdown markdown)
        {
            if (request.IsAddingPermission)
            {
                markdown.AddLine($"Please grant *{request.AdGroupName ?? request.PermissionForUserName}* the following permissions to {GetAccessor(request)} in Data.sentry.com.", false);
            }
            else
            {
                markdown.AddLine($"Please remove the following permissions to *{request.SecurableObjectName}* in Data.sentry.com.", false);
            }
            markdown.AddList(request.Permissions);
            markdown.AddBreak();
            markdown.AddBold("Said Asset:");
            markdown.AddLine(" " + request.SaidKeyCode);
        }

        private string GetAccessor(AccessRequest request)
        {
            if (request.Scope == AccessScope.Asset)
            {
                return request.SaidKeyCode;
            }
            else 
            {
                return $"{request.SecurableObjectName} ({request.SecurableObjectNamedEnvironment})";
            }
        }
        #endregion
    }
}
