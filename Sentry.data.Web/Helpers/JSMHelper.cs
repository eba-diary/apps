using Sentry.Configuration;
using System;
using System.Linq;

namespace Sentry.data.Web.Helpers
{
    public static class JSMHelper
    {
        public static string GetJsmTicketUrl(string jsmTicketId)
        {
            string ticketUrl = string.Empty;
            if (!string.IsNullOrWhiteSpace(jsmTicketId) && jsmTicketId.Contains('-'))
            {
                Uri jsmUri = new Uri(Config.GetHostSetting("JSMApiUrl"));
                ticketUrl = $"{jsmUri.Scheme}://{jsmUri.Host}/browse/{jsmTicketId}";
            }
            return ticketUrl;
        }
    }
}