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

            };
            DataFlowMetric entity2 = new DataFlowMetric()
            {

            };
            DataFlowMetric entity3 = new DataFlowMetric()
            {

            };
            DataFlowMetric entity4 = new DataFlowMetric()
            {

            };
            entityList.Add(entity1);
            entityList.Add(entity2);
            entityList.Add(entity3);
            entityList.Add(entity4);

            stubDataFlowMetricProvider.Setup(x => x.GetDataFlowMetrics(It.IsAny<DataFlowMetricSearchDto>())).Returns(entityList);
            DataFlowMetricSearchDto searchDto = new DataFlowMetricSearchDto();
            //act
            List<DataFileFlowMetricsDto> fileGroups = dataFlowMetricService.GetFileMetricGroups(searchDto);
            //assert
        }
        [TestMethod]
        public void GetDataFileFlowMetrics_GroupOrder()
        {
            //arrange
            //act
            //assert
        }
        [TestMethod]
        public void GetDataFileFlowMetrics_EventMappings()
        {
            //arrange
            //act
            //assert
        }
        [TestMethod]
        public void GetDataFileFlowMetrics_EventOrder()
        {
            //arrange
            //act
            //assert
        }
        [TestMethod]
        public void DataFileFlowMetricDtoList_ToModels_Mappings()
        {
            //arrange
            //act
            //assert
        }
        [TestMethod]
        public void DataFileFlowMetricDtoList_ToModels_EmptyInput()
        {
            //arrange
            //act
            //assert
        }
        [TestMethod]
        public void DataFlowMetricDtoList_ToModels_Mappings()
        {
            //arrange
            //act
            //assert
        }
        [TestMethod]
        public void DataFlowMetricDtoList_ToModels_EmptyInput()
        {
            //arrange
            //act
            //assert
        }
      
    }
}
