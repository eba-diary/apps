using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core.GlobalEnums;
using System.Collections.Generic;

namespace Sentry.data.Core.Tests.ServiceImplementationTests
{
    [TestClass]
    public class ConfigServiceTests
    {
        [TestCategory("ConfigServiceTests")]
        [TestMethod]
        public void Delete_Returns_True_When_Incoming_DatasetFileConfig_Is_Marked_PendingDelete_And_Performing_Logical_Delete()
        {
            //Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            DatasetFileConfig config = MockClasses.MockDatasetFileConfig(null, null);
            config.ObjectStatus = ObjectStatusEnum.Pending_Delete;

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(x => x.GetById<DatasetFileConfig>(config.ConfigId)).Returns(config);
            ConfigService configService = new ConfigService(context.Object, null, null, null, null, null, null, null, null, null, null, null, null);

            //Act
            bool IsSuccessful = configService.Delete(config.ConfigId, null, true);

            //Assert
            Assert.AreEqual(true, IsSuccessful);
        }

        [TestCategory("ConfigServiceTests")]
        [TestMethod]
        public void Delete_Does_Not_Save_DB_Changes_When_Incoming_DatasetFileConfig_Is_Marked_PendingDelete_And_Performing_Logical_Delete()
        {
            //Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            DatasetFileConfig config = MockClasses.MockDatasetFileConfig(null, null);
            config.ObjectStatus = ObjectStatusEnum.Pending_Delete;

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(x => x.GetById<DatasetFileConfig>(config.ConfigId)).Returns(config);
            context.Setup(x => x.SaveChanges(It.IsAny<bool>()));
            ConfigService configService = new ConfigService(context.Object, null, null, null, null, null, null, null, null, null, null, null, null);

            //Act
            configService.Delete(config.ConfigId, null, true);

            //Assert
            context.Verify(x => x.SaveChanges(It.IsAny<bool>()), Times.Never);
        }



        [TestCategory("ConfigServiceTests")]
        [TestMethod]
        public void Delete_Does_Not_Trigger_Child_Deletes_When_Incoming_DatasetFileConfig_Is_Marked_PendingDelete_And_Performing_Logical_Delete()
        {
            //Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            DatasetFileConfig config = MockClasses.MockDatasetFileConfig(null, null);
            config.ObjectStatus = ObjectStatusEnum.Pending_Delete;

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(x => x.GetById<DatasetFileConfig>(config.ConfigId)).Returns(config);
            context.Setup(x => x.SaveChanges(It.IsAny<bool>()));

            Mock<IJobService> jobService = mr.Create<IJobService>();
            jobService.Setup(x => x.Delete(It.IsAny<List<int>>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>()));

            Mock<IDataFlowService> dataFlowService = mr.Create<IDataFlowService>();
            dataFlowService.Setup(x => x.Delete(It.IsAny<int>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>()));

            ConfigService configService = new ConfigService(context.Object, null, null, null, null, null, jobService.Object, null, null, null, dataFlowService.Object, null, null);

            //Act
            configService.Delete(config.ConfigId, null, true);

            //Assert
            jobService.Verify(x => x.Delete(It.IsAny<List<int>>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>()), Times.Never);
            dataFlowService.Verify(x => x.Delete(It.IsAny<int>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>()), Times.Never);
        }

        [TestCategory("ConfigServiceTests")]
        [TestMethod]
        public void Delete_Returns_True_When_Incoming_DatasetFileConfig_Is_Marked_Delete_And_Performing_Logical_Delete()
        {
            //Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            DatasetFileConfig config = MockClasses.MockDatasetFileConfig(null, null);
            config.ObjectStatus = ObjectStatusEnum.Deleted;

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(x => x.GetById<DatasetFileConfig>(config.ConfigId)).Returns(config);

            ConfigService configService = new ConfigService(context.Object, null, null, null, null, null, null, null, null, null, null, null, null);

            //Act
            bool IsSuccessful = configService.Delete(config.ConfigId, null, true);

            //Assert
            Assert.AreEqual(true, IsSuccessful);
        }

