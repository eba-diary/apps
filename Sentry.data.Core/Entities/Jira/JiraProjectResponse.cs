using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.Jira
{
    /// <summary>
    /// Jira Project Response
    /// </summary>
    public class JiraProjectResponse
    {
        /// <summary> Jira Project id </summary>
        [JsonProperty("id", Required = Required.Default, NullValueHandling = NullValueHandling.Include)]
        public string Id { get; set; }
        /// <summary> Jira Project Key. </summary>
        [JsonProperty("key", Required = Required.Default, NullValueHandling = NullValueHandling.Include)]
        public string Key { get; set; }
        /// <summary> Jira Project Name </summary>
        [JsonProperty("name", Required = Required.Default, NullValueHandling = NullValueHandling.Include)]
        public string Name { get; set; }
    }
}
