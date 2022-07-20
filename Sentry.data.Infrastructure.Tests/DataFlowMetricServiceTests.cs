﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class DataFlowMetricServiceTests
    {
        [TestMethod]
        public void DataFlowMetricService_ToDto_Mappings()
        {
            //Arrange
            ProducerS3DropAction action = new ProducerS3DropAction();
            List<DataFlowStep> flowSteps = new List<DataFlowStep>();
            DataFlowStep flowStep = new DataFlowStep();
            action.Name = "Flow Step Name";
            flowStep.Id = 1;
            flowStep.Action = action;
            flowSteps.Add(flowStep);

            var stubIElasticContext = new Mock<IElasticContext>();
            DataFlowMetricProvider provider = new DataFlowMetricProvider(stubIElasticContext.Object);
            var stubIDatasetContext = new Mock<IDatasetContext>();
            DataFlowMetricService dataFlowMetricService = new DataFlowMetricService(provider, stubIDatasetContext.Object);
            stubIDatasetContext.Setup(m => m.DataFlowStep).Returns(flowSteps.AsQueryable());

            DataFlowMetricEntity entity = new DataFlowMetricEntity()
            {
                SaidKeyCode = "DATA",
                StatusCode = "C",
                QueryMadeDateTime = DateTime.Now,
                SchemaId = 0,
                EventContents = "Event Contents",
                TotalFlowSteps = 0,
                FileModifiedDateTime = DateTime.Now,
                OriginalFileName = "file name",
                DatasetId = 0,
                CurrentFlowStep = 0,
                DataActionId = 0,
                DataFlowId = 0,
                Partition = 0,
                DataActionTypeId = 0,
                MessageKey = "message key",
                Duration = 0,
                Offset = 0,
                DataFlowName = "flow name",
                DataFlowStepId = 1,
                FlowExecutionGuid = "execution guid",
                FileSize = 0,
                EventMetricId = 0,
                StorageCode = "storage code",
                FileCreatedDateTime = DateTime.Now,
                RunInstanceGuid = "run instance guid",
                FileName = "file name",
                MetricGeneratedDateTime = DateTime.Now,
                DatasetFileId = 0,
                ProcessStartDateTime = DateTime.Now,


            };

            //Act
            DataFlowMetricDto dto = dataFlowMetricService.ToDto(entity);
            //Assert
            Assert.AreEqual(entity.SaidKeyCode, dto.SaidKeyCode);
            Assert.AreEqual(entity.StatusCode, dto.StatusCode);
            Assert.AreEqual(entity.QueryMadeDateTime, dto.QueryMadeDateTime);
            Assert.AreEqual(entity.SchemaId, dto.SchemaId);
            Assert.AreEqual(entity.EventContents, dto.EventContents);
            Assert.AreEqual(entity.TotalFlowSteps, dto.TotalFlowSteps);
            Assert.AreEqual(entity.FileModifiedDateTime, dto.FileModifiedDateTime);
            Assert.AreEqual(entity.OriginalFileName, dto.OriginalFileName);
            Assert.AreEqual(entity.DatasetId, dto.DatasetId);
            Assert.AreEqual(entity.CurrentFlowStep, dto.CurrentFlowStep);
            Assert.AreEqual(entity.DataActionId, dto.DataActionId);
            Assert.AreEqual(entity.DataFlowId, dto.DataFlowId);
            Assert.AreEqual(entity.Partition, dto.Partition);
            Assert.AreEqual(entity.DataActionTypeId, dto.DataActionTypeId);
            Assert.AreEqual(entity.MessageKey, dto.MessageKey);
            Assert.AreEqual(entity.Duration, dto.Duration);
            Assert.AreEqual(entity.Offset, dto.Offset);
            Assert.AreEqual(entity.DataFlowName, dto.DataFlowName);
            Assert.AreEqual(entity.DataFlowStepId, dto.DataFlowStepId);
            Assert.AreEqual(entity.FlowExecutionGuid, dto.FlowExecutionGuid);
            Assert.AreEqual(entity.FileSize, dto.FileSize);
            Assert.AreEqual(entity.EventMetricId, dto.EventMetricId);
            Assert.AreEqual(entity.StorageCode, dto.StorageCode);
            Assert.AreEqual(entity.FileCreatedDateTime, dto.FileCreatedDateTime);
            Assert.AreEqual(entity.RunInstanceGuid, dto.RunInstanceGuid);
            Assert.AreEqual(entity.FileName, dto.FileName);
            Assert.AreEqual(entity.MetricGeneratedDateTime, dto.MetricGeneratedDateTime);
            Assert.AreEqual(entity.DatasetFileId, dto.DatasetFileId);
            Assert.AreEqual(entity.ProcessStartDateTime, dto.ProcessStartDateTime);
            Assert.AreEqual("Flow Step Name", dto.DataFlowStepName);
            stubIDatasetContext.VerifyAll();
            stubIElasticContext.VerifyAll();
        }
        [TestMethod]
        public void DataFlowMetricService_GetMetricsList_EmptyInput()
        {
            //Arrange
            var stubIElasticContext = new Mock<IElasticContext>();
            DataFlowMetricProvider provider = new DataFlowMetricProvider(stubIElasticContext.Object);
            var stubIDatasetContext = new Mock<IDatasetContext>();
            DataFlowMetricService dataFlowMetricService = new DataFlowMetricService(provider, stubIDatasetContext.Object);
            List<DataFlowMetricEntity> dataFlowMetricEntities = new List<DataFlowMetricEntity>();
            //Act
            List<DataFlowMetricDto> dtoList = dataFlowMetricService.GetMetricList(dataFlowMetricEntities);
            //Assert
            Assert.IsTrue(dtoList.Count == 0);
            stubIDatasetContext.VerifyAll();
            stubIElasticContext.VerifyAll();
        }
        [TestMethod]
        public void DataFlowMetricService_GetFileMetricGroups_Mappings()
        {
            //Arrange
            var stubIElasticContext = new Mock<IElasticContext>();
            DataFlowMetricProvider provider = new DataFlowMetricProvider(stubIElasticContext.Object);
            var stubIDatasetContext = new Mock<IDatasetContext>();
            DataFlowMetricService dataFlowMetricService = new DataFlowMetricService(provider, stubIDatasetContext.Object);
            List<DataFlowMetricDto> dataFlowMetricDtos = new List<DataFlowMetricDto>();
            for(int x = 0; x < 3; x++)
            {
                Thread.Sleep(100);
                DataFlowMetricDto dto = new DataFlowMetricDto()
                {
                    DatasetFileId = 1,
                    MetricGeneratedDateTime = DateTime.Now,
                    FileName = "FileName",
                };
                dataFlowMetricDtos.Add(dto);
            }
            //Act
            List<DataFileFlowMetricsDto> dataFileFlowMetrics = dataFlowMetricService.GetFileMetricGroups(dataFlowMetricDtos);
            //Assert
            Assert.AreEqual(dataFlowMetricDtos[0], dataFileFlowMetrics[0].FlowEvents[0]);
            Assert.AreEqual(dataFlowMetricDtos[1], dataFileFlowMetrics[0].FlowEvents[1]);
            Assert.AreEqual(dataFlowMetricDtos[2], dataFileFlowMetrics[0].FlowEvents[2]);
            Assert.AreEqual(dataFlowMetricDtos[0].MetricGeneratedDateTime, dataFileFlowMetrics[0].FirstEventTime);
            Assert.AreEqual(dataFlowMetricDtos[2].MetricGeneratedDateTime, dataFileFlowMetrics[0].LastEventTime);
            Assert.AreEqual(dataFlowMetricDtos[0].FileName, dataFileFlowMetrics[0].FileName);
            Assert.AreEqual(dataFlowMetricDtos[1].FileName, dataFileFlowMetrics[0].FileName);
            Assert.AreEqual(dataFlowMetricDtos[2].FileName, dataFileFlowMetrics[0].FileName);
            Assert.AreEqual((dataFlowMetricDtos[2].MetricGeneratedDateTime - dataFlowMetricDtos[0].MetricGeneratedDateTime).TotalSeconds.ToString(), dataFileFlowMetrics[0].Duration);
            stubIDatasetContext.VerifyAll();
            stubIElasticContext.VerifyAll();

        }
        [TestMethod]
        public void DataFlowMetricService_GetFileMetricGroups_IconBooleans()
        {
            //Arrange
            var stubIElasticContext = new Mock<IElasticContext>();
            DataFlowMetricProvider provider = new DataFlowMetricProvider(stubIElasticContext.Object);
            var stubIDatasetContext = new Mock<IDatasetContext>();
            DataFlowMetricService dataFlowMetricService = new DataFlowMetricService(provider, stubIDatasetContext.Object);
            List<DataFlowMetricDto> dataFlowMetricDtos = new List<DataFlowMetricDto>();
            DataFlowMetricDto dto1 = new DataFlowMetricDto()
            {
                DatasetFileId = 1,
                FileName = "FileName1",
                MetricGeneratedDateTime = DateTime.Now,
                CurrentFlowStep = 5,
                TotalFlowSteps = 5,
                StatusCode = "C"
            };
            DataFlowMetricDto dto2 = new DataFlowMetricDto()
            {
                DatasetFileId = 2,
                FileName = "FileName2",
                MetricGeneratedDateTime = DateTime.Now,
                CurrentFlowStep = 4,
                TotalFlowSteps = 5,
                StatusCode = "C"
            };
            DataFlowMetricDto dto3 = new DataFlowMetricDto()
            {
                DatasetFileId = 3,
                FileName = "FileName3",
                MetricGeneratedDateTime = DateTime.Now,
                CurrentFlowStep = 4,
                TotalFlowSteps = 5,
                StatusCode = "F"
            };
            DataFlowMetricDto dto4 = new DataFlowMetricDto()
            {
                DatasetFileId = 4,
                FileName = "FileName4",
                MetricGeneratedDateTime = DateTime.Now,
                CurrentFlowStep = 5,
                TotalFlowSteps = 5,
                StatusCode = "F"
            };
            dataFlowMetricDtos.Add(dto1);
            dataFlowMetricDtos.Add(dto2);
            dataFlowMetricDtos.Add(dto3);
            dataFlowMetricDtos.Add(dto4);
            //Act
            List<DataFileFlowMetricsDto> dataFileFlowMetrics = dataFlowMetricService.GetFileMetricGroups(dataFlowMetricDtos);
            //Assert
            Assert.IsTrue(dataFileFlowMetrics[0].AllEventsPresent);
            Assert.IsTrue(dataFileFlowMetrics[0].AllEventsComplete);
            Assert.IsFalse(dataFileFlowMetrics[1].AllEventsPresent);
            Assert.IsTrue(dataFileFlowMetrics[1].AllEventsComplete);
            Assert.IsFalse(dataFileFlowMetrics[2].AllEventsPresent);
            Assert.IsFalse(dataFileFlowMetrics[2].AllEventsComplete);
            Assert.IsTrue(dataFileFlowMetrics[3].AllEventsPresent);
            Assert.IsFalse(dataFileFlowMetrics[3].AllEventsComplete);
            stubIDatasetContext.VerifyAll();
            stubIElasticContext.VerifyAll();
        }
        [TestMethod]
        public void DataFlowMetricService_GetFileMetricGroups_EmptyInput()
        {
            //Arrange
            var stubIElasticContext = new Mock<IElasticContext>();
            DataFlowMetricProvider provider = new DataFlowMetricProvider(stubIElasticContext.Object);
            var stubIDatasetContext = new Mock<IDatasetContext>();
            DataFlowMetricService dataFlowMetricService = new DataFlowMetricService(provider, stubIDatasetContext.Object);
            List<DataFlowMetricDto> dtoList = new List<DataFlowMetricDto>();
            //Act
            List<DataFileFlowMetricsDto> dataFileFlowMetricsDtos = dataFlowMetricService.GetFileMetricGroups(dtoList);
            //Assert
            Assert.IsTrue(dataFileFlowMetricsDtos.Count == 0);
            stubIDatasetContext.VerifyAll();
            stubIElasticContext.VerifyAll();
        }
        [TestMethod]
        public void DataFlowMetricService_SortFlowMetrics_EmptyInput()
        {
            //Arrange
            var stubIElasticContext = new Mock<IElasticContext>();
            DataFlowMetricProvider provider = new DataFlowMetricProvider(stubIElasticContext.Object);
            var stubIDatasetContext = new Mock<IDatasetContext>();
            DataFlowMetricService dataFlowMetricService = new DataFlowMetricService(provider, stubIDatasetContext.Object);
            List<DataFileFlowMetricsDto> dtoList = new List<DataFileFlowMetricsDto>();
            //Act
            List<DataFileFlowMetricsDto> sortedList = dataFlowMetricService.SortFlowMetrics(dtoList);
            //Assert
            Assert.IsTrue(sortedList.Count == 0);
            stubIDatasetContext.VerifyAll();
            stubIElasticContext.VerifyAll();
        }
        [TestMethod]
        public void DataFlowMetricService_SortFlowMetrics_FlowEventsInOrder()
        {
            //Arrange
            var stubIElasticContext = new Mock<IElasticContext>();
            DataFlowMetricProvider provider = new DataFlowMetricProvider(stubIElasticContext.Object);
            var stubIDatasetContext = new Mock<IDatasetContext>();
            DataFlowMetricService dataFlowMetricService = new DataFlowMetricService(provider, stubIDatasetContext.Object);

            List<DataFlowMetricDto> flowEvents = new List<DataFlowMetricDto>();
            DataFlowMetricDto eventOne = new DataFlowMetricDto()
            {
                EventMetricId = 2
            };
            flowEvents.Add(eventOne);
            DataFlowMetricDto eventTwo = new DataFlowMetricDto()
            {
                EventMetricId = 5
            };
            flowEvents.Add(eventTwo);
            DataFlowMetricDto eventThree = new DataFlowMetricDto()
            {
                EventMetricId = 4
            };
            flowEvents.Add(eventThree);
            DataFlowMetricDto eventFour = new DataFlowMetricDto()
            {
                EventMetricId = 1
            };
            flowEvents.Add(eventFour);
            DataFlowMetricDto eventFive = new DataFlowMetricDto()
            {
                EventMetricId = 3
            };
            flowEvents.Add(eventFive);
            List<DataFileFlowMetricsDto> fileFlowMetrics = new List<DataFileFlowMetricsDto>();
            DataFileFlowMetricsDto fileGroup = new DataFileFlowMetricsDto();
            fileGroup.FlowEvents = flowEvents;
            fileFlowMetrics.Add(fileGroup);
            //Act
            List<DataFileFlowMetricsDto> sortedFileFlowMetrics = dataFlowMetricService.SortFlowMetrics(fileFlowMetrics);
            //Assert
            Assert.IsTrue(sortedFileFlowMetrics[0].FlowEvents[0].EventMetricId > sortedFileFlowMetrics[0].FlowEvents[1].EventMetricId);
            Assert.IsTrue(sortedFileFlowMetrics[0].FlowEvents[1].EventMetricId > sortedFileFlowMetrics[0].FlowEvents[2].EventMetricId);
            Assert.IsTrue(sortedFileFlowMetrics[0].FlowEvents[2].EventMetricId > sortedFileFlowMetrics[0].FlowEvents[3].EventMetricId);
            Assert.IsTrue(sortedFileFlowMetrics[0].FlowEvents[3].EventMetricId > sortedFileFlowMetrics[0].FlowEvents[4].EventMetricId);
            stubIDatasetContext.VerifyAll();
            stubIElasticContext.VerifyAll();
        }
        [TestMethod]
        public void DataFlowMetricService_SortFlowMetrics_FileGroupsInOrder()
        {
            //Arrange
            var stubIElasticContext = new Mock<IElasticContext>();
            DataFlowMetricProvider provider = new DataFlowMetricProvider(stubIElasticContext.Object);
            var stubIDatasetContext = new Mock<IDatasetContext>();
            DataFlowMetricService dataFlowMetricService = new DataFlowMetricService(provider, stubIDatasetContext.Object);

            List<DataFileFlowMetricsDto> fileGroups = new List<DataFileFlowMetricsDto>();
            DataFileFlowMetricsDto fileGroupOne = new DataFileFlowMetricsDto()
            {
                FirstEventTime = new DateTime(2022,7,15)
            };
            fileGroups.Add(fileGroupOne);
            DataFileFlowMetricsDto fileGroupTwo = new DataFileFlowMetricsDto()
            {
                FirstEventTime = new DateTime(2022, 7, 16)
            };
            fileGroups.Add(fileGroupTwo);
            DataFileFlowMetricsDto fileGroupThree = new DataFileFlowMetricsDto()
            {
                FirstEventTime = new DateTime(2022, 7, 14)
            };
            fileGroups.Add(fileGroupThree);
            DataFileFlowMetricsDto fileGroupFour = new DataFileFlowMetricsDto()
            {
                FirstEventTime = new DateTime(2022, 7, 13)
            };
            fileGroups.Add(fileGroupFour);
            DataFileFlowMetricsDto fileGroupFive = new DataFileFlowMetricsDto()
            {
                FirstEventTime = new DateTime(2022, 7, 17)
            };
            fileGroups.Add(fileGroupFive);
            //Act
            List<DataFileFlowMetricsDto> sortedFileGroups = dataFlowMetricService.SortFlowMetrics(fileGroups);
            //Assert
            Assert.IsTrue(sortedFileGroups[0].FirstEventTime > sortedFileGroups[1].FirstEventTime);
            Assert.IsTrue(sortedFileGroups[1].FirstEventTime > sortedFileGroups[2].FirstEventTime);
            Assert.IsTrue(sortedFileGroups[2].FirstEventTime > sortedFileGroups[3].FirstEventTime);
            Assert.IsTrue(sortedFileGroups[3].FirstEventTime > sortedFileGroups[4].FirstEventTime);
            stubIDatasetContext.VerifyAll();
            stubIElasticContext.VerifyAll();
        }
    }
}
