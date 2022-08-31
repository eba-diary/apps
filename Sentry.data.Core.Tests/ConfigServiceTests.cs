using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
using System.Collections.Generic;
using System.Linq;

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

            var configService = new ConfigService(context.Object,null,null,null,null,null,null,null,null,null,null,null,null);

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

            var configService = new ConfigService(context.Object, null, null, null, null, null, null, null, null, null, null, null, null);

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

            var configService = new ConfigService(context.Object, null, null, null, null, null, null, null, null, null, null, null, null);

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

            var configService = new ConfigService(context.Object, null, null, null, null, null, null, null, null, null, null, null, null);

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

            var configService = new ConfigService(context.Object, null, null, null, null, null, null, null, null, null, null, null, null);

            // Act
            bool IsSuccessful = configService.Delete(dfc.ConfigId, user.Object, false);

            // Assert
            Assert.AreEqual(true, IsSuccessful);
            context.Verify(x => x.SaveChanges(It.IsAny<bool>()), Times.Never);
        }

        [TestCategory("Core ConfigService")]
        [TestMethod]
        public void Delete_Passes_Incoming_User_Info_To_DataFlowService_Delete_When_LogicalDelete_Is_False()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);
            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            FileSchema schema = MockClasses.MockFileSchema();
            schema.ObjectStatus = ObjectStatusEnum.Pending_Delete;

            DatasetFileConfig dfc = MockClasses.MockDataFileConfig(null, schema);
            dfc.ObjectStatus = ObjectStatusEnum.Pending_Delete;

            var dataflows = new[] { MockClasses.MockDataFlow() };
            var schemaMaps = new[] { new SchemaMap() {
                Id = 1,
                DataFlowStepId = new DataFlowStep() 
                    { 
                        Id = 1, 
                        Action = new SchemaLoadAction() { Name = "Schema Load Action" },
                        DataFlow = MockClasses.MockDataFlow()
                    },
                Dataset = dfc.ParentDataset,
                MappedSchema = schema
                } 
            };

            context.Setup(s => s.GetById<DatasetFileConfig>(dfc.ConfigId)).Returns(dfc);
            context.Setup(s => s.DataFlow).Returns(dataflows.AsQueryable());
            context.Setup(s => s.SchemaMap).Returns(schemaMaps.AsQueryable());

            Mock<IDataFlowService> dataFlowService = mr.Create<IDataFlowService>();
            dataFlowService.Setup(s => s.Delete(It.IsAny<List<int>>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>())).Returns(true);
            dataFlowService.Setup(s => s.GetDataFlowNameForFileSchema(It.IsAny<FileSchema>())).Returns("DataflowName");

            var configService = new ConfigService(context.Object, null, null, null, null, null, null, null, null, null, dataFlowService.Object, null, null);

            // Act
            configService.Delete(dfc.ConfigId, user.Object, false);

            // Assert
            dataFlowService.Verify(x => x.Delete(It.IsAny<List<int>>(), user.Object, false), Times.Once);
        }

        [TestCategory("Core ConfigService")]
        [TestMethod]
        public void Delete_Passes_Null_User_Info_To_DataFlowService_Delete_When_LogicalDelete_Is_False()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            FileSchema schema = MockClasses.MockFileSchema();
            schema.ObjectStatus = ObjectStatusEnum.Pending_Delete;

            DatasetFileConfig dfc = MockClasses.MockDataFileConfig(null, schema);
            dfc.ObjectStatus = ObjectStatusEnum.Pending_Delete;

            var dataflows = new[] { MockClasses.MockDataFlow() };
            var schemaMaps = new[] { new SchemaMap() {
                Id = 1,
                DataFlowStepId = new DataFlowStep()
                    {
                        Id = 1,
                        Action = new SchemaLoadAction() { Name = "Schema Load Action" },
                        DataFlow = MockClasses.MockDataFlow()
                    },
                Dataset = dfc.ParentDataset,
                MappedSchema = schema
                }
            };

            context.Setup(s => s.GetById<DatasetFileConfig>(dfc.ConfigId)).Returns(dfc);
            context.Setup(s => s.DataFlow).Returns(dataflows.AsQueryable());
            context.Setup(s => s.SchemaMap).Returns(schemaMaps.AsQueryable());

            Mock<IDataFlowService> dataFlowService = mr.Create<IDataFlowService>();
            dataFlowService.Setup(s => s.Delete(It.IsAny<List<int>>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>())).Returns(true);
            dataFlowService.Setup(s => s.GetDataFlowNameForFileSchema(It.IsAny<FileSchema>())).Returns("DataflowName");

            var configService = new ConfigService(context.Object, null, null, null, null, null, null, null, null, null, dataFlowService.Object, null, null);

            // Act
            configService.Delete(dfc.ConfigId, null, false);

            // Assert
            dataFlowService.Verify(x => x.Delete(It.IsAny<List<int>>(), null, false), Times.Once);
        }
    }
}
