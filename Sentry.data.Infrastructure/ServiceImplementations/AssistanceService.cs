using Sentry.Configuration;
using Sentry.data.Core;
using Sentry.data.Core.DependencyInjection;
using Sentry.data.Core.DomainServices;
using Sentry.data.Core.Entities.Jira;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Infrastructure
{
    public class AssistanceService : BaseDomainService<AssistanceService>, IAssistanceService
    {
        private readonly IJiraService _jiraService;
        private readonly IUserService _userService;

        public AssistanceService(IJiraService jiraService, IUserService userService, DomainServiceCommonDependency<AssistanceService> commonDependency) : base(commonDependency)
        {
            _jiraService = jiraService;
            _userService = userService;
        }

        public async Task<AddAssistanceResultDto> AddAssistanceAsync(AddAssistanceDto addAssistanceDto)
        {
            if (!_dataFeatures.CLA4870_DSCAssistance.GetValue())
            {
                throw new ResourceFeatureDisabledException(nameof(_dataFeatures.CLA4870_DSCAssistance), "AddAssistance");
            }

            JiraTicket jiraTicket = new JiraTicket
            {
                Project = JiraValues.ProjectKeys.CLA,
                IssueType = JiraValues.IssueTypes.SUPPORT_REQUEST,
                Summary = addAssistanceDto.Summary,
                Labels = new List<string> { JiraValues.Labels.ASSISTANCE },
                CustomFields = new List<JiraCustomField>
                {
                    new JiraCustomField
                    {
                        Name = JiraValues.CustomFieldNames.ENVIRONMENT,
                        Value = new List<string> { Config.GetDefaultEnvironmentName() }
                    },
                    new JiraCustomField
                    {
                        Name = JiraValues.CustomFieldNames.ENVIRONMENT_TYPE,
                        Value = new List<string> { Config.GetHostSetting("EnvironmentType") }
                    }
                }
            };

            Markdown descriptionMarkdown = BuildDescription(addAssistanceDto);

            IApplicationUser user = _userService.GetCurrentUser();
            bool userExists = await _jiraService.JiraUserExistsAsync(user.AssociateId);

            //not everyone has access to Jira, only set as reporter if they are a Jira user
            if (userExists)
            {
                jiraTicket.Reporter = user.AssociateId;
            }
            else
            {
                jiraTicket.Reporter = Config.GetHostSetting("ServiceAccountID");
                AddToDescription("Reporter", $"{user.AssociateId} - {user.DisplayName}", descriptionMarkdown);
                AddToDescription("Reporter Email", user.EmailAddress, descriptionMarkdown);
            }

            jiraTicket.Description = descriptionMarkdown.ToString();

            string issueKey = await _jiraService.CreateJiraTicketAsync(jiraTicket);

            AddAssistanceResultDto resultDto = new AddAssistanceResultDto
            {
                IssueKey = issueKey,
                IssueLink = $"{Config.GetHostSetting("JiraUrl")}browse/{issueKey}"
            };

            return resultDto;
        }

        #region Private
        private Markdown BuildDescription(AddAssistanceDto addAssistanceDto)
        {
            Markdown descriptionMarkdown = new Markdown();

            descriptionMarkdown.AddLine(addAssistanceDto.Description);
            descriptionMarkdown.AddBreak();
            AddToDescription("Current Page", addAssistanceDto.CurrentPage, descriptionMarkdown);
            AddToDescription("Dataset Name", addAssistanceDto.DatasetName, descriptionMarkdown);
            AddToDescription("Schema Name", addAssistanceDto.SchemaName, descriptionMarkdown);

            return descriptionMarkdown;
        }

        private void AddToDescription(string label, string value, Markdown markdown)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                markdown.AddBold($"{label}:");
                markdown.AddLine(" " + value);
            }
        }
        #endregion
    }
}
