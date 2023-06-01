using Sentry.data.Core.Entities.Jira;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IJiraService
    {
        List<string> CreateJiraTickets(JiraIssueCreateRequest jiraIssueCreateRequets);
        bool JiraUserExists(string associateId);
    }
}
