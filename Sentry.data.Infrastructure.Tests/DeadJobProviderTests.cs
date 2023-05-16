using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Sentry.data.Core;
using Sentry.data.Core.Interfaces;
using Sentry.data.Infrastructure;
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
            dataTable.Columns.Add("FlowExecutionGuid");
            dataTable.Columns.Add("RunInstanceGuid");
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
                "TriggerBucket", "FlowExecutionGuid", "RunInstanceGuid", 1,
                1, 1, 1, 1);

            dataTable.Rows.Add(2, DateTime.Today, "DatasetName",
                "SchemaName", "SourceBucketName", "SourceKey",
                "TargetKey", 2, "Dead",
                "LivyAppID", "LivyDriverlogUrl", "LivySparkUiUrl",
                2, 2, "TriggerKey",
                "TriggerBucket", "FlowExecutionGuid", "RunInstanceGuid", 2,
                2, 2, 2, 2);


            mockExecuter.Setup(e => e.ExecuteQuery(DateTime.Today, DateTime.Now)).Returns(dataTable);

            //Act
            List<DeadSparkJob> deadSparkJobList = deadJobProvider.GetDeadSparkJobs(DateTime.Today, DateTime.Now);

            //Assert
            mockExecuter.VerifyAll();

            Assert.AreEqual(DateTime.Today, deadSparkJobList[0].SubmissionCreated);
            Assert.AreEqual("DatasetName", deadSparkJobList[0].DatasetName);
            Assert.AreEqual("SchemaName", deadSparkJobList[0].SchemaName);
            Assert.AreEqual("SourceKey", deadSparkJobList[0].SourceKey);
            Assert.AreEqual("FlowExecutionGuid", deadSparkJobList[0].FlowExecutionGuid);
            Assert.AreEqual("RunInstanceGuid", deadSparkJobList[0].RunInstanceGuid);
            Assert.AreEqual("_SUCCESS/TargetKey", deadSparkJobList[0].TargetKey);
            Assert.AreEqual(1, deadSparkJobList[0].SubmissionID);
            Assert.AreEqual("SourceBucketName", deadSparkJobList[0].SourceBucketName);
            Assert.AreEqual(1, deadSparkJobList[0].BatchID);
            Assert.AreEqual("LivyAppID", deadSparkJobList[0].LivyAppID);
            Assert.AreEqual("LivyDriverlogUrl", deadSparkJobList[0].LivyDriverlogUrl);
            Assert.AreEqual("LivySparkUiUrl", deadSparkJobList[0].LivySparkUiUrl);
            Assert.AreEqual(1, deadSparkJobList[0].DatasetFileID);
            Assert.AreEqual(1, deadSparkJobList[0].DataFlowStepID);

            Assert.AreEqual(DateTime.Today, deadSparkJobList[1].SubmissionCreated);
            Assert.AreEqual("DatasetName", deadSparkJobList[1].DatasetName);
            Assert.AreEqual("SchemaName", deadSparkJobList[1].SchemaName);
            Assert.AreEqual("SourceKey", deadSparkJobList[1].SourceKey);
            Assert.AreEqual("FlowExecutionGuid", deadSparkJobList[1].FlowExecutionGuid);
            Assert.AreEqual("RunInstanceGuid", deadSparkJobList[1].RunInstanceGuid);
            Assert.AreEqual("TargetKey", deadSparkJobList[1].TargetKey);
            Assert.AreEqual(2, deadSparkJobList[1].SubmissionID);
            Assert.AreEqual("SourceBucketName", deadSparkJobList[1].SourceBucketName);
            Assert.AreEqual(2, deadSparkJobList[1].BatchID);
            Assert.AreEqual("LivyAppID", deadSparkJobList[1].LivyAppID);
            Assert.AreEqual("LivyDriverlogUrl", deadSparkJobList[1].LivyDriverlogUrl);
            Assert.AreEqual("LivySparkUiUrl", deadSparkJobList[1].LivySparkUiUrl);
            Assert.AreEqual(2, deadSparkJobList[1].DatasetFileID);
            Assert.AreEqual(2, deadSparkJobList[1].DataFlowStepID);
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
            dataTable.Columns.Add("FlowExecutionGuid");
            dataTable.Columns.Add("RunInstanceGuid");
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
                "TriggerBucket", "FlowExecutionGuid", "RunInstanceGuid", 1,
                1, 1, 1, 1);


            mockExecuter.Setup(e => e.ExecuteQuery(DateTime.Today, DateTime.Now)).Returns(dataTable);
            

            //Act
            List<DeadSparkJob> deadSparkJobList = deadJobProvider.GetDeadSparkJobs(DateTime.Today, DateTime.Now);

            //Assert
            mockExecuter.VerifyAll();

            Assert.AreEqual("DatasetName", deadSparkJobList[0].DatasetName);
            Assert.AreEqual("SchemaName", deadSparkJobList[0].SchemaName);

            Assert.AreNotEqual("DatasetName     ", deadSparkJobList[0].DatasetName);
            Assert.AreNotEqual("SchemaName     ", deadSparkJobList[0].SchemaName);
        }
    }
}
