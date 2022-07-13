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

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class DataFlowMetricServiceTests
    {
        [TestMethod]
        public void DataFlowMetricService_ToDto_Mappings()
        {
            //Arrange
            DataFlowMetricEntity entity = Mock.Of<DataFlowMetricEntity>();
            var stubIElasticContext = new Mock<IElasticContext>();
            DataFlowMetricProvider provider = new DataFlowMetricProvider(stubIElasticContext.Object);
            var stubIDatasetContext = new Mock<IDatasetContext>();
            DataFlowMetricService dataFlowMetricService = new DataFlowMetricService(provider, stubIDatasetContext.Object);
            //Act
            DataFlowMetricDto dto = dataFlowMetricService.ToDto(entity);
            //Assert
            Assert.AreEqual(entity.SaidKeyCode, dto.SaidKeyCode);
            Assert.AreEqual(entity.StatusCode, dto.StatusCode);
            Assert.AreEqual(entity.QueryMadeDateTime, dto.QueryMadeDateTime);
            Assert.AreEqual(entity.SchemaId, dto.SchemaId);
            Assert.AreEqual(entity.EventContents, dto.EventContents);
            Assert.AreEqual(entity.TotalFlowSteps, dto.TotalFlowteps);
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
            //Act
            //Assert
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
