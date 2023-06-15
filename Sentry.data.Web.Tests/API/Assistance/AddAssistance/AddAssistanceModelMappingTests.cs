using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Core;
using Sentry.data.Web.API;

namespace Sentry.data.Web.Tests.API
{
    [TestClass]
    public class AddAssistanceModelMappingTests : BaseModelMappingTests
    {
        [TestMethod]
        public void Map_AddAssistanceRequestModel_To_AddAssistanceDto()
        {
            AddAssistanceRequestModel model = new AddAssistanceRequestModel
            {
                Summary = "Summary",
                Description = "Description",
                CurrentPage = "CurrentPage",
                DatasetName = "Dataset Name",
                SchemaName = "Schema Name"
            };

            AddAssistanceDto dto = _mapper.Map<AddAssistanceDto>(model);

            Assert.AreEqual("Summary", dto.Summary);
            Assert.AreEqual("Description", dto.Description);
            Assert.AreEqual("CurrentPage", dto.CurrentPage);
            Assert.AreEqual("Dataset Name", dto.DatasetName);
            Assert.AreEqual("Schema Name", dto.SchemaName);
        }

        [TestMethod]
        public void Map_AddAssistanceResultDto_To_AddAssistanceResponseModel()
        {
            AddAssistanceResultDto dto = new AddAssistanceResultDto
            {
                IssueKey = "CLA-000",
                IssueLink = "jira.sentry.com/CLA-000"
            };

            AddAssistanceResponseModel model = _mapper.Map<AddAssistanceResponseModel>(dto);

            Assert.AreEqual("CLA-000", model.IssueKey);
            Assert.AreEqual("jira.sentry.com/CLA-000", model.IssueLink);
        }
    }
}
