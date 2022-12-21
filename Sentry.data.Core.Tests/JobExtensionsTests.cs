using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Core.Entities.DataProcessing;
using System.Collections.Generic;
using System.Linq;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class JobExtensionsTests
    {
        [TestMethod]
        public void ToDto_RetrieverJob_RetrieverJobDto()
        {
            RetrieverJob entity = new RetrieverJob
            {
                Id = 1,
                Schedule = "* * * * *",
                RelativeUri = "parent/child",
                ExecutionParameters = new Dictionary<string, string>
                {
                    { "Param", "Value" }
                },
                DataSource = new HTTPSSource { Id = 2 },
                DatasetConfig = new DatasetFileConfig { ConfigId = 3 },
                DataFlow = new DataFlow { Id = 4 },
                JobOptions = new RetrieverJobOptions()
                {
                    SearchCriteria = "SearchCriteria",
                    TargetFileName = "TargetFile",
                    CreateCurrentFile = true,
                    FtpPattern = FtpPattern.None,
                    HttpOptions = new RetrieverJobOptions.HttpsOptions
                    {
                        Body = "Body",
                        RequestMethod = HttpMethods.get,
                        RequestDataFormat = HttpDataFormat.json,
                        PagingType = PagingType.PageNumber,
                        PageParameterName = "PageParameter"
                    }
                },
                RequestVariables = new List<RequestVariable>
                {
                    new RequestVariable {
                        VariableName = "VariableName",
                        VariableValue = "VariableValue",
                        VariableIncrementType = RequestVariableIncrementType.DailyExcludeToday
                    }
                }
            };

            RetrieverJobDto result = entity.ToDto();

            Assert.AreEqual(1, result.JobId);
            Assert.AreEqual("* * * * *", result.Schedule);
            Assert.AreEqual("parent/child", result.RelativeUri);
            Assert.AreEqual("Instant", result.ReadableSchedule);
            Assert.AreEqual("Body", result.HttpRequestBody);
            Assert.AreEqual("SearchCriteria", result.SearchCriteria);
            Assert.AreEqual("TargetFile", result.TargetFileName);
            Assert.IsTrue(result.CreateCurrentFile);
            Assert.AreEqual(2, result.DataSourceId);
            Assert.AreEqual(DataSourceDiscriminator.HTTPS_SOURCE, result.DataSourceType);
            Assert.AreEqual(3, result.DatasetFileConfig);
            Assert.AreEqual(4, result.DataFlow);
            Assert.AreEqual(HttpMethods.get, result.RequestMethod);
            Assert.AreEqual(HttpDataFormat.json, result.RequestDataFormat);
            Assert.AreEqual(FtpPattern.None, result.FtpPattern);
            Assert.AreEqual(1, result.ExecutionParameters.Count);
            Assert.AreEqual("Param", result.ExecutionParameters.First().Key);
            Assert.AreEqual("Value", result.ExecutionParameters.First().Value);
            Assert.AreEqual(PagingType.PageNumber, result.PagingType);
            Assert.AreEqual("PageParameter", result.PageParameterName);
            Assert.AreEqual(1, result.RequestVariables.Count);

            RequestVariableDto dto = result.RequestVariables.First();
            Assert.AreEqual("VariableName", dto.VariableName);
            Assert.AreEqual("VariableValue", dto.VariableValue);
            Assert.AreEqual(RequestVariableIncrementType.DailyExcludeToday, dto.VariableIncrementType);

        }

        [TestMethod]
        public void ToDto_RetrieverJob_Nulls_RetrieverJobDto()
        {
            RetrieverJob entity = new RetrieverJob
            {
                Id = 1,
                Schedule = "* * * * *",
                RelativeUri = "parent/child",
                ExecutionParameters = new Dictionary<string, string>
                {
                    { "Param", "Value" }
                },
                DataSource = new HTTPSSource { Id = 2 },
                DataFlow = new DataFlow { Id = 4 }
            };

            RetrieverJobDto result = entity.ToDto();

            Assert.AreEqual(1, result.JobId);
            Assert.AreEqual("* * * * *", result.Schedule);
            Assert.AreEqual("parent/child", result.RelativeUri);
            Assert.AreEqual("Instant", result.ReadableSchedule);
            Assert.IsNull(result.HttpRequestBody);
            Assert.IsNull(result.SearchCriteria);
            Assert.IsNull(result.TargetFileName);
            Assert.IsFalse(result.CreateCurrentFile);
            Assert.AreEqual(2, result.DataSourceId);
            Assert.AreEqual(DataSourceDiscriminator.HTTPS_SOURCE, result.DataSourceType);
            Assert.AreEqual(0, result.DatasetFileConfig);
            Assert.AreEqual(4, result.DataFlow);
            Assert.IsNull(result.RequestMethod);
            Assert.IsNull(result.RequestDataFormat);
            Assert.IsNull(result.FtpPattern);
            Assert.AreEqual(1, result.ExecutionParameters.Count);
            Assert.AreEqual("Param", result.ExecutionParameters.First().Key);
            Assert.AreEqual("Value", result.ExecutionParameters.First().Value);
            Assert.AreEqual(PagingType.None, result.PagingType);
            Assert.IsNull(result.PageParameterName);
        }

        [TestMethod]
        public void ToEntity_RequestVariableDto_RequestVariable()
        {
            RequestVariableDto dto = new RequestVariableDto
            {
                VariableName = "VariableName",
                VariableValue = "VariableValue",
                VariableIncrementType = RequestVariableIncrementType.DailyExcludeToday
            };

            RequestVariable entity = dto.ToEntity();

            Assert.AreEqual("VariableName", entity.VariableName);
            Assert.AreEqual("VariableValue", entity.VariableValue);
            Assert.AreEqual(RequestVariableIncrementType.DailyExcludeToday, entity.VariableIncrementType);
        }

        [TestMethod]
        public void ToDto_RequestVariable_RequestVariableDto()
        {
            RequestVariable entity = new RequestVariable
            {
                VariableName = "VariableName",
                VariableValue = "VariableValue",
                VariableIncrementType = RequestVariableIncrementType.DailyExcludeToday
            };

            RequestVariableDto dto = entity.ToDto();

            Assert.AreEqual("VariableName", dto.VariableName);
            Assert.AreEqual("VariableValue", dto.VariableValue);
            Assert.AreEqual(RequestVariableIncrementType.DailyExcludeToday, dto.VariableIncrementType);
        }
    }
}
