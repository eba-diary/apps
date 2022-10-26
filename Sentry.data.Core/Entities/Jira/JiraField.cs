using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.Jira
{
    /// <summary>
    /// Field metadata
    /// </summary>
    public class JiraField
    {
        /// <summary>
        /// Holds information about field type
        /// </summary>
        [JsonProperty("schema")]
        public Schema Schema { get; set; }
        /// <summary>
        /// Name of field
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        /// <summary>
        /// Id of field, for custom fields this is customfield_somenumber
        /// </summary>
        [JsonProperty("fieldId")]
        public string FieldId { get; set; }
    }

    /// <summary>
    /// Information on type of field
    /// </summary>
    public class Schema
    {
        /// <summary>
        /// Exists if field is a custom field
        /// </summary>
        [JsonProperty("custom")]
        public string Custom { get; set; }
    }
}
