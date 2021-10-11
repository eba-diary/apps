using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
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
        /// </summary>
        [TestMethod]
        public async Task DataFlowService_Validate_DuplicateName_NoNamedEnvironments()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            var dataFlows = new[] { new DataFlow() { Name = "Foo" } };
            context.Setup(f => f.DataFlow).Returns(dataFlows.AsQueryable());

            var quartermasterService = new Mock<IQuartermasterService>();
            var validationResults = new ValidationResults();
            quartermasterService.Setup(f => f.VerifyNamedEnvironmentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NamedEnvironmentType>()).Result).Returns(validationResults);

            var dataFlowService = new DataFlowService(context.Object, null, null, null, null, quartermasterService.Object, null);
            var dataFlow = new DataFlowDto() { Name = "Foo" };

            // Act
            var result = await dataFlowService.Validate(dataFlow);

            // Assert
            Assert.AreEqual(1, result.ValidationResults.GetAll().Count);
            Assert.IsTrue(result.ValidationResults.Contains(DataFlow.ValidationErrors.nameMustBeUnique));
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

            var quartermasterService = new Mock<IQuartermasterService>();
            var validationResults = new ValidationResults();
            quartermasterService.Setup(f => f.VerifyNamedEnvironmentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NamedEnvironmentType>()).Result).Returns(validationResults);

            var dataFlowService = new DataFlowService(context.Object, null, null, null, null, quartermasterService.Object, null);
            var dataFlow = new DataFlowDto() { Name = "Bar", NamedEnvironment = "TEST", NamedEnvironmentType = GlobalEnums.NamedEnvironmentType.NonProd };

            // Act
            var result = await dataFlowService.Validate(dataFlow);

            // Assert
            Assert.AreEqual(0, result.ValidationResults.GetAll().Count);
        }


    }
}
