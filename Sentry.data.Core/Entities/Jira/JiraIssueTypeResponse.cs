using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.Jira
{
    /// <summary>
    /// Holds array of issue types on a project
    /// </summary>
    public class JiraIssuetypeResponse
    {
        /// <summary>
        /// Array of issue types
        /// </summary>
        [JsonProperty("values")]
        public JiraIssueType[] Values { get; set; }
    }
}
