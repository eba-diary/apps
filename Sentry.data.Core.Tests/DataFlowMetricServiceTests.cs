using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.Helpers.Paginate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class DataFlowMetricServiceTests
    {
        [TestMethod]
        public void DataFlowMetricService_ToDto_Mappings()
        {
            //Arrange
            DataFlowMetricEntity entity = MockClasses.MockDataFlowMetricEntity();
            //Act
            //Assert

        }
        [TestMethod]
        public void DataFlowMetricService_ToDto_EmptyInput()
        {
            //Arrange
            //Act
            //Assert
        }
        [TestMethod]
        public void DataFlowMetricService_GetMetricsList_Mappings()
        {
            //Arrange
            //Act
            //Assert
        }
        [TestMethod]
        public void DataFlowMetricService_GetMetricsList_EmptyInput()
        {
            //Arrange
            //Act
            //Assert
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
            //Act
            //Assert
        }
    }
}
