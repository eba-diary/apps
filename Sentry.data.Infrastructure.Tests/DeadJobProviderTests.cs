using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Sentry.data.Core;
using Sentry.data.Core.Interfaces;
using System.Threading;
using System;
using Moq;
using System.Collections.Generic;
using System.Data;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class DeadJobProviderTests
    {
        [TestMethod]
        public void GetDeadSparkJobDtos_Returns_DtoList()
        {
            //Arrange
            Mock<IDbExecuter> mockExecuter = new Mock<IDbExecuter>();

            DeadJobProvider deadJobProvider = new DeadJobProvider(mockExecuter.Object);

            DataTable dataTable = new DataTable();

            dataTable.Columns.Add("Submission_id");
            dataTable.Columns.Add("sub_Created");
            dataTable.Columns.Add("Dataset_NME");
            dataTable.Columns.Add("Schema_NME");
            dataTable.Columns.Add("SourceBucketName");
            dataTable.Columns.Add("SourceKey");
            dataTable.Columns.Add("TargetKey");
            dataTable.Columns.Add("BatchId");
            dataTable.Columns.Add("State");
            dataTable.Columns.Add("LivyAppId");
            dataTable.Columns.Add("LivyDriverlogUrl");
            dataTable.Columns.Add("LivySparkUiUrl");
            dataTable.Columns.Add("Day of Month");
            dataTable.Columns.Add("Hour of Day");
            dataTable.Columns.Add("TriggerKey");
            dataTable.Columns.Add("TriggerBucket");
            dataTable.Columns.Add("ExecutionGuid");
            dataTable.Columns.Add("Dataset_ID");
            dataTable.Columns.Add("Schema_ID");
            dataTable.Columns.Add("DatasetFile_ID");
            dataTable.Columns.Add("DataFlow_ID");
            dataTable.Columns.Add("DataFlowStep_ID");

            dataTable.Rows.Add(1, DateTime.Today, "DatasetName", 
                "SchemaName", "SourceBucketName", "SourceKey", 
                "_SUCCESS/TargetKey", 1, "Dead", 
                "LivyAppID", "LivyDriverlogUrl", "LivySparkUiUrl", 
                1, 1, "TriggerKey",
                "TriggerBucket", "FlowExecutionGuid", 1,
                1, 1, 1, 1);

            dataTable.Rows.Add(2, DateTime.Today, "DatasetName",
                "SchemaName", "SourceBucketName", "SourceKey",
                "TargetKey", 2, "Dead",
                "LivyAppID", "LivyDriverlogUrl", "LivySparkUiUrl",
                2, 2, "TriggerKey",
                "TriggerBucket", "FlowExecutionGuid", 2,
                2, 2, 2, 2);


            mockExecuter.Setup(e => e.ExecuteQuery(-10)).Returns(dataTable);

            //Act
            List<DeadSparkJobDto> deadSparkJobDtoList = deadJobProvider.GetDeadSparkJobDtos(-10);

            //Assert
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

        [TestMethod]
        public void GetDeadSparkJobDtos_Returns_DtoList_RemovedWhiteSpace()
        {
            //Arrange
            Mock<IDbExecuter> mockExecuter = new Mock<IDbExecuter>();

            DeadJobProvider deadJobProvider = new DeadJobProvider(mockExecuter.Object);

            DataTable dataTable = new DataTable();

            dataTable.Columns.Add("Submission_id");
            dataTable.Columns.Add("sub_Created");
            dataTable.Columns.Add("Dataset_NME");
            dataTable.Columns.Add("Schema_NME");
            dataTable.Columns.Add("SourceBucketName");
            dataTable.Columns.Add("SourceKey");
            dataTable.Columns.Add("TargetKey");
            dataTable.Columns.Add("BatchId");
            dataTable.Columns.Add("State");
            dataTable.Columns.Add("LivyAppId");
            dataTable.Columns.Add("LivyDriverlogUrl");
            dataTable.Columns.Add("LivySparkUiUrl");
            dataTable.Columns.Add("Day of Month");
            dataTable.Columns.Add("Hour of Day");
            dataTable.Columns.Add("TriggerKey");
            dataTable.Columns.Add("TriggerBucket");
            dataTable.Columns.Add("ExecutionGuid");
            dataTable.Columns.Add("Dataset_ID");
            dataTable.Columns.Add("Schema_ID");
            dataTable.Columns.Add("DatasetFile_ID");
            dataTable.Columns.Add("DataFlow_ID");
            dataTable.Columns.Add("DataFlowStep_ID");

            dataTable.Rows.Add(1, DateTime.Today, "DatasetName     ",
                "SchemaName     ", "SourceBucketName", "SourceKey",
                "_SUCCESS/TargetKey", 1, "Dead",
                "LivyAppID", "LivyDriverlogUrl", "LivySparkUiUrl",
                1, 1, "TriggerKey",
                "TriggerBucket", "FlowExecutionGuid", 1,
                1, 1, 1, 1);


            mockExecuter.Setup(e => e.ExecuteQuery(-10)).Returns(dataTable);

            //Act
            List<DeadSparkJobDto> deadSparkJobDtoList = deadJobProvider.GetDeadSparkJobDtos(-10);

            //Assert
            Assert.AreEqual("DatasetName", deadSparkJobDtoList[0].DatasetName);
            Assert.AreEqual("SchemaName", deadSparkJobDtoList[0].SchemaName);

            Assert.AreNotEqual("DatasetName     ", deadSparkJobDtoList[0].DatasetName);
            Assert.AreNotEqual("SchemaName     ", deadSparkJobDtoList[0].SchemaName);
        }
    }
}
