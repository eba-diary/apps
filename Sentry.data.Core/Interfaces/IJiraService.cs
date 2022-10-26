using Sentry.data.Core.Entities.Jira;
using Sentry.data.Core.Interfaces.QuartermasterRestClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IJiraService
    {
        List<string> CreateJiraTickets(JiraIssueCreateRequest jiraIssueCreateRequets);
    }
}
