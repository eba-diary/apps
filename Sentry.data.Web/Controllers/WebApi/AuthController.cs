using Sentry.data.Core;
using Sentry.data.Core.Entities.Jira;
using Sentry.WebAPI.Versioning;
using Swashbuckle.Swagger.Annotations;
using System.Collections.Generic;
using System.Web.Http;

namespace Sentry.data.Web.WebApi.Controllers
{
    [RoutePrefix(WebConstants.Routes.VERSION_AUTH)]
    //Users need aleast UseApp permission to access any endpoint on this controller
    [WebApiAuthorizeUseApp]
    public class AuthController : BaseWebApiController
    {
        private readonly IDatasetContext _dsContext;
        private readonly UserService _userService;
        private readonly ISecurityService _securityService;
        private readonly IJiraService _jiraService;

        public AuthController(IDatasetContext dsContext, UserService userService, ISecurityService securityService, IJiraService jiraService)
        {
            _dsContext = dsContext;
            _userService = userService;
            _securityService = securityService;
            _jiraService = jiraService;
        }

        /// <summary>
        /// Creates default security for dataset IDs provided
        /// </summary>
        [HttpPost]
        [ApiVersionBegin(WebAPI.Version.v20220609)]
        [WebApiAuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        [Route("CreateDefaultSecurity")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK)]
        [SwaggerResponse(System.Net.HttpStatusCode.BadRequest)]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError)]
        public IHttpActionResult CreateDefaultSecurity(int[] datasetIdList)
        {
            Sentry.Common.Logging.Logger.Info($"{_userService.GetCurrentRealUser().AssociateId} called CreateDefaultSecurity on Dataset Id(s) {datasetIdList}");
            //validate
            foreach (int datasetId in datasetIdList)
            {
                var ds = _dsContext.GetById<Dataset>(datasetId);
                if (ds == null)
                {
                    return BadRequest($"Dataset with ID \"{datasetId}\" could not be found.");
                }
            }
            //queue jobs
            _securityService.EnqueueCreateDefaultSecurityForDatasetList(datasetIdList);
            return Ok();
        }

        /// <summary>
        /// Testing
        /// </summary>
        [HttpPost]
        [ApiVersionBegin(WebAPI.Version.v20220609)]
        [WebApiAuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        [Route("CreateJiraTicket")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK)]
        [SwaggerResponse(System.Net.HttpStatusCode.BadRequest)]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError)]
        public IHttpActionResult CreateJiraTicket()
        {
            string project = Sentry.Configuration.Config.GetHostSetting("S3_JiraTicketProject");
            string summary = "Sam Test Ticket 2";
            string issueType = "Request";

            List<JiraCustomField> customFields = new List<JiraCustomField>();
            JiraCustomField acceptanceCriteria = new JiraCustomField();
            acceptanceCriteria.Name = "Acceptance Criteria";
            acceptanceCriteria.Value = "Placeholder";
            customFields.Add(acceptanceCriteria);
            JiraCustomField reporter = new JiraCustomField();
            reporter.Name = "Reporter";
            reporter.Value = "082116";

            JiraIssueCreateRequest req = new JiraIssueCreateRequest();

            JiraTicket ticket = new JiraTicket();
            ticket.Project = project;
            ticket.CustomFields = customFields;
            ticket.Reporter = "082116";
            ticket.IssueType = issueType;
            ticket.Summary = summary;
            ticket.Labels = new List<string>();
            ticket.Components = new List<string>();
            ticket.Description = "";

            req.Tickets = new List<JiraTicket>() { ticket };
            _jiraService.CreateJiraTickets(req);
            return Ok();
        }
    }
}
