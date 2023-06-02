﻿using Amazon.Runtime.Internal.Transform;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.Configuration;
using Sentry.data.Core;
using Sentry.data.Core.DependencyInjection;
using Sentry.data.Core.DomainServices;
using Sentry.data.Core.Entities.Jira;
using Sentry.data.Infrastructure.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class JiraService : BaseDomainService<JiraService>, IJiraService
    {
        private string _jiraBaseUrl;
        private readonly HttpClient _httpClient;

        public JiraService(HttpClient httpClient, string jiraBaseUrl, DomainServiceCommonDependency<JiraService> commonDependency) : base(commonDependency)
        {
            _httpClient = httpClient;
            _jiraBaseUrl = jiraBaseUrl;
        }

        /// <summary>
        /// Logic to create an Assistance Request via the Jira API
        /// </summary>
        /// <param name="jiraIssueCreateRequets"></param>
        public List<string> CreateJiraTickets(JiraIssueCreateRequest jiraIssueCreateRequets)
        {
            var ticketIds = new List<string>();
            foreach (var ticket in jiraIssueCreateRequets.Tickets)
            {
                JiraIssue jiraIssue = BuildJiraIssue(ticket);
                ticketIds.Add(CreateAndValidateJiraIssue(jiraIssue));
            }

            return ticketIds;
        }

        /// <summary>
        /// Create single Jira ticket asynchronously
        /// </summary>
        /// <param name="jiraTicket"></param>
        /// <returns></returns>
        public async Task<string> CreateJiraTicketAsync(JiraTicket jiraTicket)
        {
            JiraIssue jiraIssue = BuildJiraIssue(jiraTicket);
            string issueKey = await CreateJiraIssueAsync(jiraIssue);

            return issueKey;
        }

        /// <summary>
        /// Check if associate exists in Jira asynchronously
        /// </summary>
        /// <param name="associateId"></param>
        /// <returns></returns>
        public async Task<bool> JiraUserExistsAsync(string associateId)
        {
            var response = await _httpClient.GetAsync(_jiraBaseUrl + $"user/search?username={associateId}");
            
            if (response.IsSuccessStatusCode)
            {
                JArray users = JArray.Parse(await response.Content.ReadAsStringAsync());
                return users.Any();
            }
            else
            {
                _logger.LogInformation($"Jira User does not exist for {associateId}");
                return false;
            }
        }

        /// <summary>
        /// Seach for issues in Jira
        /// </summary>
        /// <param name="jql">Jira query language string</param>
        /// <returns></returns>
        public dynamic IssueSearch(JiraSearchRequest jql)
        {
            var response = _httpClient.PostAsync(_jiraBaseUrl + "issue", new StringContent(jql.ToString())).Result;
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(response.Content.ToString());
                throw new JiraServiceException($"Unable to search for issues with following JQL: {jql}. Status code: {response.StatusCode}.");
            }
            return JsonConvert.DeserializeObject<dynamic>(response.Content.ToString());
        }

        #region Private
        private JiraIssue BuildJiraIssue(JiraTicket jiraTicket)
        {
            var projectKey = ValidateAndReturnProject(jiraTicket.Project);
            var components = ValidateAndReturnComponents(jiraTicket.Project, jiraTicket.Components);
            var issueTypeId = "";
            try
            {
                issueTypeId = GetIssueTypeId(projectKey, jiraTicket.IssueType);
            }
            catch (NullReferenceException ex)
            {
                _logger.LogError(ex, $"Issue type {jiraTicket.IssueType} not found in {jiraTicket.Project}");
                throw new JiraServiceException($"Issue type {jiraTicket.IssueType} not found in {jiraTicket.Project}");
            }

            var customFields = GetCustomFields(projectKey, issueTypeId).ToList();
            var includeDescription = IssueTypeHasField(projectKey, issueTypeId, "Description");
            if (includeDescription)
            {
                customFields = customFields.Where(x => x.Name != "Acceptance Criteria").ToList();
            }
            var includeLabels = IssueTypeHasField(projectKey, issueTypeId, "Labels");
            var includeComponents = IssueTypeHasField(projectKey, issueTypeId, "Component/s");
            var jiraIssue = new JiraIssue(projectKey, jiraTicket.Summary, jiraTicket.Description, includeDescription, components.ToList(), includeComponents, jiraTicket.Labels, includeLabels, jiraTicket.Reporter, jiraTicket.IssueType);
            var fields = jiraIssue.JiraFields.fields;
            foreach (var field in jiraTicket.CustomFields)
            {
                var customField = customFields.FirstOrDefault(x => x.Name == field.Name);

                if (customField != null)
                {
                    if (field.Name == "Epic Link") //epic link is our special case here, because it must be referenced by Jira Issue ID (example TIS-33) which differs depending on what Jira environment we're in
                    {
                        fields.Add(customField.FieldId, ValidateAndReturnEpicId(jiraTicket.Project, field.Value.ToString()));
                    }
                    else
                    {
                        fields.Add(customField.FieldId, field.Value);
                    }
                }
            }
            jiraIssue.JiraFields.fields = fields;

            return jiraIssue;
        }

        /// <summary>
        /// Validates that a project exists
        /// </summary>
        /// <param name="projectKey"></param>
        /// <returns></returns>
        private string ValidateAndReturnProject(string projectKey)
        {
            var response = _httpClient.GetAsync(_jiraBaseUrl + $"project/{projectKey}").Result;

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(response.Content.ToString());
                throw new JiraServiceException($"Unable to validate project with key: {projectKey}. Status code: {response.StatusCode}.");
            }
            var projResponse = JsonConvert.DeserializeObject<JiraProjectResponse>(response.Content.ReadAsStringAsync().Result);
            return projResponse.Id;
        }

        /// <summary>
        /// Determines Epic Issue Key based on name
        /// </summary>
        /// <param name="projectKey"></param>
        /// <param name="epicName"></param>
        /// <returns></returns>
        private string ValidateAndReturnEpicId(string projectKey, string epicName)
        {
            var searchReq = new JiraSearchRequest();
            searchReq.jql = $"project='{projectKey}' and issuetype = 'EPIC' and summary ~ '{epicName}'";
            var epicList = IssueSearch(searchReq);

            return epicList.issues[0].key; //return the first epic key
        }

        /// <summary>
        /// Validates components exist on a project
        /// </summary>
        /// <param name="projectKey"></param>
        /// <param name="components"></param>
        /// <returns></returns>
        private IEnumerable<string> ValidateAndReturnComponents(string projectKey, IEnumerable<string> components)
        {
            if(components == null)
            {
                return new List<string>();
            }
            var response = _httpClient.GetAsync(_jiraBaseUrl + $"project/{projectKey}/components").Result;
            var componentResponse = JsonConvert.DeserializeObject<List<JiraComponentResponse>>(response.Content.ReadAsStringAsync().Result);
            var componentIds = new List<string>();
            foreach (var component in components)
            {
                var compId = componentResponse.Find((item) => item.name == component);
                if (compId is object)
                {
                    componentIds.Add(compId.id);
                }
            }
            return componentIds;
        }

        /// <summary>
        /// Get the list of available custom fields for an issue type in a project
        /// </summary>
        /// <param name="projectKey"></param>
        /// <param name="issueTypeId"></param>
        /// <returns></returns>
        private IEnumerable<JiraField> GetCustomFields(string projectKey, string issueTypeId)
        {
            var response = _httpClient.GetAsync(_jiraBaseUrl + $"issue/createmeta/{projectKey}/issuetypes/{issueTypeId}").Result;
            var r = JsonConvert.DeserializeObject<JiraMetaResponse>(response.Content.ReadAsStringAsync().Result);
            return r.Values.Where(x => x.Schema.Custom != null).ToList();
        }

        /// <summary>
        /// Gets the issue type Id
        /// </summary>
        /// <param name="projectKey"></param>
        /// <param name="issueType"></param>
        /// <returns></returns>
        private string GetIssueTypeId(string projectKey, string issueType)
        {
            var response = _httpClient.GetAsync(_jiraBaseUrl + $"issue/createmeta/{projectKey}/issuetypes").Result;
            var r = JsonConvert.DeserializeObject<JiraIssuetypeResponse>(response.Content.ReadAsStringAsync().Result);
            var issueTypeId = r.Values.FirstOrDefault(x => x.Name == issueType).Id;
            return issueTypeId;
        }

        /// <summary>
        /// Check if issue type has a certain field
        /// </summary>
        /// <param name="projectKey"></param>
        /// <param name="issueTypeId"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        private bool IssueTypeHasField(string projectKey, string issueTypeId, string fieldName)
        {
            var response = _httpClient.GetAsync(_jiraBaseUrl + $"issue/createmeta/{projectKey}/issuetypes/{issueTypeId}").Result;
            var r = JsonConvert.DeserializeObject<JiraMetaResponse>(response.Content.ReadAsStringAsync().Result);
            return r.Values.Any(x => x.Name == fieldName);
        }

        /// <summary>
        /// Creates a jira issue via the rest API
        /// </summary>
        /// <param name="issueInfo"></param>
        /// <returns></returns>
        public string CreateAndValidateJiraIssue(JiraIssue issueInfo)
        {
            var response = CreateJiraIssue(issueInfo);
            if (!response.IsSuccessStatusCode)
            {
                throw new JiraServiceException($"Creating Jira issue resulted in error. Status code: {response.StatusCode}. Error: {response.Content}");
            }
            var issueResponse = JsonConvert.DeserializeObject<JiraIssueResponse>(response.Content.ReadAsStringAsync().Result);
            _logger.LogInformation($"Jira issue {issueResponse.key} created.");
            return issueResponse.key;
        }

        /// <summary>
        /// Create Jira Issue
        /// </summary>
        /// <param name="ticketInfo"></param>
        /// <returns></returns>
        private HttpResponseMessage CreateJiraIssue(JiraIssue ticketInfo)
        {
            return _httpClient.PostAsync(_jiraBaseUrl + "issue", new StringContent(ticketInfo.ToJson(), Encoding.UTF8, "application/json")).Result;
        }

        /// <summary>
        /// Creates a jira issue via the rest API asynchronously
        /// </summary>
        /// <param name="jiraIssue"></param>
        /// <returns></returns>
        public async Task<string> CreateJiraIssueAsync(JiraIssue jiraIssue)
        {
            var response = await _httpClient.PostAsync(_jiraBaseUrl + "issue", new StringContent(jiraIssue.ToJson(), Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                throw new JiraServiceException($"Creating Jira issue resulted in error. Status code: {response.StatusCode}. Error: {response.Content.ReadAsStringAsync().Result}");
            }

            var issueResponse = JsonConvert.DeserializeObject<JiraIssueResponse>(await response.Content.ReadAsStringAsync());
            _logger.LogInformation($"Jira issue {issueResponse.key} created.");
            return issueResponse.key;
        }
        #endregion
    }
}