        [TestCategory("ConfigServiceTests")]
        [TestMethod]
        public void Delete_Does_Not_Save_DB_Changes_When_Incoming_DatasetFileConfig_Is_Marked_Delete_And_Performing_Logical_Delete()
        {
            //Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            DatasetFileConfig config = MockClasses.MockDatasetFileConfig(null, null);
            config.ObjectStatus = ObjectStatusEnum.Deleted;

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(x => x.GetById<DatasetFileConfig>(config.ConfigId)).Returns(config);
            context.Setup(x => x.SaveChanges(It.IsAny<bool>()));

            ConfigService configService = new ConfigService(context.Object, null, null, null, null, null, null, null, null, null, null, null, null);

            //Act
            configService.Delete(config.ConfigId, null, true);

            //Assert
            context.Verify(x => x.SaveChanges(It.IsAny<bool>()), Times.Never);
        }

        [TestCategory("ConfigServiceTests")]
        [TestMethod]
        public void Delete_Does_Not_Trigger_Child_Deletes_When_Incoming_DatasetFileConfig_Is_Marked_Deleted_And_Performing_Physical_Delete()
        {
            //Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            DatasetFileConfig config = MockClasses.MockDatasetFileConfig(null, null);
            config.ObjectStatus = ObjectStatusEnum.Deleted;

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(x => x.GetById<DatasetFileConfig>(config.ConfigId)).Returns(config);
            context.Setup(x => x.SaveChanges(It.IsAny<bool>()));

            Mock<IJobService> jobService = mr.Create<IJobService>();
            jobService.Setup(x => x.Delete(It.IsAny<List<int>>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>()));

            Mock<IDataFlowService> dataFlowService = mr.Create<IDataFlowService>();
            dataFlowService.Setup(x => x.Delete(It.IsAny<List<int>>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>()));

            ConfigService configService = new ConfigService(context.Object, null, null, null, null, null, jobService.Object, null, null, null, dataFlowService.Object, null, null);

            //Act
            configService.Delete(config.ConfigId, null, false);

            //Assert
            jobService.Verify(x => x.Delete(It.IsAny<List<int>>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>()), Times.Never);
            dataFlowService.Verify(x => x.Delete(It.IsAny<List<int>>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>()), Times.Never);
        }

        [TestCategory("ConfigServiceTests")]
        [TestMethod]
        public void Delete_Returns_True_When_DatasetFileConfig_Is_Marked_Delete_And_Performing_Physical_Delete()
        {
            //Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            DatasetFileConfig config = MockClasses.MockDatasetFileConfig(null, null);
            config.ObjectStatus = ObjectStatusEnum.Deleted;

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(x => x.GetById<DatasetFileConfig>(config.ConfigId)).Returns(config);
            context.Setup(x => x.SaveChanges(It.IsAny<bool>()));

            Mock<IJobService> jobService = mr.Create<IJobService>();
            jobService.Setup(x => x.Delete(It.IsAny<List<int>>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>()));

            Mock<IDataFlowService> dataFlowService = mr.Create<IDataFlowService>();
            dataFlowService.Setup(x => x.Delete(It.IsAny<List<int>>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>()));

            ConfigService configService = new ConfigService(context.Object, null, null, null, null, null, jobService.Object, null, null, null, dataFlowService.Object, null, null);

            //Act
            bool IsSuccessful = configService.Delete(config.ConfigId, null, false);

            //Assert
            Assert.AreEqual(true, IsSuccessful);
            jobService.Verify(x => x.Delete(It.IsAny<List<int>>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>()), Times.Never);
            dataFlowService.Verify(x => x.Delete(It.IsAny<List<int>>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>()), Times.Never);
            context.Verify(x => x.SaveChanges(It.IsAny<bool>()), Times.Never);
        }
    }
}
