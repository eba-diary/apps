using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.Jira
{
    /// <summary>
    /// Class to hold payload for a Jira Issue Search
    /// </summary>
    public class JiraSearchRequest
    {
        /// <summary>
        /// JQL to search for
        /// </summary>
        public string jql { get; set; }
        /// <summary>
        /// This is needed for the POST request to JIRA API search endpoint
        /// </summary>
        public string[] fields { get; set; } = new string[0];
    }
}
