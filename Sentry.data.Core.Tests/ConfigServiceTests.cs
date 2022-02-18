using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core.GlobalEnums;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class ConfigServiceTests
    {
        [TestCategory("Core ConfigService")]
        [TestMethod]
        public void Delete_Does_Not_Call_Save_Changes()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            user.Setup(s => s.DisplayName).Returns("displayName");

            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(s => s.GetCurrentUser()).Returns(user.Object);

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            FileSchema schema = MockClasses.MockFileSchema();
            DatasetFileConfig dfc = MockClasses.MockDataFileConfig(null, schema);
            context.Setup(s => s.GetById<DatasetFileConfig>(dfc.ConfigId)).Returns(dfc);

            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            securityService.Setup(s => s.GetUserSecurity(dfc.ParentDataset, user.Object));

            var configService = new ConfigService(context.Object,null,null,null,null,null,null,null,null,null,null,null);

            //Act
            configService.Delete(dfc.ConfigId, user.Object, true);

            //Assert
            context.Verify(x => x.SaveChanges(true), Times.Never);
        }

        [TestCategory("Core ConfigService")]
        [TestMethod]
        public void Delete_Returns_True_When_Incoming_Object_Marked_PendingDelete_And_LogicalDelete_Is_True()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);
            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            FileSchema schema = MockClasses.MockFileSchema();
            schema.ObjectStatus = ObjectStatusEnum.Pending_Delete;

            DatasetFileConfig dfc = MockClasses.MockDataFileConfig(null, schema);
            dfc.ObjectStatus = ObjectStatusEnum.Pending_Delete;

            context.Setup(s => s.GetById<DatasetFileConfig>(dfc.ConfigId)).Returns(dfc);

            var configService = new ConfigService(context.Object, null, null, null, null, null, null, null, null, null, null, null);

            // Act
            bool IsSuccessful = configService.Delete(dfc.ConfigId, user.Object, true);

            // Assert
            Assert.AreEqual(true, IsSuccessful);
        }

        [TestCategory("Core ConfigService")]
        [TestMethod]
        public void Delete_Does_Not_Call_SaveChanges_When_Incoming_Object_Marked_PendingDelete_And_LogicalDelete_Is_True()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            FileSchema schema = MockClasses.MockFileSchema();
            schema.ObjectStatus = ObjectStatusEnum.Pending_Delete;

            DatasetFileConfig dfc = MockClasses.MockDataFileConfig(null, schema);
            dfc.ObjectStatus = ObjectStatusEnum.Pending_Delete;

            context.Setup(s => s.GetById<DatasetFileConfig>(dfc.ConfigId)).Returns(dfc);
            context.Setup(x => x.SaveChanges(It.IsAny<bool>()));

            var configService = new ConfigService(context.Object, null, null, null, null, null, null, null, null, null, null, null);

            // Act
            configService.Delete(dfc.ConfigId, user.Object, true);

            // Assert
            context.Verify(x => x.SaveChanges(It.IsAny<bool>()), Times.Never);
        }

        [TestCategory("Core ConfigService")]
        [TestMethod]
        public void Delete_Returns_True_When_Incoming_Object_Marked_Deleted_And_LogicalDelete_Is_False()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);
            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            FileSchema schema = MockClasses.MockFileSchema();
            schema.ObjectStatus = ObjectStatusEnum.Deleted;

            DatasetFileConfig dfc = MockClasses.MockDataFileConfig(null, schema);
            dfc.ObjectStatus = ObjectStatusEnum.Deleted;

            context.Setup(s => s.GetById<DatasetFileConfig>(dfc.ConfigId)).Returns(dfc);

            var configService = new ConfigService(context.Object, null, null, null, null, null, null, null, null, null, null, null);

            // Act
            bool IsSuccessful = configService.Delete(dfc.ConfigId, user.Object, false);

            // Assert
            Assert.AreEqual(true, IsSuccessful);
        }

        [TestCategory("Core ConfigService")]
        [TestMethod]
        public void Delete_Does_Not_Call_SaveChanges_When_Incoming_Object_Marked_Deleted_And_LogicalDelete_Is_False()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);
            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            FileSchema schema = MockClasses.MockFileSchema();
            schema.ObjectStatus = ObjectStatusEnum.Deleted;

            DatasetFileConfig dfc = MockClasses.MockDataFileConfig(null, schema);
            dfc.ObjectStatus = ObjectStatusEnum.Deleted;

            context.Setup(s => s.GetById<DatasetFileConfig>(dfc.ConfigId)).Returns(dfc);
            context.Setup(s => s.SaveChanges(It.IsAny<bool>()));

            var configService = new ConfigService(context.Object, null, null, null, null, null, null, null, null, null, null, null);

            // Act
            bool IsSuccessful = configService.Delete(dfc.ConfigId, user.Object, false);

            // Assert
            Assert.AreEqual(true, IsSuccessful);
            context.Verify(x => x.SaveChanges(It.IsAny<bool>()), Times.Never);
        }
    }
}
