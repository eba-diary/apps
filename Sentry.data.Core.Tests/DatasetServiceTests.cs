using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core.Interfaces;
using System;
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
        
        [TestCategory("Core DatasetService")]
        [TestMethod]
        public async Task Validate_DuplicateName_NoNamedEnvironments()
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

            var datasetService = new DatasetService(context.Object, null, null, null, null, null, quartermasterService.Object, null, null);
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

        [TestCategory("Core DatasetService")]
        [TestMethod]
        public void Delete_Does_Not_Call_Save_Changes()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);
            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            user.Setup(s => s.DisplayName).Returns("displayName");

            Dataset ds = MockClasses.MockDataset(user.Object, true, false);

            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(s => s.GetCurrentUser()).Returns(user.Object);

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.GetById<Dataset>(ds.DatasetId)).Returns(ds);

            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            securityService.Setup(s => s.GetUserSecurity(ds, user.Object));

            Mock<IConfigService> configService = mr.Create<IConfigService>();
            configService.Setup(s => s.Delete(ds.DatasetFileConfigs[0].ConfigId, user.Object, true)).Returns(true);

            var datasetService = new DatasetService(context.Object, securityService.Object, userService.Object, configService.Object, 
                                                    null, null, null, null, null);

            //Act
            datasetService.Delete(ds.DatasetId, user.Object, true);

            //Assert
            context.Verify(x => x.SaveChanges(true), Times.Never);
        }

        [TestCategory("Core DatasetService")]
        [TestMethod]
        public void Delete_Returns_True_When_Incoming_Dataset_Marked_PendingDelete_And_LogicalDelete_Is_True()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);
            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            user.Setup(x => x.DisplayName).Returns("user1");
            Dataset ds = MockClasses.MockDataset(user.Object, false, false);
            ds.ObjectStatus = ObjectStatusEnum.Pending_Delete;

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.GetById<Dataset>(ds.DatasetId)).Returns(ds);

            var datasetService = new DatasetService(context.Object, null, null, null, null, null, null, null, null);

            // Act
            bool IsSuccessful = datasetService.Delete(ds.DatasetId, user.Object, true);

            // Assert
            Assert.AreEqual(true, IsSuccessful);
        }

        [TestCategory("Core DatasetService")]
        [TestMethod]
        public void Delete_Does_Not_Call_SaveChanges_When_Incoming_Dataset_Marked_PendingDelete_And_LogicalDelete_Is_True()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            user.Setup(x => x.DisplayName).Returns("user1");

            Dataset ds = MockClasses.MockDataset(user.Object, false, false);
            ds.ObjectStatus = ObjectStatusEnum.Pending_Delete;

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.GetById<Dataset>(ds.DatasetId)).Returns(ds);
            context.Setup(x => x.SaveChanges(It.IsAny<bool>()));

            var datasetService = new DatasetService(context.Object, null, null, null, null, null, null, null, null);

            // Act
            datasetService.Delete(ds.DatasetId, user.Object, true);

            // Assert
            context.Verify(x => x.SaveChanges(It.IsAny<bool>()), Times.Never);
        }

        [TestCategory("Core DatasetService")]
        [TestMethod]
        public void Delete_Returns_True_When_Incoming_Dataset_Marked_Deleted_And_LogicalDelete_Is_False()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);
            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            user.Setup(x => x.DisplayName).Returns("user1");
            Dataset ds = MockClasses.MockDataset(user.Object, false, false);
            ds.ObjectStatus = ObjectStatusEnum.Deleted;

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.GetById<Dataset>(ds.DatasetId)).Returns(ds);

            var datasetService = new DatasetService(context.Object, null, null, null, null, null, null, null, null);

            // Act
            bool IsSuccessful = datasetService.Delete(ds.DatasetId, user.Object, false);

            // Assert
            Assert.AreEqual(true, IsSuccessful);
        }

        [TestCategory("Core DatasetService")]
        [TestMethod]
        public void Delete_Returns_True_When_Incoming_Dataset_Marked_Deleted_And_LogicalDelete_Is_True()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);
            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            user.Setup(x => x.DisplayName).Returns("user1");
            Dataset ds = MockClasses.MockDataset(user.Object, false, false);
            ds.ObjectStatus = ObjectStatusEnum.Deleted;

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.GetById<Dataset>(ds.DatasetId)).Returns(ds);

            var datasetService = new DatasetService(context.Object, null, null, null, null, null, null, null, null);

            // Act
            bool IsSuccessful = datasetService.Delete(ds.DatasetId, user.Object, true);

            // Assert
            Assert.AreEqual(true, IsSuccessful);
        }

        [TestCategory("Core DatasetService")]
        [TestMethod]
        public void Delete_Does_Not_Call_SaveChanges_When_Incoming_Dataset_Marked_Deleted_And_LogicalDelete_Is_False()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            user.Setup(x => x.DisplayName).Returns("user1");

            Dataset ds = MockClasses.MockDataset(user.Object, false, false);
            ds.ObjectStatus = ObjectStatusEnum.Deleted;

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.GetById<Dataset>(ds.DatasetId)).Returns(ds);
            context.Setup(x => x.SaveChanges(It.IsAny<bool>()));

            var datasetService = new DatasetService(context.Object, null, null, null, null, null, null, null, null);

            // Act
            datasetService.Delete(ds.DatasetId, user.Object, false);

            // Assert
            context.Verify(x => x.SaveChanges(It.IsAny<bool>()), Times.Never);
        }

        [TestCategory("Core DatasetService")]
        [TestMethod]
        public void Delete_Passes_Incoming_User_Info_To_ConfigService_Delete_When_LogicalDelete_Is_False()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();

            Dataset ds = MockClasses.MockDataset(null, true, false);
            ds.ObjectStatus = ObjectStatusEnum.Pending_Delete;

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.GetById<Dataset>(ds.DatasetId)).Returns(ds);

            Mock<IConfigService> configService = mr.Create<IConfigService>();
            configService.Setup(s => s.Delete(It.IsAny<int>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>())).Returns(true);

            var datasetService = new DatasetService(context.Object, null, null, configService.Object, null, null, null, null, null);

            // Act
            datasetService.Delete(ds.DatasetId, user.Object, false);

            // Assert
            configService.Verify(v => v.Delete(It.IsAny<int>(), user.Object, false), Times.Once);
        }


        [TestCategory("Core DatasetService")]
        [TestMethod]
        public void DatasetService_GetAsset_ExistingAsset()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            var expected = new Asset() { AssetId = 1, SaidKeyCode = "ABCD" };
            var assets = new[] { expected };
            context.Setup(c => c.Assets).Returns(assets.AsQueryable());
            var service = new DatasetService(context.Object, null, null, null, null, null, null, null, null);

            // Act
            var actual = service.GetAsset("ABCD");

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestCategory("Core DatasetService")]
        [TestMethod]
        public void DatasetService_GetAsset_NewAsset()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            var existing = new Asset() { AssetId = 1, SaidKeyCode = "ABCD" };
            var assets = new[] { existing };
            context.Setup(c => c.Assets).Returns(assets.AsQueryable());
            
            var user = new Mock<IApplicationUser>();
            user.Setup(u => u.AssociateId).Returns("000000");
            var userService = new Mock<IUserService>();
            userService.Setup(u => u.GetCurrentUser()).Returns(user.Object);

            var service = new DatasetService(context.Object, null, userService.Object, null, null, null, null, null, null);

            // Act
            var actual = service.GetAsset("EFGH");

            // Assert
            Assert.AreNotEqual(existing, actual);
        }
        [TestCategory("Core DatasetService")]
        [TestMethod]
        public void Delete_Passes_Null_User_Info_To_ConfigService_Delete_When_LogicalDelete_Is_False()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Dataset ds = MockClasses.MockDataset(null, true, false);
            ds.ObjectStatus = ObjectStatusEnum.Pending_Delete;

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.GetById<Dataset>(ds.DatasetId)).Returns(ds);

            Mock<IConfigService> configService = mr.Create<IConfigService>();
            configService.Setup(s => s.Delete(It.IsAny<int>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>())).Returns(true);

            var datasetService = new DatasetService(context.Object, null, null, configService.Object, null, null, null, null, null);

            // Act
            datasetService.Delete(ds.DatasetId, null, false);

            // Assert
            configService.Verify(v => v.Delete(It.IsAny<int>(), null, false), Times.Once);
        }


        [TestCategory("Core DatasetService")]
        [TestMethod]
        public void GetDatasetPermissions_Test()
        {
            // Arrange
            var ds = MockClasses.MockDataset(null, true, false);
            var context = new Mock<IDatasetContext>();
            context.Setup(c => c.Datasets).Returns((new[] { ds }).AsQueryable());
            var datasetService = new DatasetService(context.Object, new Mock<ISecurityService>().Object, null, null, null, null, null, new Mock<ISAIDService>().Object, null);

            // Act
            var actual = datasetService.GetDatasetPermissions(ds.DatasetId);

            // Assert
            Assert.AreEqual(ds.DatasetId, actual.DatasetId);
        }

        [TestMethod]
        public void SetDatasetFavorite_1_000000_Add()
        {
            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);

            Dataset ds = new Dataset()
            {
                DatasetId = 1,
                Favorities = new List<Favorite>()
            };

            datasetContext.Setup(x => x.GetById<Dataset>(1)).Returns(ds);
            datasetContext.Setup(x => x.Merge(It.IsAny<Favorite>())).Returns<Favorite>(null).Callback<Favorite>(x =>
            {
                Assert.AreEqual(1, x.DatasetId);
                Assert.AreEqual("000000", x.UserId);
            });
            datasetContext.Setup(x => x.SaveChanges(true));

            DatasetService datasetService = new DatasetService(datasetContext.Object, null, null, null, null, null, null, null, null);

            string result = datasetService.SetDatasetFavorite(1, "000000");

            datasetContext.VerifyAll();
            
            Assert.AreEqual("Successfully added favorite.", result);
        }

        [TestMethod]
        public void SetDatasetFavorite_1_000000_Remove()
        {
            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);

            Dataset ds = new Dataset()
            {
                DatasetId = 1,
                Favorities = new List<Favorite>()
                {
                    new Favorite()
                    {
                        DatasetId = 1,
                        UserId = "000000"
                    }
                }
            };

            datasetContext.Setup(x => x.GetById<Dataset>(1)).Returns(ds);
            datasetContext.Setup(x => x.Remove(It.IsAny<Favorite>())).Callback<Favorite>(x =>
            {
                Assert.AreEqual(1, x.DatasetId);
                Assert.AreEqual("000000", x.UserId);
            });
            datasetContext.Setup(x => x.SaveChanges(true));

            DatasetService datasetService = new DatasetService(datasetContext.Object, null, null, null, null, null, null, null, null);

            string result = datasetService.SetDatasetFavorite(1, "000000");

            datasetContext.VerifyAll();
            
            Assert.AreEqual("Successfully removed favorite.", result);
        }

        [TestMethod]
        public void SetDatasetFavorite_1_000000_ThrowException()
        {
            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);

            datasetContext.Setup(x => x.GetById<Dataset>(1)).Throws(new Exception("foo"));
            datasetContext.Setup(x => x.Clear());

            DatasetService datasetService = new DatasetService(datasetContext.Object, null, null, null, null, null, null, null, null);

            Assert.ThrowsException<Exception>(() => datasetService.SetDatasetFavorite(1, "000000"), "foo");

            datasetContext.VerifyAll();
        }
    }
}
