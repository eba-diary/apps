using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.Jira
{
    /// <summary>
    /// Information about a Jira issue type
    /// </summary>
    public class JiraIssueType
    {
        /// <summary>
        /// Id of issue type
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }
        /// <summary>
        /// Name of issue type
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

    }
}
