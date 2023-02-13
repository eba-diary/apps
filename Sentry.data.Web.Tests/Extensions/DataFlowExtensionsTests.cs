using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Core;
using System.Linq;
using System.Collections.Generic;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.Tests
{
    [TestClass]
    public class DataFlowExtensionsTests
    {
        [TestMethod]
        public void ToDto_RequestVariableModel_RequestVariableDto()
        {
            RequestVariableModel model = new RequestVariableModel
            {
                VariableName = "foo",
                VariableValue = "bar",
                VariableIncrementType = RequestVariableIncrementType.DailyExcludeToday
            };

            RequestVariableDto dto = model.ToDto();

            Assert.AreEqual("foo", dto.VariableName);
            Assert.AreEqual("bar", dto.VariableValue);
            Assert.AreEqual(RequestVariableIncrementType.DailyExcludeToday, dto.VariableIncrementType);
        }

        [TestMethod]
        public void ToModel_RequestVariableDto_RequestVariableModel()
        {
            RequestVariableDto dto = new RequestVariableDto
            {
                VariableName = "foo",
                VariableValue = "bar",
                VariableIncrementType = RequestVariableIncrementType.DailyExcludeToday
            };

            RequestVariableModel model = dto.ToModel();

            Assert.AreEqual("foo", model.VariableName);
            Assert.AreEqual("bar", model.VariableValue);
            Assert.AreEqual(RequestVariableIncrementType.DailyExcludeToday, model.VariableIncrementType);
        }

        [TestMethod]
        public void ToDto_JobModel_RetrieverJobDto()
        {
            JobModel model = new JobModel
            {
                SelectedDataSource = "1",
                SelectedSourceType = "SourceType",
                CreateCurrentFile = true,
                FtpPattern = FtpPattern.NoPattern,
                HttpRequestBody = "RequestBody",
                RelativeUri = "RelativeUri",
                SelectedRequestDataFormat = HttpDataFormat.json,
                SelectedRequestMethod = HttpMethods.post,
                Schedule = "* * * * *",
                SearchCriteria = "SearchCriteria",
                TargetFileName = "TargetFile",
                ExecutionParameters = new Dictionary<string, string>()
                {
                    { "Key", "Value" }
                },
                PagingType = PagingType.PageNumber,
                PageParameterName = "param",
                RequestVariables = new List<RequestVariableModel>
                {
                    new RequestVariableModel
                    {
                        VariableName = "foo",
                        VariableValue = "bar",
                        VariableIncrementType = RequestVariableIncrementType.DailyExcludeToday
                    }
                }
            };

            RetrieverJobDto dto = model.ToDto();

            Assert.AreEqual(1, dto.DataSourceId);
            Assert.AreEqual("SourceType", dto.DataSourceType);
            Assert.IsFalse(dto.IsCompressed);
            Assert.IsTrue(dto.CreateCurrentFile);
            Assert.AreEqual(0, dto.DatasetFileConfig);
            Assert.IsNull(dto.FileNameExclusionList);
            Assert.AreEqual(0, dto.FileSchema);
            Assert.AreEqual(FtpPattern.NoPattern, dto.FtpPattern);
            Assert.AreEqual("RequestBody", dto.HttpRequestBody);
            Assert.AreEqual(0, dto.JobId);
            Assert.AreEqual("RelativeUri", dto.RelativeUri);
            Assert.AreEqual(HttpDataFormat.json, dto.RequestDataFormat);
            Assert.AreEqual(HttpMethods.post, dto.RequestMethod);
            Assert.AreEqual("* * * * *", dto.Schedule);
            Assert.AreEqual("SearchCriteria", dto.SearchCriteria);
            Assert.AreEqual("TargetFile", dto.TargetFileName);
            Assert.AreEqual(1, dto.ExecutionParameters.Count);
            Assert.IsTrue(dto.ExecutionParameters.ContainsKey("Key"));
            Assert.AreEqual("Value", dto.ExecutionParameters["Key"]);
            Assert.AreEqual(PagingType.PageNumber, dto.PagingType);
            Assert.AreEqual("param", dto.PageParameterName);
            Assert.AreEqual(1, dto.RequestVariables.Count);
            Assert.AreEqual("foo", dto.RequestVariables.First().VariableName);
            Assert.AreEqual("bar", dto.RequestVariables.First().VariableValue);
            Assert.AreEqual(RequestVariableIncrementType.DailyExcludeToday, dto.RequestVariables.First().VariableIncrementType);
        }
    }
}
