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
            Assert.AreEqual(deadSparkJobDtoList[0].DatasetName, "DatasetName");
            Assert.AreEqual(deadSparkJobDtoList[0].SchemaName, "SchemaName");

            Assert.AreNotEqual(deadSparkJobDtoList[0].DatasetName, "DatasetName     ");
            Assert.AreNotEqual(deadSparkJobDtoList[0].SchemaName, "SchemaName     ");
        }
    }
}
