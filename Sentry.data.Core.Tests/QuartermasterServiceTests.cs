using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core.Interfaces.QuartermasterRestClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class QuartermasterServiceTests
    {
        /// <summary>
        /// - Validates that when the QuartermasterService returns no Named Environments, no validation
        /// is done against the NamedEnvironment or NamedEnvironmentType
        /// </summary>
        [TestMethod]
        public async Task QuartermasterService_VerifyNamedEnvironmentAsync_NoNamedEnvironments()
        {
            // Arrange
            var quartermasterClient = new Mock<IClient>();
            var namedEnvironmentList = new List<NamedEnvironment>();
            quartermasterClient.Setup(f => f.NamedEnvironmentsGet2Async(It.IsAny<string>(), It.IsAny<ShowDeleted10>()).Result).Returns(namedEnvironmentList);
            var quartermasterService = new QuartermasterService(quartermasterClient.Object);

            // Act
            var result = await quartermasterService.VerifyNamedEnvironmentAsync("ABCD", "FOO", GlobalEnums.NamedEnvironmentType.NonProd);

            // Assert
            Assert.AreEqual(0, result.GetAll().Count);
        }


        /// <summary>
        /// - Test that when the QuartermasterService *does* return Named Environments, but the given
        /// NamedEnvironment doesn't match what Quartermaster has, a validation error is returned
        /// </summary>
        [TestMethod]
        public async Task QuartermasterService_Validate_InvalidNamedEnvironment()
        {
            // Arrange
            var quartermasterClient = new Mock<IClient>();
            var namedEnvironmentList = new[] { new NamedEnvironment() { Name = "TEST", Environmenttype = "NonProd" } };
            quartermasterClient.Setup(f => f.NamedEnvironmentsGet2Async(It.IsAny<string>(), It.IsAny<ShowDeleted10>()).Result).Returns(namedEnvironmentList);
            var quartermasterService = new QuartermasterService(quartermasterClient.Object);

            // Act
            var result = await quartermasterService.VerifyNamedEnvironmentAsync("ABCD", "FOO", GlobalEnums.NamedEnvironmentType.NonProd);

            // Assert
            Assert.AreEqual(1, result.GetAll().Count);
            Assert.IsTrue(result.Contains(GlobalConstants.ValidationErrors.NAMED_ENVIRONMENT_INVALID));
        }

        /// <summary>
        /// - Test that when the QuartermasterService *does* return Named Environments, but the given
        /// NamedEnvironmentType doesn't match what Quartermaster has, a validation error is returned
        /// </summary>
        [TestMethod]
        public async Task QuartermasterService_Validate_InvalidNamedEnvironmentType()
        {
            // Arrange
            var quartermasterClient = new Mock<IClient>();
            var namedEnvironmentList = new[] { new NamedEnvironment() { Name = "TEST", Environmenttype = "NonProd" } };
            quartermasterClient.Setup(f => f.NamedEnvironmentsGet2Async(It.IsAny<string>(), It.IsAny<ShowDeleted10>()).Result).Returns(namedEnvironmentList);
            var quartermasterService = new QuartermasterService(quartermasterClient.Object);

            // Act
            var result = await quartermasterService.VerifyNamedEnvironmentAsync("ABCD", "TEST", GlobalEnums.NamedEnvironmentType.Prod);

            // Assert
            Assert.AreEqual(1, result.GetAll().Count);
            Assert.IsTrue(result.Contains(GlobalConstants.ValidationErrors.NAMED_ENVIRONMENT_TYPE_INVALID));
        }

    }
}
