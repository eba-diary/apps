using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

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
            Mock<IS3ServiceProvider> s3Provider = new Mock<IS3ServiceProvider>();

            List<DeadSparkJob> deadSparkJobsSetupList = new List<DeadSparkJob>();

            deadSparkJobsSetupList.Add(new DeadSparkJob()
            {
                SubmissionCreated = DateTime.Today,
                DatasetName = "DatasetName",
                SchemaName = "SchemaName",
                SourceKey = "SourceKey",
                TargetKey = "_SUCCESS",
                FlowExecutionGuid = "FlowExecutionGuid",
                SubmissionID = 1,
                SourceBucketName = "SourceBucketName1",
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
                FlowExecutionGuid = "FlowExecutionGuid",
                SubmissionID = 2,
                SourceBucketName = "SourceBucketName2",
                BatchID = 2,
                LivyAppID = "LivyAppID",
                LivyDriverlogUrl = "LivyDriverlogUrl",
                LivySparkUiUrl = "LivySparkUiUrl",
                DatasetFileID = 2,
                DataFlowStepID = 2
            });

            mockProvider.Setup(e => e.GetDeadSparkJobs(DateTime.Today,DateTime.Today)).Returns(deadSparkJobsSetupList);
            s3Provider.Setup(s3 => s3.ListObjects("SourceBucketName1", It.IsAny<string>(),null)).Returns(new List<string>(){ "_SUCCESS", "File" });
            s3Provider.Setup(s3 => s3.ListObjects("SourceBucketName2", It.IsAny<string>(), null)).Returns(new List<string>() { "File" });

            DeadSparkJobService jobService = new DeadSparkJobService(mockProvider.Object, s3Provider.Object);



            // Act
            List<DeadSparkJobDto> deadSparkJobDtoList = jobService.GetDeadSparkJobDtos(DateTime.Today, DateTime.Today);

            // Assert
            mockProvider.VerifyAll();

            Assert.AreEqual(DateTime.Today, deadSparkJobDtoList[0].SubmissionTime);
            Assert.AreEqual("DatasetName", deadSparkJobDtoList[0].DatasetName);
            Assert.AreEqual("SchemaName", deadSparkJobDtoList[0].SchemaName);
            Assert.AreEqual("SourceKey", deadSparkJobDtoList[0].SourceKey);
            Assert.AreEqual("FlowExecutionGuid", deadSparkJobDtoList[0].FlowExecutionGuid);
            Assert.AreEqual(false, deadSparkJobDtoList[0].ReprocessingRequired);
            Assert.AreEqual(1, deadSparkJobDtoList[0].SubmissionID);
            Assert.AreEqual("SourceBucketName1", deadSparkJobDtoList[0].SourceBucketName);
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
            Assert.AreEqual(true, deadSparkJobDtoList[1].ReprocessingRequired);
            Assert.AreEqual(2, deadSparkJobDtoList[1].SubmissionID);
            Assert.AreEqual("SourceBucketName2", deadSparkJobDtoList[1].SourceBucketName);
            Assert.AreEqual(2, deadSparkJobDtoList[1].BatchID);
            Assert.AreEqual("LivyAppID", deadSparkJobDtoList[1].LivyAppID);
            Assert.AreEqual("LivyDriverlogUrl", deadSparkJobDtoList[1].LivyDriverlogUrl);
            Assert.AreEqual("LivySparkUiUrl", deadSparkJobDtoList[1].LivySparkUiUrl);
            Assert.AreEqual(2, deadSparkJobDtoList[1].DatasetFileID);
            Assert.AreEqual(2, deadSparkJobDtoList[1].DataFlowStepID);
        }
    }
}
