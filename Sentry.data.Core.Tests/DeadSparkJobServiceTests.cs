using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core.DomainServices;
using System;
using System.Collections.Generic;
using System.Data;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class DeadSparkJobServiceTests
    {
        [TestMethod]
        public void GetDeadSparkJobDtos_Returns_Dto_List()
        {
            // Arrange
            Mock<IDeadJobProvider> mockProvider = new Mock<IDeadJobProvider>();

            List<DeadSparkJobDto> deadSparkJobDtoSetupList = new List<DeadSparkJobDto>();

            deadSparkJobDtoSetupList.Add(new DeadSparkJobDto()
            {
                SubmissionTime = DateTime.Today,
                DatasetName = "DatasetName",
                SchemaName = "SchemaName",
                SourceKey = "SourceKey",
                FlowExecutionGuid = "FlowExecutionGuid",
                ReprocessingRequired = "Yes",
                SubmissionID = 1,
                SourceBucketName = "SourceBucketName",
                BatchID = 1,
                LivyAppID = "LivyAppID",
                LivyDriverlogUrl = "LivyDriverlogUrl",
                LivySparkUiUrl = "LivySparkUiUrl",
                DatasetFileID = 1,
                DataFlowStepID = 1
            });

            deadSparkJobDtoSetupList.Add(new DeadSparkJobDto()
            {
                SubmissionTime = DateTime.Today,
                DatasetName = "DatasetName",
                SchemaName = "SchemaName",
                SourceKey = "SourceKey",
                FlowExecutionGuid = "FlowExecutionGuid",
                ReprocessingRequired = "No",
                SubmissionID = 2,
                SourceBucketName = "SourceBucketName",
                BatchID = 2,
                LivyAppID = "LivyAppID",
                LivyDriverlogUrl = "LivyDriverlogUrl",
                LivySparkUiUrl = "LivySparkUiUrl",
                DatasetFileID = 2,
                DataFlowStepID = 2
            });

            mockProvider.Setup(e => e.GetDeadSparkJobDtos(-10)).Returns(deadSparkJobDtoSetupList);
            DeadSparkJobService jobService = new DeadSparkJobService(mockProvider.Object);


            // Act
            List<DeadSparkJobDto> deadSparkJobDtoList = jobService.GetDeadSparkJobDtos(-10);

            // Assert
            Assert.AreEqual(deadSparkJobDtoList[0].SubmissionTime, DateTime.Today);
            Assert.AreEqual(deadSparkJobDtoList[0].DatasetName, "DatasetName");
            Assert.AreEqual(deadSparkJobDtoList[0].SchemaName, "SchemaName");
            Assert.AreEqual(deadSparkJobDtoList[0].SourceKey, "SourceKey");
            Assert.AreEqual(deadSparkJobDtoList[0].FlowExecutionGuid, "FlowExecutionGuid");
            Assert.AreEqual(deadSparkJobDtoList[0].ReprocessingRequired, "Yes");
            Assert.AreEqual(deadSparkJobDtoList[0].SubmissionID, 1);
            Assert.AreEqual(deadSparkJobDtoList[0].SourceBucketName, "SourceBucketName");
            Assert.AreEqual(deadSparkJobDtoList[0].BatchID, 1);
            Assert.AreEqual(deadSparkJobDtoList[0].LivyAppID, "LivyAppID");
            Assert.AreEqual(deadSparkJobDtoList[0].LivyDriverlogUrl, "LivyDriverlogUrl");
            Assert.AreEqual(deadSparkJobDtoList[0].LivySparkUiUrl, "LivySparkUiUrl");
            Assert.AreEqual(deadSparkJobDtoList[0].DatasetFileID, 1);
            Assert.AreEqual(deadSparkJobDtoList[0].DataFlowStepID, 1);

            Assert.AreEqual(deadSparkJobDtoList[1].SubmissionTime, DateTime.Today);
            Assert.AreEqual(deadSparkJobDtoList[1].DatasetName, "DatasetName");
            Assert.AreEqual(deadSparkJobDtoList[1].SchemaName, "SchemaName");
            Assert.AreEqual(deadSparkJobDtoList[1].SourceKey, "SourceKey");
            Assert.AreEqual(deadSparkJobDtoList[1].FlowExecutionGuid, "FlowExecutionGuid");
            Assert.AreEqual(deadSparkJobDtoList[1].ReprocessingRequired, "No");
            Assert.AreEqual(deadSparkJobDtoList[1].SubmissionID, 2);
            Assert.AreEqual(deadSparkJobDtoList[1].SourceBucketName, "SourceBucketName");
            Assert.AreEqual(deadSparkJobDtoList[1].BatchID, 2);
            Assert.AreEqual(deadSparkJobDtoList[1].LivyAppID, "LivyAppID");
            Assert.AreEqual(deadSparkJobDtoList[1].LivyDriverlogUrl, "LivyDriverlogUrl");
            Assert.AreEqual(deadSparkJobDtoList[1].LivySparkUiUrl, "LivySparkUiUrl");
            Assert.AreEqual(deadSparkJobDtoList[1].DatasetFileID, 2);
            Assert.AreEqual(deadSparkJobDtoList[1].DataFlowStepID, 2);
        }
    }
}
