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

            List<DataFlowStep> flowSteps = new List<DataFlowStep>();
            stubIDatasetContext.Setup(x => x.DataFlowStep).Returns(flowSteps.AsQueryable());
            List<DataFlowMetric> entityList = new List<DataFlowMetric>();
            stubDataFlowMetricProvider.Setup(x => x.GetDataFlowMetrics(It.IsAny<DataFlowMetricSearchDto>())).Returns(entityList);
            DataFlowMetricSearchDto searchDto = new DataFlowMetricSearchDto();
            //act
            List<DataFileFlowMetricsDto> fileGroups = dataFlowMetricService.GetFileMetricGroups(searchDto);
            //assert
            Assert.AreEqual(0, fileGroups.Count);
        }
        [TestMethod]
        public void GetDataFileFlowMetrics_GroupMappings()
        {
            //arrange
            var stubIElasticContext = new Mock<IElasticContext>();
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
            //assert
            Assert.AreEqual(entity2.EventMetricCreatedDateTime, fileGroups[0].FirstEventTime);
            Assert.AreEqual(entity1.EventMetricCreatedDateTime, fileGroups[0].LastEventTime);
            Assert.AreEqual(entity1.FileName, fileGroups[0].FileName);
            Assert.AreEqual(entity1.DatasetFileId, fileGroups[0].DatasetFileId);
            Assert.AreEqual((entity1.EventMetricCreatedDateTime - entity2.EventMetricCreatedDateTime).TotalSeconds.ToString(), fileGroups[0].Duration);
            Assert.AreEqual(true, fileGroups[0].AllEventsComplete);
            Assert.AreEqual(true, fileGroups[0].AllEventsPresent);
            Assert.AreEqual(2, fileGroups[0].FlowEvents.Count);
        }
        [TestMethod]
        public void GetDataFileFlowMetrics_GroupOrder()
        {
            //arrange
            var stubIElasticContext = new Mock<IElasticContext>();
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
        }
        [TestMethod]
        public void GetDataFileFlowMetrics_EventMappings()
        {
            //arrange
            var stubIElasticContext = new Mock<IElasticContext>();
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
                EventContents = "",
                MaxExecutionOrder = 5,
                FileModifiedDateTime = time,
                OriginalFileName = "name",
                DatasetId = 1,
                ExecutionOrder = 5,
                DataActionId = 1,
                DataFlowId = 1,
                Partition = 1,
                DataActionTypeId = 1,
                MessageKey = "",
                Duration = 1,
                Offset = 1,
                DataFlowName = "",
                DataFlowStepId = 1,
                FlowExecutionGuid = "",
                FileSize = 1,
                EventMetricId = 1,
                StorageCode = "",
                FileCreatedDateTime = time,
                RunInstanceGuid = "",
                FileName = "name",
                SaidKeyCode = "",
                EventMetricCreatedDateTime = time,
                DatasetFileId = 1,
                ProcessStartDateTime = time,
                StatusCode = "",
            };
            entityList.Add(entity1);
            DataFlowMetricSearchDto searchDto = new DataFlowMetricSearchDto();
            stubDataFlowMetricProvider.Setup(x => x.GetDataFlowMetrics(It.IsAny<DataFlowMetricSearchDto>())).Returns(entityList);
            //act
            List<DataFileFlowMetricsDto> fileGroups = dataFlowMetricService.GetFileMetricGroups(searchDto);
            //assert
            Assert.AreEqual(time, fileGroups[0].FlowEvents[0].QueryMadeDateTime);
            Assert.AreEqual(1, fileGroups[0].FlowEvents[0].SchemaId);
            Assert.AreEqual("", fileGroups[0].FlowEvents[0].EventContents);
            Assert.AreEqual(5, fileGroups[0].FlowEvents[0].TotalFlowSteps);
            Assert.AreEqual(time, fileGroups[0].FlowEvents[0].FileModifiedDateTime);
            Assert.AreEqual("name", fileGroups[0].FlowEvents[0].OriginalFileName);
            Assert.AreEqual(1, fileGroups[0].FlowEvents[0].DatasetId);
            Assert.AreEqual(5, fileGroups[0].FlowEvents[0].CurrentFlowStep);
            Assert.AreEqual(1, fileGroups[0].FlowEvents[0].DataActionId);
            Assert.AreEqual(1, fileGroups[0].FlowEvents[0].DataFlowId);
            Assert.AreEqual(1, fileGroups[0].FlowEvents[0].Partition);
            Assert.AreEqual(1, fileGroups[0].FlowEvents[0].DataActionTypeId);
            Assert.AreEqual("", fileGroups[0].FlowEvents[0].MessageKey);
            Assert.AreEqual(1, fileGroups[0].FlowEvents[0].Duration);
            Assert.AreEqual(1, fileGroups[0].FlowEvents[0].Offset);
            Assert.AreEqual("", fileGroups[0].FlowEvents[0].DataFlowName);
            Assert.AreEqual(1, fileGroups[0].FlowEvents[0].DataFlowStepId);
            Assert.AreEqual("", fileGroups[0].FlowEvents[0].FlowExecutionGuid);
            Assert.AreEqual(1, fileGroups[0].FlowEvents[0].FileSize);
            Assert.AreEqual(1, fileGroups[0].FlowEvents[0].EventMetricId);
            Assert.AreEqual("", fileGroups[0].FlowEvents[0].StorageCode);
            Assert.AreEqual(time, fileGroups[0].FlowEvents[0].FileCreatedDateTime);
            Assert.AreEqual("", fileGroups[0].FlowEvents[0].RunInstanceGuid);
            Assert.AreEqual("name", fileGroups[0].FlowEvents[0].FileName);
            Assert.AreEqual("", fileGroups[0].FlowEvents[0].SaidKeyCode);
            Assert.AreEqual(time, fileGroups[0].FlowEvents[0].MetricGeneratedDateTime);
            Assert.AreEqual(1, fileGroups[0].FlowEvents[0].DatasetFileId);
            Assert.AreEqual(time, fileGroups[0].FlowEvents[0].ProcessStartDateTime);
            Assert.AreEqual("", fileGroups[0].FlowEvents[0].StatusCode);

        }
        [TestMethod]
        public void GetDataFileFlowMetrics_EventOrder()
        {
            //arrange
            var stubIElasticContext = new Mock<IElasticContext>();
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
            //assert
            Assert.AreEqual(entity3.EventMetricId, fileGroups[0].FlowEvents[0].EventMetricId);
            Assert.AreEqual(entity1.EventMetricId, fileGroups[0].FlowEvents[1].EventMetricId);
            Assert.AreEqual(entity2.EventMetricId, fileGroups[0].FlowEvents[2].EventMetricId);
        }
      
      
    }
}
