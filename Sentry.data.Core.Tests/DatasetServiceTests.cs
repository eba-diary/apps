using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class DatasetServiceTests
    {
        /// <summary>
        /// - Test that the DatasetService.Validate() method correctly identifies a duplicate Dataset name
        /// and responds with the correct validation result.
        /// </summary>
        [TestMethod]
        public async Task DatasetService_Validate_DuplicateName_NoNamedEnvironments()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            var dataFlows = new[] { new Dataset() {
                DatasetName = "Foo",
                DatasetType = GlobalConstants.DataEntityCodes.DATASET,
                DatasetCategories = new List<Category> { new Category() { Id=1 } }
            } };
            context.Setup(f => f.Datasets).Returns(dataFlows.AsQueryable());

            var quartermasterService = new Mock<IQuartermasterService>();
            var validationResults = new ValidationResults();
            quartermasterService.Setup(f => f.VerifyNamedEnvironmentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NamedEnvironmentType>()).Result).Returns(validationResults);

            var datasetService = new DatasetService(context.Object, null, null, null, null, null, quartermasterService.Object, null);
            var dataset = new DatasetDto()
            {
                DatasetName = "Foo",
                DatasetType = GlobalConstants.DataEntityCodes.DATASET,
                DatasetCategoryIds = new List<int> { 1 }
            };

            // Act
            var result = await datasetService.Validate(dataset);

            // Assert
            Assert.IsTrue(result.ValidationResults.GetAll().Count > 0);
            Assert.IsTrue(result.ValidationResults.Contains(Dataset.ValidationErrors.datasetNameDuplicate));
        }

        [TestMethod]
        public void DatasetService_GetDatasetAsset_ExistingAsset()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            var expected = new DatasetAsset() { DatasetAssetId = 1, SaidKeyCode = "ABCD" };
            var datasetAssets = new[] { expected };
            context.Setup(c => c.DatasetAssets).Returns(datasetAssets.AsQueryable());
            var service = new DatasetService(context.Object, null, null, null, null, null, null, null);

            // Act
            var actual = service.GetDatasetAsset("ABCD");

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DatasetService_GetDatasetAsset_NewAsset()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            var existing = new DatasetAsset() { DatasetAssetId = 1, SaidKeyCode = "ABCD" };
            var datasetAssets = new[] { existing };
            context.Setup(c => c.DatasetAssets).Returns(datasetAssets.AsQueryable());
            
            var user = new Mock<IApplicationUser>();
            user.Setup(u => u.AssociateId).Returns("000000");
            var userService = new Mock<IUserService>();
            userService.Setup(u => u.GetCurrentUser()).Returns(user.Object);

            var service = new DatasetService(context.Object, null, userService.Object, null, null, null, null, null);

            // Act
            var actual = service.GetDatasetAsset("EFGH");

            // Assert
            Assert.AreNotEqual(existing, actual);
        }
    }
}
