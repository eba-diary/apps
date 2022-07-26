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
            var stubIElasticContext = new Mock<IElasticContext>();
            DataFlowMetricProvider provider = new DataFlowMetricProvider(stubIElasticContext.Object);
            var stubIDatasetContext = new Mock<IDatasetContext>();
            DataFlowMetricService dataFlowMetricService = new DataFlowMetricService(provider, stubIDatasetContext.Object);

            //act
            //assert
        }
        public void GetDataFileFlowMetrics_Mappings()
        {
            //arrange
            //act
            //assert
        }
        public void DataFileFlowMetricDtoList_ToModels_Mappings()
        {
            //arrange
            //act
            //assert
        }
        public void DataFileFlowMetricDtoList_ToModels_EmptyInput()
        {
            //arrange
            //act
            //assert
        }
        public void DataFlowMetricDtoList_ToModels_Mappings()
        {
            //arrange
            //act
            //assert
        }
        public void DataFlowMetricDtoList_ToModels_EmptyInput()
        {
            //arrange
            //act
            //assert
        }
      
    }
}
