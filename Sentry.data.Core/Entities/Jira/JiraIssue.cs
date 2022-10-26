using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.Jira
{
    public class JiraIssue
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="projectKey"></param>
        /// <param name="summary"></param>
        /// <param name="description"></param>
        /// <param name="includeDescription"></param>
        /// <param name="components"></param>
        /// <param name="includeComponents"></param>
        /// <param name="labels"></param>
        /// <param name="includeLabels"></param>
        /// <param name="reporter"></param>
        /// <param name="issueType"></param>
        public JiraIssue(string projectKey, string summary, string description, bool includeDescription, IEnumerable<string> components, bool includeComponents, IEnumerable<string> labels, bool includeLabels, string reporter, string issueType)
        {
            dynamic fields = new JiraFields();
            fields.Project = new GenericJiraField() { Id = projectKey };
            fields.Summary = summary;
            if (includeDescription)
            {
                fields.Description = description;
            }
            if (components is object && includeComponents)
            {
                var compList = new List<GenericJiraField>();
                foreach (var componentId in components)
                {
                    compList.Add(new GenericJiraField() { Id = componentId });
                }
                fields.Components = compList;
            }
            if (includeLabels)
            {
                fields.Labels = labels;
            }
            fields.Reporter = new GenericJiraField() { Name = reporter };
            fields.IssueType = new GenericJiraField() { Name = issueType };
            this.JiraFields = fields;
        }

        /// <summary>
        /// Fields for the Jira issue
        /// </summary>
        [JsonProperty("fields", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public JiraFields JiraFields { get; set; }

        /// <summary>
        /// Converts object to a string of Json
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
