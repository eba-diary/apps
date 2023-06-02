using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NHibernate.Util;
using Sentry.data.Core;
using Sentry.data.Core.DependencyInjection;
using Sentry.data.Core.Entities.Jira;
using System.Linq;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class AssistanceServiceTests
    {
        [TestMethod]
        public async Task AddAssistanceAsync_UserExists_NoDataset_NoSchema_AddAssistanceResultDto()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IApplicationUser> user = new Mock<IApplicationUser>();
            user.SetupGet(x => x.AssociateId).Returns("000000");

            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(user.Object);

            Mock<IJiraService> jiraService = mr.Create<IJiraService>();
            jiraService.Setup(x => x.JiraUserExistsAsync("000000")).ReturnsAsync(true);
            jiraService.Setup(x => x.CreateJiraTicketAsync(It.IsAny<JiraTicket>())).ReturnsAsync("CLA-000").Callback<JiraTicket>(x =>
            {
                Assert.AreEqual(JiraValues.ProjectKeys.CLA, x.Project);
                Assert.AreEqual(JiraValues.IssueTypes.SUPPORT_REQUEST, x.IssueType);
                Assert.AreEqual("Summary", x.Summary);
                Assert.AreEqual(1, x.Labels.Count);
                Assert.AreEqual(JiraValues.Labels.ASSISTANCE, x.Labels.First());
                Assert.AreEqual(1, x.CustomFields.Count);
                Assert.AreEqual("000000", x.Reporter);
                Assert.IsTrue(x.Description.Contains("Description"));
                Assert.IsTrue(x.Description.Contains("*Current Page:* data.sentry.com/Search/Datasets"));
                Assert.IsTrue(x.Description.Contains("*DSC Environment:* DEV"));
                Assert.IsFalse(x.Description.Contains("Dataset Name"));
                Assert.IsFalse(x.Description.Contains("Schema Name"));
                Assert.IsFalse(x.Description.Contains("Reporter"));
                Assert.IsFalse(x.Description.Contains("Reporter Email"));

                JiraCustomField customField = x.CustomFields.First();
                Assert.AreEqual(JiraValues.CustomFieldNames.SYSTEM, customField.Name);
            });

            Mock<ILogger<AssistanceService>> logger = mr.Create<ILogger<AssistanceService>>();

            Mock<IDataFeatures> dataFeatures = mr.Create<IDataFeatures>();
            dataFeatures.Setup(x => x.CLA4870_DSCAssistance.GetValue()).Returns(true);

            DomainServiceCommonDependency<AssistanceService> commonDependency = new DomainServiceCommonDependency<AssistanceService>(logger.Object, dataFeatures.Object);

            AssistanceService assistanceService = new AssistanceService(jiraService.Object, userService.Object, commonDependency);

            AddAssistanceDto addAssistanceDto = new AddAssistanceDto
            {
                Summary = "Summary",
                Description = "Description",
                CurrentPage = "data.sentry.com/Search/Datasets"
            };

            AddAssistanceResultDto result = await assistanceService.AddAssistanceAsync(addAssistanceDto);

            Assert.AreEqual("CLA-000", result.IssueKey);
            Assert.AreEqual("https://jira.sentry.com/browse/CLA-000", result.IssueLink);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task AddAssistanceAsync_UserNotExists_AddAssistanceResultDto()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IApplicationUser> user = new Mock<IApplicationUser>();
            user.SetupGet(x => x.AssociateId).Returns("000000");
            user.SetupGet(x => x.EmailAddress).Returns("foo.bar@sentry.com");
            user.SetupGet(x => x.DisplayName).Returns("bar, foo");

            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(user.Object);

            Mock<IJiraService> jiraService = mr.Create<IJiraService>();
            jiraService.Setup(x => x.JiraUserExistsAsync("000000")).ReturnsAsync(false);
            jiraService.Setup(x => x.CreateJiraTicketAsync(It.IsAny<JiraTicket>())).ReturnsAsync("CLA-000").Callback<JiraTicket>(x =>
            {
                Assert.AreEqual(JiraValues.ProjectKeys.CLA, x.Project);
                Assert.AreEqual(JiraValues.IssueTypes.SUPPORT_REQUEST, x.IssueType);
                Assert.AreEqual("Summary", x.Summary);
                Assert.AreEqual(1, x.Labels.Count);
                Assert.AreEqual(JiraValues.Labels.ASSISTANCE, x.Labels.First());
                Assert.AreEqual(1, x.CustomFields.Count);
                Assert.IsNotNull(x.Reporter);
                Assert.IsTrue(x.Description.Contains("Description"));
                Assert.IsTrue(x.Description.Contains("*Current Page:* data.sentry.com/Search/Datasets"));
                Assert.IsTrue(x.Description.Contains("*DSC Environment:* DEV"));
                Assert.IsTrue(x.Description.Contains("*Dataset Name:* Dataset Name"));
                Assert.IsTrue(x.Description.Contains("*Schema Name:* Schema Name"));
                Assert.IsTrue(x.Description.Contains(@"*Reporter:* 000000 \- bar, foo"));
                Assert.IsTrue(x.Description.Contains("*Reporter Email:* foo.bar@sentry.com"));

                JiraCustomField customField = x.CustomFields.First();
                Assert.AreEqual(JiraValues.CustomFieldNames.SYSTEM, customField.Name);
            });

            Mock<ILogger<AssistanceService>> logger = mr.Create<ILogger<AssistanceService>>();

            Mock<IDataFeatures> dataFeatures = mr.Create<IDataFeatures>();
            dataFeatures.Setup(x => x.CLA4870_DSCAssistance.GetValue()).Returns(true);

            DomainServiceCommonDependency<AssistanceService> commonDependency = new DomainServiceCommonDependency<AssistanceService>(logger.Object, dataFeatures.Object);

            AssistanceService assistanceService = new AssistanceService(jiraService.Object, userService.Object, commonDependency);

            AddAssistanceDto addAssistanceDto = new AddAssistanceDto
            {
                Summary = "Summary",
                Description = "Description",
                CurrentPage = "data.sentry.com/Search/Datasets",
                DatasetName = "Dataset Name",
                SchemaName = "Schema Name"
            };

            AddAssistanceResultDto result = await assistanceService.AddAssistanceAsync(addAssistanceDto);

            Assert.AreEqual("CLA-000", result.IssueKey);
            Assert.AreEqual("https://jira.sentry.com/browse/CLA-000", result.IssueLink);

            mr.VerifyAll();
        }
    }
}
