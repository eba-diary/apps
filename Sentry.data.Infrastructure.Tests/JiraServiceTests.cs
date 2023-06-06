using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Sentry.data.Core;
using Sentry.data.Core.DependencyInjection;
using Sentry.data.Core.Entities.Jira;
using Sentry.data.Infrastructure.Exceptions;
using Sentry.data.Infrastructure.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class JiraServiceTests : BaseInfrastructureUnitTest
    {
        [TestMethod]
        public async Task CreateJiraTicketAsync_AssistanceTicket_IssueKey()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<HttpMessageHandler> httpMessageHandler = mr.Create<HttpMessageHandler>();

            string baseUrl = "https://jira.sentry.com/rest/api/2/";

            HttpResponseMessage projectResponse = GetResponseMessage("Jira_Project.json");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == $"{baseUrl}project/CLA"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(projectResponse);

            HttpResponseMessage issueTypesResponse = GetResponseMessage("Jira_IssueTypes.json");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == $"{baseUrl}issue/createmeta/16900/issuetypes"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(issueTypesResponse);

            HttpResponseMessage issueTypeResponse = GetResponseMessage("Jira_IssueType.json");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == $"{baseUrl}issue/createmeta/16900/issuetypes/12100"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(issueTypeResponse);

            HttpResponseMessage createResponse = GetResponseMessage("Jira_Create.json");
            string requestContent = @"{""fields"":{""project"":{""id"":""16900""},""summary"":""Summary"",""description"":""Description"",""components"":[],""labels"":[""DSCRequestAssistance""],""reporter"":{""name"":""000000""},""issuetype"":{""name"":""Support Request""},""customfield_16303"":[""DEV""],""customfield_16306"":[""NonProd""]}}";
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == $"{baseUrl}issue" &&
                                                                                x.Content.ReadAsStringAsync().Result == requestContent),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(createResponse);

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object);

            JiraService jiraService = new JiraService(httpClient, baseUrl, GetDependency<JiraService>(mr));

            JiraTicket jiraTicket = new JiraTicket
            {
                Project = JiraValues.ProjectKeys.CLA,
                IssueType = JiraValues.IssueTypes.SUPPORT_REQUEST,
                Summary = "Summary",
                Description = "Description",
                Reporter = "000000",
                Labels = new List<string> { JiraValues.Labels.ASSISTANCE },
                CustomFields = new List<JiraCustomField>
                {
                    new JiraCustomField
                    {
                        Name = JiraValues.CustomFieldNames.ENVIRONMENT,
                        Value = new List<string> { "DEV" }
                    },
                    new JiraCustomField
                    {
                        Name = JiraValues.CustomFieldNames.ENVIRONMENT_TYPE,
                        Value = new List<string> { "NonProd" }
                    }
                }
            };

            string issueKey = await jiraService.CreateJiraTicketAsync(jiraTicket);

            Assert.AreEqual("CLA-000", issueKey);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task CreateJiraTicketAsync_IssueTypeNotFound_ThrowsException()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<HttpMessageHandler> httpMessageHandler = mr.Create<HttpMessageHandler>();

            string baseUrl = "https://jira.sentry.com/rest/api/2/";

            HttpResponseMessage projectResponse = GetResponseMessage("Jira_Project.json");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == $"{baseUrl}project/CLA"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(projectResponse);

            HttpResponseMessage issueTypesResponse = GetResponseMessage("Jira_IssueTypes.json");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == $"{baseUrl}issue/createmeta/16900/issuetypes"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(issueTypesResponse);

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object);

            JiraService jiraService = new JiraService(httpClient, baseUrl, GetDependency<JiraService>(mr));

            JiraTicket jiraTicket = new JiraTicket
            {
                Project = JiraValues.ProjectKeys.CLA,
                IssueType = "Other",
            };

            await Assert.ThrowsExceptionAsync<JiraServiceException>(() => jiraService.CreateJiraTicketAsync(jiraTicket));

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task CreateJiraTicketAsync_FailedCreateRequest_ThrowsException()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<HttpMessageHandler> httpMessageHandler = mr.Create<HttpMessageHandler>();

            string baseUrl = "https://jira.sentry.com/rest/api/2/";

            HttpResponseMessage projectResponse = GetResponseMessage("Jira_Project.json");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == $"{baseUrl}project/CLA"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(projectResponse);

            HttpResponseMessage issueTypesResponse = GetResponseMessage("Jira_IssueTypes.json");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == $"{baseUrl}issue/createmeta/16900/issuetypes"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(issueTypesResponse);

            HttpResponseMessage issueTypeResponse = GetResponseMessage("Jira_IssueType.json");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == $"{baseUrl}issue/createmeta/16900/issuetypes/12100"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(issueTypeResponse);

            HttpResponseMessage failedResponse = GetErrorMessage();
            string requestContent = @"{""fields"":{""project"":{""id"":""16900""},""summary"":""Summary"",""description"":""Description"",""components"":[],""labels"":[""DSCRequestAssistance""],""reporter"":{""name"":""000000""},""issuetype"":{""name"":""Support Request""},""customfield_16303"":[""DEV""],""customfield_16306"":[""NonProd""]}}";
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == $"{baseUrl}issue" &&
                                                                                x.Content.ReadAsStringAsync().Result == requestContent),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(failedResponse);

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object);

            JiraService jiraService = new JiraService(httpClient, baseUrl, GetDependency<JiraService>(mr));

            JiraTicket jiraTicket = new JiraTicket
            {
                Project = JiraValues.ProjectKeys.CLA,
                IssueType = JiraValues.IssueTypes.SUPPORT_REQUEST,
                Summary = "Summary",
                Description = "Description",
                Reporter = "000000",
                Labels = new List<string> { JiraValues.Labels.ASSISTANCE },
                CustomFields = new List<JiraCustomField>
                {
                    new JiraCustomField
                    {
                        Name = JiraValues.CustomFieldNames.ENVIRONMENT,
                        Value = new List<string> { "DEV" }
                    },
                    new JiraCustomField
                    {
                        Name = JiraValues.CustomFieldNames.ENVIRONMENT_TYPE,
                        Value = new List<string> { "NonProd" }
                    }
                }
            };

            await Assert.ThrowsExceptionAsync<JiraServiceException>(() => jiraService.CreateJiraTicketAsync(jiraTicket));

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task JiraUserExistsAsync_True()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<HttpMessageHandler> httpMessageHandler = mr.Create<HttpMessageHandler>();

            string baseUrl = "https://jira.sentry.com/rest/api/2/";

            HttpResponseMessage userResponse = GetResponseMessage("Jira_User.json");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == $"{baseUrl}user/search?username=000000"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(userResponse);

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object);

            JiraService jiraService = new JiraService(httpClient, baseUrl, GetDependency<JiraService>(mr));

            bool result = await jiraService.JiraUserExistsAsync("000000");

            Assert.IsTrue(result);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task JiraUserExistsAsync_False()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<HttpMessageHandler> httpMessageHandler = mr.Create<HttpMessageHandler>();

            string baseUrl = "https://jira.sentry.com/rest/api/2/";

            HttpResponseMessage emptyResponse = CreateResponseMessage("[]");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == $"{baseUrl}user/search?username=000000"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(emptyResponse);

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object);

            JiraService jiraService = new JiraService(httpClient, baseUrl, GetDependency<JiraService>(mr));

            bool result = await jiraService.JiraUserExistsAsync("000000");

            Assert.IsFalse(result);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task JiraUserExistsAsync_Error()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<HttpMessageHandler> httpMessageHandler = mr.Create<HttpMessageHandler>();

            string baseUrl = "https://jira.sentry.com/rest/api/2/";

            HttpResponseMessage failedResponse = GetErrorMessage();
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == $"{baseUrl}user/search?username=000000"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(failedResponse);

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object);

            JiraService jiraService = new JiraService(httpClient, baseUrl, GetDependency<JiraService>(mr));

            bool result = await jiraService.JiraUserExistsAsync("000000");

            Assert.IsFalse(result);

            mr.VerifyAll();
        }

        #region Helpers
        private DomainServiceCommonDependency<T> GetDependency<T>(MockRepository mr)
        {
            Mock<IDataFeatures> dataFeatures = mr.Create<IDataFeatures>();
            return new DomainServiceCommonDependency<T>(new MockLoggingService<T>(), dataFeatures.Object);
        }

        private HttpResponseMessage GetErrorMessage()
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("error")
            };
        }
        #endregion
    }
}
