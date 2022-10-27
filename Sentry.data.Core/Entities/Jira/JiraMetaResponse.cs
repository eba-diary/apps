using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.Jira
{
    /// <summary>
    /// Holds array of field metadata
    /// </summary>
    public class JiraMetaResponse
    {
        /// <summary>
        /// Array of fields
        /// </summary>
        [JsonProperty("values")]
        public JiraField[] Values { get; set; }
    }
}
