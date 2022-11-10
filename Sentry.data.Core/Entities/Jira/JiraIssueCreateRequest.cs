using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.Jira
{
    public class JiraIssueCreateRequest
    {
        [Newtonsoft.Json.JsonProperty("tickets", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public ICollection<JiraTicket> Tickets { get; set; }
    }
}
