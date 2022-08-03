using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.Helpers.Paginate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sentry.data.Core.Interfaces;
using Sentry.data.Core;
using Nest;
using System.Threading;
using Sentry.data.Web;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class DataFlowMetricServiceTests
    {
        [TestMethod]
        public void GetDataFileFlowMetrics_EmptyReturn()
        {
            //arrange
            var stubDataFlowMetricProvider  = new Mock<IDataFlowMetricProvider>();
            var stubIDatasetContext = new Mock<IDatasetContext>();

            DataFlowMetricService dataFlowMetricService = new DataFlowMetricService(stubDataFlowMetricProvider.Object, stubIDatasetContext.Object);

            List<DataFlowMetric> entityList = new List<DataFlowMetric>();
            stubDataFlowMetricProvider.Setup(x => x.GetDataFlowMetrics(It.IsAny<DataFlowMetricSearchDto>())).Returns(entityList);
            DataFlowMetricSearchDto searchDto = new DataFlowMetricSearchDto();
            //act
            List<DataFileFlowMetricsDto> fileGroups = dataFlowMetricService.GetFileMetricGroups(searchDto);
            //assert
            Assert.AreEqual(0, fileGroups.Count);
            stubDataFlowMetricProvider.VerifyAll();
        }
        [TestMethod]
        public void GetDataFileFlowMetrics_GroupMappings()
        {
            //arrange
            var stubDataFlowMetricProvider = new Mock<IDataFlowMetricProvider>();
            var stubIDatasetContext = new Mock<IDatasetContext>();

            DataFlowMetricService dataFlowMetricService = new DataFlowMetricService(stubDataFlowMetricProvider.Object, stubIDatasetContext.Object);

            List<DataFlowStep> flowSteps = new List<DataFlowStep>();
            stubIDatasetContext.Setup(x => x.DataFlowStep).Returns(flowSteps.AsQueryable());
            List<DataFlowMetric> entityList = new List<DataFlowMetric>();
            DataFlowMetric entity1 = new DataFlowMetric()
            {
                EventMetricId = 2,
                EventContents = "contents",
                DatasetFileId = 1,
                FileName = "filename",
                EventMetricCreatedDateTime = new DateTime(100000),
                ExecutionOrder = 5,
                MaxExecutionOrder = 5,
                StatusCode = "C"

            };
            DataFlowMetric entity2 = new DataFlowMetric()
            {
                EventMetricId = 1,
                EventContents = "contents",
                DatasetFileId = 1,
                FileName = "filename",
                EventMetricCreatedDateTime = new DateTime(10000),
                ExecutionOrder = 4,
                MaxExecutionOrder = 5,
                StatusCode = "C"
            };

            entityList.Add(entity1);
            entityList.Add(entity2);


            stubDataFlowMetricProvider.Setup(x => x.GetDataFlowMetrics(It.IsAny<DataFlowMetricSearchDto>())).Returns(entityList);
            DataFlowMetricSearchDto searchDto = new DataFlowMetricSearchDto();
            //act
            List<DataFileFlowMetricsDto> fileGroups = dataFlowMetricService.GetFileMetricGroups(searchDto);
            DataFileFlowMetricsDto fileGroup = fileGroups[0];
            //assert
            Assert.AreEqual(entity2.EventMetricCreatedDateTime, fileGroup.FirstEventTime);
            Assert.AreEqual(entity1.EventMetricCreatedDateTime, fileGroup.LastEventTime);
            Assert.AreEqual(entity1.FileName, fileGroup.FileName);
            Assert.AreEqual(entity1.DatasetFileId, fileGroup.DatasetFileId);
            Assert.AreEqual("0.009", fileGroup.Duration);
            Assert.IsTrue(fileGroup.AllEventsComplete);
            Assert.IsTrue(fileGroup.AllEventsPresent);
            Assert.AreEqual(2, fileGroup.FlowEvents.Count);
            stubIDatasetContext.VerifyAll();
            stubDataFlowMetricProvider.VerifyAll();
        }
        [TestMethod]
        public void GetDataFileFlowMetrics_GroupOrder()
        {
            //arrange
            var stubDataFlowMetricProvider = new Mock<IDataFlowMetricProvider>();
            var stubIDatasetContext = new Mock<IDatasetContext>();

            DataFlowMetricService dataFlowMetricService = new DataFlowMetricService(stubDataFlowMetricProvider.Object, stubIDatasetContext.Object);

            List<DataFlowStep> flowSteps = new List<DataFlowStep>();
            stubIDatasetContext.Setup(x => x.DataFlowStep).Returns(flowSteps.AsQueryable());
            List<DataFlowMetric> entityList = new List<DataFlowMetric>();
            DataFlowMetric entity1 = new DataFlowMetric()
            {
                EventMetricId = 2,
                EventContents = "contents",
                DatasetFileId = 1,
                FileName = "filename",
                EventMetricCreatedDateTime = new DateTime(100000),
                ExecutionOrder = 5,
                MaxExecutionOrder = 5,
                StatusCode = "C"

            };
            DataFlowMetric entity2 = new DataFlowMetric()
            {
                EventMetricId = 1,
                EventContents = "contents",
                DatasetFileId = 2,
                FileName = "filename2",
                EventMetricCreatedDateTime = new DateTime(10000),
                ExecutionOrder = 5,
                MaxExecutionOrder = 5,
                StatusCode = "C"
            };

            entityList.Add(entity1);
            entityList.Add(entity2);


            stubDataFlowMetricProvider.Setup(x => x.GetDataFlowMetrics(It.IsAny<DataFlowMetricSearchDto>())).Returns(entityList);
            DataFlowMetricSearchDto searchDto = new DataFlowMetricSearchDto();
            //act
            List<DataFileFlowMetricsDto> fileGroups = dataFlowMetricService.GetFileMetricGroups(searchDto);
            //assert
            Assert.AreEqual(entity1.FileName, fileGroups[0].FileName);
            Assert.AreEqual(entity2.FileName, fileGroups[1].FileName);
            stubIDatasetContext.VerifyAll();
            stubDataFlowMetricProvider.VerifyAll();
        }
        [TestMethod]
        public void GetDataFileFlowMetrics_EventMappings()
        {
            //arrange
            var stubDataFlowMetricProvider = new Mock<IDataFlowMetricProvider>();
            var stubIDatasetContext = new Mock<IDatasetContext>();

            DataFlowMetricService dataFlowMetricService = new DataFlowMetricService(stubDataFlowMetricProvider.Object, stubIDatasetContext.Object);

            List<DataFlowStep> flowSteps = new List<DataFlowStep>();
            stubIDatasetContext.Setup(x => x.DataFlowStep).Returns(flowSteps.AsQueryable());
            List<DataFlowMetric> entityList = new List<DataFlowMetric>();
            DateTime time = DateTime.Now;
            DataFlowMetric entity1 = new DataFlowMetric()
            {
                QueryMadeDateTime = time,
                SchemaId = 1,
                EventContents = "contents",
                MaxExecutionOrder = 5,
                FileModifiedDateTime = time,
                OriginalFileName = "name",
                DatasetId = 1,
                ExecutionOrder = 5,
                DataActionId = 1,
                DataFlowId = 1,
                Partition = 1,
                DataActionTypeId = 1,
                MessageKey = "message",
                Duration = 1,
                Offset = 1,
                DataFlowName = "flowname",
                DataFlowStepId = 1,
                FlowExecutionGuid = "executionguid",
                FileSize = 1,
                EventMetricId = 1,
                StorageCode = "storagecode",
                FileCreatedDateTime = time,
                RunInstanceGuid = "instanceguid",
                FileName = "name",
                SaidKeyCode = "keycode",
                EventMetricCreatedDateTime = time,
                DatasetFileId = 1,
                ProcessStartDateTime = time,
                StatusCode = "statuscode",
            };
            entityList.Add(entity1);
            DataFlowMetricSearchDto searchDto = new DataFlowMetricSearchDto();
            stubDataFlowMetricProvider.Setup(x => x.GetDataFlowMetrics(It.IsAny<DataFlowMetricSearchDto>())).Returns(entityList);
            //act
            List<DataFileFlowMetricsDto> fileGroups = dataFlowMetricService.GetFileMetricGroups(searchDto);
            DataFlowMetricDto metricDto = fileGroups[0].FlowEvents[0];
            //assert
            Assert.AreEqual(time, metricDto.QueryMadeDateTime);
            Assert.AreEqual(1, metricDto.SchemaId);
            Assert.AreEqual("contents", metricDto.EventContents);
            Assert.AreEqual(5, metricDto.TotalFlowSteps);
            Assert.AreEqual(time, metricDto.FileModifiedDateTime);
            Assert.AreEqual("name", metricDto.OriginalFileName);
            Assert.AreEqual(1, metricDto.DatasetId);
            Assert.AreEqual(5, metricDto.CurrentFlowStep);
            Assert.AreEqual(1, metricDto.DataActionId);
            Assert.AreEqual(1, metricDto.DataFlowId);
            Assert.AreEqual(1, metricDto.Partition);
            Assert.AreEqual(1, metricDto.DataActionTypeId);
            Assert.AreEqual("message", metricDto.MessageKey);
            Assert.AreEqual(1, metricDto.Duration);
            Assert.AreEqual(1, metricDto.Offset);
            Assert.AreEqual("flowname", metricDto.DataFlowName);
            Assert.AreEqual(1, metricDto.DataFlowStepId);
            Assert.AreEqual("executionguid", metricDto.FlowExecutionGuid);
            Assert.AreEqual(1, metricDto.FileSize);
            Assert.AreEqual(1, metricDto.EventMetricId);
            Assert.AreEqual("storagecode", metricDto.StorageCode);
            Assert.AreEqual(time, metricDto.FileCreatedDateTime);
            Assert.AreEqual("instanceguid", metricDto.RunInstanceGuid);
            Assert.AreEqual("name", metricDto.FileName);
            Assert.AreEqual("keycode", metricDto.SaidKeyCode);
            Assert.AreEqual(time, metricDto.MetricGeneratedDateTime);
            Assert.AreEqual(1, metricDto.DatasetFileId);
            Assert.AreEqual(time, metricDto.ProcessStartDateTime);
            Assert.AreEqual("statuscode", metricDto.StatusCode);
            stubIDatasetContext.VerifyAll();
            stubDataFlowMetricProvider.VerifyAll();

        }
        [TestMethod]
        public void GetDataFileFlowMetrics_EventOrder()
        {
            //arrange
            var stubDataFlowMetricProvider = new Mock<IDataFlowMetricProvider>();
            var stubIDatasetContext = new Mock<IDatasetContext>();

            DataFlowMetricService dataFlowMetricService = new DataFlowMetricService(stubDataFlowMetricProvider.Object, stubIDatasetContext.Object);

            List<DataFlowStep> flowSteps = new List<DataFlowStep>();
            stubIDatasetContext.Setup(x => x.DataFlowStep).Returns(flowSteps.AsQueryable());
            List<DataFlowMetric> entityList = new List<DataFlowMetric>();
            DataFlowMetric entity1 = new DataFlowMetric()
            {
                EventMetricId = 2,


            };
            DataFlowMetric entity2 = new DataFlowMetric()
            {
                EventMetricId = 1,
  
            };
            DataFlowMetric entity3 = new DataFlowMetric()
            {
                EventMetricId = 3,

            };

            entityList.Add(entity1);
            entityList.Add(entity2);
            entityList.Add(entity3);


            stubDataFlowMetricProvider.Setup(x => x.GetDataFlowMetrics(It.IsAny<DataFlowMetricSearchDto>())).Returns(entityList);
            DataFlowMetricSearchDto searchDto = new DataFlowMetricSearchDto();
            //act
            List<DataFileFlowMetricsDto> fileGroups = dataFlowMetricService.GetFileMetricGroups(searchDto);
            DataFileFlowMetricsDto fileGroup = fileGroups[0];
            //assert
            Assert.AreEqual(entity3.EventMetricId, fileGroup.FlowEvents[0].EventMetricId);
            Assert.AreEqual(entity1.EventMetricId, fileGroup.FlowEvents[1].EventMetricId);
            Assert.AreEqual(entity2.EventMetricId, fileGroup.FlowEvents[2].EventMetricId);
            stubIDatasetContext.VerifyAll();
            stubDataFlowMetricProvider.VerifyAll();
        }
      
      
    }
}
