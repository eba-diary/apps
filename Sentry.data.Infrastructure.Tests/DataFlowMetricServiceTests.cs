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
            action.Id = 1;
            flowStep.Action = action;
            flowSteps.Add(flowStep);

            var stubIElasticContext = new Mock<IElasticContext>();
            DataFlowMetricProvider provider = new DataFlowMetricProvider(stubIElasticContext.Object);
            var stubIDatasetContext = new Mock<IDatasetContext>();
            DataFlowMetricService dataFlowMetricService = new DataFlowMetricService(provider, stubIDatasetContext.Object);
            stubIDatasetContext.Setup(m => m.DataFlowStep).Returns(flowSteps.AsQueryable());

            DataFlowMetricEntity entity = new DataFlowMetricEntity()
            {

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
            Assert.AreEqual(dataFlowMetricDtos[2].MetricGeneratedDateTime - dataFlowMetricDtos[0].MetricGeneratedDateTime, dataFileFlowMetrics[0].Duration);
        
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
        }
    }
}
