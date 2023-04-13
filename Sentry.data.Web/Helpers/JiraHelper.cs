using Sentry.Configuration;
using System;
using System.Linq;

namespace Sentry.data.Web.Helpers
{
    public static class JiraHelper
    {
        public static string GetJiraTicketUrl(string jiraTicketId)
        {
            string ticketUrl = string.Empty;
            if (!string.IsNullOrWhiteSpace(jiraTicketId) && jiraTicketId.Contains('-'))
            {
                Uri jiraUri = new Uri(Config.GetHostSetting("JiraServiceUrl"));
                ticketUrl = $"{jiraUri.Scheme}://{jiraUri.Host}/browse/{jiraTicketId}";
            }            
            return ticketUrl;
        }
    }
}