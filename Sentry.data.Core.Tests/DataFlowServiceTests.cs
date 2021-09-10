using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Interfaces.QuartermasterRestClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class DataFlowServiceTests
    {
        /// <summary>
        /// - Test that the DataFlowService.Validate() method correctly identifies a duplicate DataFlow name
        /// and responds with the correct validation result.
        /// - Also validates that when the QuartermasterService returns no Named Environments, no validation
        /// is done against the NamedEnvironment or NamedEnvironmentType
        /// </summary>
        [TestMethod]
        public async Task DataFlowService_Validate_DuplicateName_NoNamedEnvironments()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            var dataFlows = new[] { new DataFlow() { Name = "Foo" } };
            context.Setup(f => f.DataFlow).Returns(dataFlows.AsQueryable());

            var quartermasterClient = new Mock<IClient>();
            var namedEnvironmentList = new NamedEnvironment[0];
            quartermasterClient.Setup(f => f.NamedEnvironmentsGet2Async(It.IsAny<string>(), It.IsAny<ShowDeleted11>()).Result).Returns(namedEnvironmentList);

            var dataFlowService = new DataFlowService(context.Object, null, null, null, null, quartermasterClient.Object, null);
            var dataFlow = new DataFlowDto() { Name = "Foo" };

            // Act
            var result = await dataFlowService.Validate(dataFlow);

            // Assert
            Assert.AreEqual(1, result.ValidationResults.GetAll().Count);
            Assert.IsTrue(result.ValidationResults.Contains(DataFlow.ValidationErrors.nameMustBeUnique));
        }

        /// <summary>
        /// - Test that when the QuartermasterService *does* return Named Environments, but the given
        /// NamedEnvironment doesn't match what Quartermaster has, a validation error is returned
        /// </summary>
        [TestMethod]
        public async Task DataFlowService_Validate_InvalidNamedEnvironment()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            var dataFlows = new DataFlow[0];
            context.Setup(f => f.DataFlow).Returns(dataFlows.AsQueryable());

            var quartermasterClient = new Mock<IClient>();
            var namedEnvironmentList = new[] { new NamedEnvironment() { Name = "TEST", Environmenttype = "NonProd" } };
            quartermasterClient.Setup(f => f.NamedEnvironmentsGet2Async(It.IsAny<string>(), It.IsAny<ShowDeleted11>()).Result).Returns(namedEnvironmentList);

            var dataFlowService = new DataFlowService(context.Object, null, null, null, null, quartermasterClient.Object, null);
            var dataFlow = new DataFlowDto() { Name = "Bar", NamedEnvironment="PROD", NamedEnvironmentType = GlobalEnums.NamedEnvironmentType.Prod };

            // Act
            var result = await dataFlowService.Validate(dataFlow);

            // Assert
            Assert.AreEqual(1, result.ValidationResults.GetAll().Count);
            Assert.IsTrue(result.ValidationResults.Contains(DataFlow.ValidationErrors.namedEnvironmentInvalid));
        }

        /// <summary>
        /// - Test that when the QuartermasterService *does* return Named Environments, but the given
        /// NamedEnvironmentType doesn't match what Quartermaster has, a validation error is returned
        /// </summary>
        [TestMethod]
        public async Task DataFlowService_Validate_InvalidNamedEnvironmentType()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            var dataFlows = new DataFlow[0];
            context.Setup(f => f.DataFlow).Returns(dataFlows.AsQueryable());

            var quartermasterClient = new Mock<IClient>();
            var namedEnvironmentList = new[] { new NamedEnvironment() { Name = "TEST", Environmenttype = "NonProd" } };
            quartermasterClient.Setup(f => f.NamedEnvironmentsGet2Async(It.IsAny<string>(), It.IsAny<ShowDeleted11>()).Result).Returns(namedEnvironmentList);

            var dataFlowService = new DataFlowService(context.Object, null, null, null, null, quartermasterClient.Object, null);
            var dataFlow = new DataFlowDto() { Name = "Bar", NamedEnvironment = "TEST", NamedEnvironmentType = GlobalEnums.NamedEnvironmentType.Prod };

            // Act
            var result = await dataFlowService.Validate(dataFlow);

            // Assert
            Assert.AreEqual(1, result.ValidationResults.GetAll().Count);
            Assert.IsTrue(result.ValidationResults.Contains(DataFlow.ValidationErrors.namedEnvironmentTypeInvalid));
        }

        /// <summary>
        /// Tests successful validation of the DataFlowDto
        /// </summary>
        [TestMethod]
        public async Task DataFlowService_Validate_Success()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            var dataFlows = new DataFlow[0];
            context.Setup(f => f.DataFlow).Returns(dataFlows.AsQueryable());

            var quartermasterClient = new Mock<IClient>();
            var namedEnvironmentList = new[] { new NamedEnvironment() { Name = "TEST", Environmenttype = "NonProd" } };
            quartermasterClient.Setup(f => f.NamedEnvironmentsGet2Async(It.IsAny<string>(), It.IsAny<ShowDeleted11>()).Result).Returns(namedEnvironmentList);

            var dataFlowService = new DataFlowService(context.Object, null, null, null, null, quartermasterClient.Object, null);
            var dataFlow = new DataFlowDto() { Name = "Bar", NamedEnvironment = "TEST", NamedEnvironmentType = GlobalEnums.NamedEnvironmentType.NonProd };

            // Act
            var result = await dataFlowService.Validate(dataFlow);

            // Assert
            Assert.AreEqual(0, result.ValidationResults.GetAll().Count);
        }


    }
}
