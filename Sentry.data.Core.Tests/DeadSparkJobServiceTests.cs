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

            List<DeadSparkJob> deadSparkJobsSetupList = new List<DeadSparkJob>();

            deadSparkJobsSetupList.Add(new DeadSparkJob()
            {
                SubmissionCreated = DateTime.Today,
                DatasetName = "DatasetName",
                SchemaName = "SchemaName",
                SourceKey = "SourceKey",
                TargetKey = "_SUCCESS",
                ExecutionGuid = "FlowExecutionGuid",
                SubmissionID = 1,
                SourceBucketName = "SourceBucketName",
                BatchID = 1,
                LivyAppID = "LivyAppID",
                LivyDriverlogUrl = "LivyDriverlogUrl",
                LivySparkUiUrl = "LivySparkUiUrl",
                DatasetFileID = 1,
                DataFlowStepID = 1
            });

            deadSparkJobsSetupList.Add(new DeadSparkJob()
            {
                SubmissionCreated = DateTime.Today,
                DatasetName = "DatasetName",
                SchemaName = "SchemaName",
                SourceKey = "SourceKey",
                TargetKey = "_FAILED",
                ExecutionGuid = "FlowExecutionGuid",
                SubmissionID = 2,
                SourceBucketName = "SourceBucketName",
                BatchID = 2,
                LivyAppID = "LivyAppID",
                LivyDriverlogUrl = "LivyDriverlogUrl",
                LivySparkUiUrl = "LivySparkUiUrl",
                DatasetFileID = 2,
                DataFlowStepID = 2
            });

            mockProvider.Setup(e => e.GetDeadSparkJobs(DateTime.Today)).Returns(deadSparkJobsSetupList);
            DeadSparkJobService jobService = new DeadSparkJobService(mockProvider.Object);



            // Act
            List<DeadSparkJobDto> deadSparkJobDtoList = jobService.GetDeadSparkJobDtos(DateTime.Today);

            // Assert
            mockProvider.VerifyAll();

            Assert.AreEqual(DateTime.Today, deadSparkJobDtoList[0].SubmissionTime);
            Assert.AreEqual("DatasetName", deadSparkJobDtoList[0].DatasetName);
            Assert.AreEqual("SchemaName", deadSparkJobDtoList[0].SchemaName);
            Assert.AreEqual("SourceKey", deadSparkJobDtoList[0].SourceKey);
            Assert.AreEqual("FlowExecutionGuid", deadSparkJobDtoList[0].FlowExecutionGuid);
            Assert.AreEqual("Yes", deadSparkJobDtoList[0].ReprocessingRequired);
            Assert.AreEqual(1, deadSparkJobDtoList[0].SubmissionID);
            Assert.AreEqual("SourceBucketName", deadSparkJobDtoList[0].SourceBucketName);
            Assert.AreEqual(1, deadSparkJobDtoList[0].BatchID);
            Assert.AreEqual("LivyAppID", deadSparkJobDtoList[0].LivyAppID);
            Assert.AreEqual("LivyDriverlogUrl", deadSparkJobDtoList[0].LivyDriverlogUrl);
            Assert.AreEqual("LivySparkUiUrl", deadSparkJobDtoList[0].LivySparkUiUrl);
            Assert.AreEqual(1, deadSparkJobDtoList[0].DatasetFileID);
            Assert.AreEqual(1, deadSparkJobDtoList[0].DataFlowStepID);

            Assert.AreEqual(DateTime.Today, deadSparkJobDtoList[1].SubmissionTime);
            Assert.AreEqual("DatasetName", deadSparkJobDtoList[1].DatasetName);
            Assert.AreEqual("SchemaName", deadSparkJobDtoList[1].SchemaName);
            Assert.AreEqual("SourceKey", deadSparkJobDtoList[1].SourceKey);
            Assert.AreEqual("FlowExecutionGuid", deadSparkJobDtoList[1].FlowExecutionGuid);
            Assert.AreEqual("No", deadSparkJobDtoList[1].ReprocessingRequired);
            Assert.AreEqual(2, deadSparkJobDtoList[1].SubmissionID);
            Assert.AreEqual("SourceBucketName", deadSparkJobDtoList[1].SourceBucketName);
            Assert.AreEqual(2, deadSparkJobDtoList[1].BatchID);
            Assert.AreEqual("LivyAppID", deadSparkJobDtoList[1].LivyAppID);
            Assert.AreEqual("LivyDriverlogUrl", deadSparkJobDtoList[1].LivyDriverlogUrl);
            Assert.AreEqual("LivySparkUiUrl", deadSparkJobDtoList[1].LivySparkUiUrl);
            Assert.AreEqual(2, deadSparkJobDtoList[1].DatasetFileID);
            Assert.AreEqual(2, deadSparkJobDtoList[1].DataFlowStepID);
        }
    }
}
