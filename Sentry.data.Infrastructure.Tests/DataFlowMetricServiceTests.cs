﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nest;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

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

        [TestMethod]
        public void GetDataFlowMetrics_MappedReturn()
        {
            //arrange
            var stubIElasticDocumentClient = new Mock<IElasticDocumentClient>();

            DataFlowMetricSearchDto dto = new DataFlowMetricSearchDto()
            {
                DatasetFileIds = new int[] { 1, 2, 3 },
                DatasetId = 1,
                SchemaId = 1
            };

            stubIElasticDocumentClient.Setup(x => x.SearchAsync(It.IsAny<Func<Nest.SearchDescriptor<DataFlowMetric>, ISearchRequest>>())).ReturnsAsync(GetDataFlowMetricList);

            DataFlowMetricProvider stubDataFlowMetricProvider = new DataFlowMetricProvider(stubIElasticDocumentClient.Object);

            //act
            List<DataFlowMetric> metricList = stubDataFlowMetricProvider.GetDataFlowMetrics(dto);

            //assert
            Assert.IsNotNull(metricList);

            stubIElasticDocumentClient.VerifyAll();
        }

        private ElasticResult<DataFlowMetric> GetDataFlowMetricList()
        {
            return new ElasticResult<DataFlowMetric>()
            {
                SearchTotal = 1,
                Documents = new List<DataFlowMetric>()
                {
                    new DataFlowMetric()
                    {
                        QueryMadeDateTime = DateTime.Now,
                        SchemaId = 1,
                        EventContents = "eventContents",
                        MaxExecutionOrder = 0,
                        FileModifiedDateTime = DateTime.Now,
                        OriginalFileName = "ogFileName",
                        DatasetId = 1,
                        ExecutionOrder = 1,
                        DataActionId = 1,
                        Partition = 1,
                        DataActionTypeId = 1,
                        MessageKey = "messageKey",
                        Duration = 1,
                        Offset = 0,
                        DataFlowName = "flowName",
                        DataFlowStepId = 1,
                        FlowExecutionGuid = "executionGuid",
                        FileSize = 1,
                        EventMetricId = 1,
                        StorageCode = "storageCode",
                        FileCreatedDateTime = DateTime.Now,
                        FileName = "fileName",
                        SaidKeyCode = "keyCode",
                        EventMetricCreatedDateTime = DateTime.Now,
                        DatasetFileId = 1,
                        ProcessStartDateTime = DateTime.Now,
                        StatusCode = "statusCode"
                    },
                    new DataFlowMetric()
                    {
                        QueryMadeDateTime = DateTime.Now,
                        SchemaId = 2,
                        EventContents = "eventContents",
                        MaxExecutionOrder = 0,
                        FileModifiedDateTime = DateTime.Now,
                        OriginalFileName = "ogFileName",
                        DatasetId = 2,
                        ExecutionOrder = 2,
                        DataActionId = 2,
                        Partition = 2,
                        DataActionTypeId = 2,
                        MessageKey = "messageKey",
                        Duration = 2,
                        Offset = 0,
                        DataFlowName = "flowName",
                        DataFlowStepId = 2,
                        FlowExecutionGuid = "executionGuid",
                        FileSize = 1,
                        EventMetricId = 2,
                        StorageCode = "storageCode",
                        FileCreatedDateTime = DateTime.Now,
                        FileName = "fileName",
                        SaidKeyCode = "keyCode",
                        EventMetricCreatedDateTime = DateTime.Now,
                        DatasetFileId = 2,
                        ProcessStartDateTime = DateTime.Now,
                        StatusCode = "statusCode"
                    }
        }
            };
        }
    }
}
