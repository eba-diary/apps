using Sentry.data.Core.Entities.Jira;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IJiraService
    {
        List<string> CreateJiraTickets(JiraIssueCreateRequest jiraIssueCreateRequets);
        Task<string> CreateJiraTicketAsync(JiraTicket jiraTicket);
        Task<bool> JiraUserExistsAsync(string associateId);
    }
}
