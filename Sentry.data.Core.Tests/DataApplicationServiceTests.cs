using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core.DomainServices;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.Migration;
using Sentry.data.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class DataApplicationServiceTests
    {
        [TestCategory("DataApplicationServiceTests")]
        [TestMethod]
        public void DeleteDataset_Initializes_DatasetService()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(x => x.SaveChanges(true));

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            List<int> idList = new List<int>() { 1 };

            Mock<IDatasetService> datasetService = mr.Create<IDatasetService>();
            datasetService.Setup(x => x.Delete(It.IsAny<int>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>())).Returns(true).Verifiable();

            var lazyService = new Lazy<IDatasetService>(() => datasetService.Object);
            var dataApplicationService = new DataApplicationService(context.Object, lazyService, null, null, null, null, null, null, null);

            // Act
            dataApplicationService.DeleteDataset(idList, user.Object);

            // Assert
            mr.VerifyAll();
        }

        [TestCategory("DataApplicationServiceTests")]
        [TestMethod]
        public void DeleteDatasetFileConfig_Initializes_ConfigService()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(x => x.SaveChanges(true));

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            List<int> idList = new List<int>() { 1 };

            Mock<IConfigService> configService = mr.Create<IConfigService>();
            configService.Setup(x => x.Delete(It.IsAny<int>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>())).Returns(true).Verifiable();

            var lazyService = new Lazy<IConfigService>(() => configService.Object);
            var dataApplicationService = new DataApplicationService(context.Object, null, lazyService, null, null, null, null, null, null);

            // Act
            dataApplicationService.DeleteDatasetFileConfig(idList, user.Object);

            // Assert
            mr.VerifyAll();
        }

        [TestCategory("DataApplicationServiceTests")]
        [TestMethod]
        public void DeleteDataFlow_Initializes_DataFlowService()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(x => x.SaveChanges(true));

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            List<int> idList = new List<int>() { 1 };

            Mock<IDataFlowService> dataFlowService = mr.Create<IDataFlowService>();
            dataFlowService.Setup(x => x.Delete(It.IsAny<int>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>())).Returns(true).Verifiable();

            var lazyService = new Lazy<IDataFlowService>(() => dataFlowService.Object);
            var dataApplicationService = new DataApplicationService(context.Object, null, null, lazyService, null, null, null, null, null);

            // Act
            dataApplicationService.DeleteDataflow(idList, user.Object);

            // Assert
            mr.VerifyAll();
        }

        [TestCategory("DataApplicationServiceTests")]
        [TestMethod]
        public void Delete_Calls_SaveChanges_Once()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(x => x.SaveChanges(true));

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            List<int> idList = new List<int>() { 1 };

            Mock<IDatasetService> datasetService = mr.Create<IDatasetService>();
            datasetService.Setup(x => x.Delete(idList[0], user.Object, true)).Returns(true);

            var lazyDatasetService = new Lazy<IDatasetService>(() => datasetService.Object);
            var dataApplicationService = new DataApplicationService(context.Object, lazyDatasetService, null, null, null, null, null, null, null);

            // Act
            dataApplicationService.DeleteDataset(idList, user.Object);

            // Assert
            context.Verify(x => x.SaveChanges(true), Times.Once);
        }

        [TestCategory("DataApplicationServiceTests")]
        [TestMethod]
        public void Delete_Loops_Through_All_Ids_Passed()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Loose);

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(x => x.SaveChanges(true));

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            List<int> idList = new List<int>() { 1, 2 };

            Mock<IDatasetService> datasetService = mr.Create<IDatasetService>();
            datasetService.Setup(x => x.Delete(It.IsAny<int>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>())).Returns(true);

            var lazyDatasetService = new Lazy<IDatasetService>(() => datasetService.Object);
            var dataApplicationService = new DataApplicationService(context.Object, lazyDatasetService, null, null, null, null, null, null, null);

            // Act
            dataApplicationService.DeleteDataset(idList, user.Object);

            // Assert
            datasetService.Verify(x => x.Delete(It.Is<int>(id => id == 1), It.IsAny<IApplicationUser>(), It.IsAny<bool>()), Times.Once);
            datasetService.Verify(x => x.Delete(It.Is<int>(id => id == 2), It.IsAny<IApplicationUser>(), It.IsAny<bool>()), Times.Once);
            datasetService.Verify(x => x.Delete(It.IsAny<int>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>()), Times.Exactly(2));
        }

        [TestCategory("DataApplicationServiceTests")]
        [TestMethod]
        public void Delete_One_Failure_Calls_Context_Clear_And_No_Changes_Saved()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Loose);

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(x => x.SaveChanges(true));

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            List<int> idList = new List<int>() { 1 };

            Mock<IDatasetService> datasetService = mr.Create<IDatasetService>();
            datasetService.Setup(x => x.Delete(It.IsAny<int>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>())).Returns(false);

            var lazyDatasetService = new Lazy<IDatasetService>(() => datasetService.Object);
            var dataApplicationService = new DataApplicationService(context.Object, lazyDatasetService, null, null, null, null, null, null, null);

            // Act
            dataApplicationService.DeleteDataset(idList, user.Object);

            // Assert
            context.Verify(x => x.SaveChanges(It.IsAny<bool>()), Times.Never);
            context.Verify(x => x.Clear(), Times.Once);
        }

        [TestCategory("DataApplicationServiceTests")]
        [TestMethod]
        public void Delete_One_Failure_Returns_False()
        {
            // Arrange
            MockRepository mr = new MockRepository(MockBehavior.Loose);

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(x => x.SaveChanges(true));

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            List<int> idList = new List<int>() { 1 };

            Mock<IDatasetService> datasetService = mr.Create<IDatasetService>();
            datasetService.Setup(x => x.Delete(It.IsAny<int>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>())).Returns(false);

            var lazyDatasetService = new Lazy<IDatasetService>(() => datasetService.Object);
            var dataApplicationService = new DataApplicationService(context.Object, lazyDatasetService, null, null, null, null, null, null, null);


            // Act
            dataApplicationService.DeleteDataset(idList, user.Object);

            // Assert
            context.Verify(x => x.SaveChanges(It.IsAny<bool>()), Times.Never);
            context.Verify(x => x.Clear(), Times.Once);
        }

        [TestMethod]
        public void ReturnValue()
        {
            var dataApplicationService = new Mock<IDataApplicationService>();
            dataApplicationService.Setup(x => x.DeleteDataset(new List<int>() { 1 }, new Mock<IApplicationUser>().Object, false)).Returns(false);
        }

        [TestMethod]
        public void MigrateDataset_Dataset_Only_Call_Order()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            user.Setup(s => s.DisplayName).Returns("Me");

            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(s => s.GetCurrentUser()).Returns(user.Object);

            DatasetMigrationRequest request = new DatasetMigrationRequest()
            {
                SourceDatasetId = 1,
                TargetDatasetNamedEnvironment = "QUAL"
            };
            Dataset dataset = MockClasses.MockDataset(user: user.Object);
            DatasetDto dto = MockClasses.MockDatasetDto(new List<Dataset>() { dataset }).First();
            var calls = new List<Tuple<string, int>>();
            int callOrder = 0;

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.GetById<Dataset>(It.IsAny<int>())).Returns(dataset);
            context.Setup(s => s.SaveChanges(It.IsAny<bool>())).Callback<bool>(s => calls.Add(new Tuple<string, int>($"{nameof(IDatasetContext.SaveChanges)}", ++callOrder)));

            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            securityService.Setup(s => s.GetUserSecurity(It.IsAny<ISecurable>(), It.IsAny<IApplicationUser>())).Returns(new UserSecurity() { CanEditDataset = true});

            Mock<IDatasetService> datasetService = mr.Create<IDatasetService>();
            datasetService.Setup(s => s.GetDatasetDto(It.IsAny<int>())).Returns(dto);

            var lazyDatasetService = new Lazy<IDatasetService>(() => datasetService.Object);
            var lazySecurityService = new Lazy<ISecurityService>(() => securityService.Object);
            var lazyUserService = new Lazy<IUserService>(() => userService.Object);
            Mock<DataApplicationService> dataApplicationService = new Mock<DataApplicationService>(context.Object, lazyDatasetService, null, null, null, lazyUserService, null, lazySecurityService, null);
            dataApplicationService.Setup(s => s.CreateWithoutSave(dto)).Returns(1).Callback<DatasetDto>(s => calls.Add(new Tuple<string, int>($"{nameof(DataApplicationService.CreateWithoutSave)}", ++callOrder)));
            dataApplicationService.Setup(s => s.MigrateSchemaWithoutSave_Internal(It.IsAny<List<SchemaMigrationRequest>>())).Returns(new List<int>()).Callback<List<SchemaMigrationRequest>>(s => calls.Add(new Tuple<string, int>($"{nameof(DataApplicationService.MigrateSchemaWithoutSave_Internal)}", ++callOrder)));
            dataApplicationService.Setup(s => s.CreateExternalDependenciesForDataset(It.IsAny<List<int>>())).Callback<List<int>>(s => calls.Add(new Tuple<string, int>($"{nameof(DataApplicationService.CreateExternalDependenciesForDataset)}", ++callOrder)));
            dataApplicationService.Setup(s => s.CreateExternalDependenciesForDataFlowBySchemaId(It.IsAny<List<int>>())).Callback<List<int>>(s => calls.Add(new Tuple<string, int>($"{nameof(DataApplicationService.CreateExternalDependenciesForDataFlowBySchemaId)}", ++callOrder)));


            //Act
            _ = dataApplicationService.Object.MigrateDataset(request);

            //Arrage
            mr.VerifyAll();
            Assert.AreEqual(5, calls.Count);
            Assert.AreEqual(1, calls.Where(w => w.Item1 == $"{nameof(DataApplicationService.CreateWithoutSave)}").Select(s => s.Item2).FirstOrDefault(), $"{nameof(DataApplicationService.CreateWithoutSave)} called out of order");
            Assert.AreEqual(2, calls.Where(w => w.Item1 == $"{nameof(DataApplicationService.MigrateSchemaWithoutSave_Internal)}").Select(s => s.Item2).FirstOrDefault(), $"{nameof(DataApplicationService.MigrateSchemaWithoutSave_Internal)} called out of order");
            Assert.AreEqual(3, calls.Where(w => w.Item1 == $"{nameof(IDatasetContext.SaveChanges)}").Select(s => s.Item2).FirstOrDefault(), $"{nameof(DataApplicationService.CreateExternalDependenciesForDataFlowBySchemaId)} called out of order");
            Assert.AreEqual(4, calls.Where(w => w.Item1 == $"{nameof(DataApplicationService.CreateExternalDependenciesForDataset)}").Select(s => s.Item2).FirstOrDefault(), $"{nameof(IDatasetContext.SaveChanges)} called out of order");
            Assert.AreEqual(5, calls.Where(w => w.Item1 == $"{nameof(DataApplicationService.CreateExternalDependenciesForDataFlowBySchemaId)}").Select(s => s.Item2).FirstOrDefault(), $"{nameof(DataApplicationService.CreateExternalDependenciesForDataFlowBySchemaId)} called out of order");
        }

        [TestMethod]
        [ExpectedException(typeof(DatasetUnauthorizedAccessException))]
        public void MigrateDataset__NoPermissions_To_Migrate()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            user.Setup(s => s.DisplayName).Returns("Me");

            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(s => s.GetCurrentUser()).Returns(user.Object);

            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            securityService.Setup(s => s.GetUserSecurity(It.IsAny<ISecurable>(), It.IsAny<IApplicationUser>())).Returns(new UserSecurity() { CanEditDataset = false, CanManageSchema = true, CanCreateDataset = true, CanCreateDataFlow = true });

            Dataset dataset = MockClasses.MockDataset(user: user.Object);
            DatasetDto dto = MockClasses.MockDatasetDto(new List<Dataset>() { dataset }).First();
            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.GetById<Dataset>(It.IsAny<int>())).Returns(dataset);
            context.Setup(s => s.Clear());

            var lazySecurityService = new Lazy<ISecurityService>(() => securityService.Object);
            var lazyUserService = new Lazy<IUserService>(() => userService.Object);
            DataApplicationService dataApplicationService = new DataApplicationService(context.Object, null, null, null, null, lazyUserService, null, lazySecurityService, null);

            DatasetMigrationRequest request = new DatasetMigrationRequest()
            {
                SourceDatasetId = dataset.DatasetId,
                TargetDatasetNamedEnvironment = "QUAL"
            };

            //Act
            _ = dataApplicationService.MigrateDataset(request);
        }

        [TestMethod]
        public void MigrateSchemaWithoutSaveInternal__Call_Order()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            var calls = new List<Tuple<string, int>>();
            int callOrder = 0;

            FileSchema fileSchema = MockClasses.MockFileSchema();
            FileSchemaDto fileSchemaDto = MockClasses.MockFileSchemaDto(fileSchema);

            Dataset dataset = MockClasses.MockDataset();
            DatasetFileConfig datasetFileConfig = MockClasses.MockDatasetFileConfig(dataset, fileSchema);
            DatasetFileConfigDto datasetFileConfigDto = MockClasses.MockDatasetFileConfigDtoList(new List<DatasetFileConfig>() { datasetFileConfig }).First();

            DataFlowDetailDto dataFlowDetailDto2 = new DataFlowDetailDto() { SchemaMap = new List<SchemaMapDto>() { new SchemaMapDto() } };

            SchemaMigrationRequest request = new SchemaMigrationRequest()
            {
                SourceSchemaId = fileSchema.SchemaId,
                TargetDatasetNamedEnvironment = "QUAL",
                TargetDataFlowNamedEnvironment = "QUALV2"
            };

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.DatasetFileConfigs).Returns(new List<DatasetFileConfig>() { datasetFileConfig }.AsQueryable());
            //context.Setup(s => s.DataFlow).Returns(new List<DataFlow>() { new DataFlow() { Id = 1, SchemaId = fileSchema.SchemaId } }.AsQueryable());

            Mock<ISchemaService> schemaService = mr.Create<ISchemaService>();
            schemaService.Setup(s => s.GetFileSchemaDto(It.IsAny<int>())).Returns(fileSchemaDto);
            Mock<IConfigService> configService = mr.Create<IConfigService>();
            configService.Setup(s => s.GetDatasetFileConfigDtoByDataset(It.IsAny<int>())).Returns(new List<DatasetFileConfigDto>() { datasetFileConfigDto });
            Mock<IDataFlowService> dataFlowService = mr.Create<IDataFlowService>();
            dataFlowService.Setup(s => s.GetDataFlowDetailDtoBySchemaId(It.IsAny<int>())).Returns(new List<DataFlowDetailDto>() { dataFlowDetailDto2 });

            var lazySchemaService = new Lazy<ISchemaService>(() => schemaService.Object);
            var lazyConfigService = new Lazy<IConfigService>(() => configService.Object);
            var lazyDataFlowService = new Lazy<IDataFlowService>(() => dataFlowService.Object);
            Mock<DataApplicationService> dataApplicationService = new Mock<DataApplicationService>(context.Object, null, lazyConfigService, lazyDataFlowService, null, null, null, null, lazySchemaService);
            dataApplicationService.Setup(s => s.CreateWithoutSave(fileSchemaDto)).Returns(11).Callback<FileSchemaDto>(s => calls.Add(new Tuple<string, int>($"{nameof(DataApplicationService.CreateWithoutSave)}_FileSchema", ++callOrder)));
            dataApplicationService.Setup(s => s.CreateWithoutSave(datasetFileConfigDto)).Returns(22).Callback<DatasetFileConfigDto>(s => calls.Add(new Tuple<string, int>($"{nameof(DataApplicationService.CreateWithoutSave)}_DatasetFileConfig", ++callOrder)));
            dataApplicationService.Setup(s => s.CreateWithoutSave(dataFlowDetailDto2)).Returns(33).Callback<DataFlowDto>(s => calls.Add(new Tuple<string, int>($"{nameof(DataApplicationService.CreateWithoutSave)}_DataFlow", ++callOrder)));

            //Act
            _ = dataApplicationService.Object.MigrateSchemaWithoutSave_Internal(request);

            //Assert
            mr.VerifyAll();
            Assert.AreEqual(1, calls.Where(w => w.Item1 == $"{nameof(DataApplicationService.CreateWithoutSave)}_FileSchema").Select(s => s.Item2).FirstOrDefault(), $"{nameof(DataApplicationService.CreateWithoutSave)} called out of order");
            Assert.AreEqual(2, calls.Where(w => w.Item1 == $"{nameof(DataApplicationService.CreateWithoutSave)}_DatasetFileConfig").Select(s => s.Item2).FirstOrDefault(), $"{nameof(DataApplicationService.CreateWithoutSave)} called out of order");
            Assert.AreEqual(3, calls.Where(w => w.Item1 == $"{nameof(DataApplicationService.CreateWithoutSave)}_DataFlow").Select(s => s.Item2).FirstOrDefault(), $"{nameof(DataApplicationService.CreateWithoutSave)} called out of order");
            context.Verify(v => v.SaveChanges(It.IsAny<bool>()), Times.Never);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void MigrateSchemaWithoutSaveInternal__Exception()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            SchemaMigrationRequest request = new SchemaMigrationRequest()
            {
                SourceSchemaId = 1,
                TargetDatasetNamedEnvironment = "QUAL",
                TargetDataFlowNamedEnvironment = "QUALV2"
            };

            Mock<ISchemaService> schemaService = mr.Create<ISchemaService>();
            schemaService.Setup(s => s.GetFileSchemaDto(It.IsAny<int>())).Throws<InvalidOperationException>();

            var lazySchemaService = new Lazy<ISchemaService>(() => schemaService.Object);
            DataApplicationService dataApplicationService = new DataApplicationService(null, null, null, null, null, null, null, null, lazySchemaService);

            //Act
            dataApplicationService.MigrateSchemaWithoutSave_Internal(request);
        }

        [TestMethod]
        public void CreateWithoutSave_For_Dataset()
        {
            //Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            DatasetDto datasetDto = MockClasses.MockDatasetDto(new List<Dataset>() { MockClasses.MockDataset() }).First();

            Mock<IDatasetService> datasetService = mr.Create<IDatasetService>();
            datasetService.Setup(s => s.Create(datasetDto)).Returns(1);

            var lazyDatasetService = new Lazy<IDatasetService>(() => datasetService.Object);
            DataApplicationService dataApplicationService = new DataApplicationService(null, lazyDatasetService, null, null, null, null, null, null, null);

            //Act
            int result = dataApplicationService.CreateWithoutSave(datasetDto);

            //Assert
            mr.VerifyAll();
            Assert.AreEqual(1, result);

        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateWithoutSave_For_Dataset__DataasetService_Exception()
        {
            //Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            DatasetDto datasetDto = MockClasses.MockDatasetDto(new List<Dataset>() { MockClasses.MockDataset() }).First();

            Mock<IDatasetService> datasetService = mr.Create<IDatasetService>();
            datasetService.Setup(s => s.Create(datasetDto)).Throws<InvalidOperationException>();

            var lazyDatasetService = new Lazy<IDatasetService>(() => datasetService.Object);
            DataApplicationService dataApplicationService = new DataApplicationService(null, lazyDatasetService, null, null, null, null, null, null, null);


            //Act
            _ = dataApplicationService.CreateWithoutSave(datasetDto);
        }

        [TestMethod]
        public void Create_For_Dataset_Call_Order()
        {
            //Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            DatasetDto dto = MockClasses.MockDatasetDto(new List<Dataset>() { MockClasses.MockDataset() }).First();
            var calls = new List<Tuple<string, int>>();
            int callOrder = 0;

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.SaveChanges(It.IsAny<bool>())).Callback<bool>(s => calls.Add(new Tuple<string, int>($"{nameof(IDatasetContext.SaveChanges)}", ++callOrder)));

            Mock<DataApplicationService> dataApplicationService = new Mock<DataApplicationService>(context.Object, null, null, null, null, null, null, null, null);
            dataApplicationService.Setup(s => s.CreateWithoutSave(It.IsAny<DatasetDto>())).Returns(1).Callback<DatasetDto>(s => calls.Add(new Tuple<string, int>($"{nameof(DataApplicationService.CreateWithoutSave)}", ++callOrder)));
            dataApplicationService.Setup(s => s.CreateExternalDependenciesForDataset(It.IsAny<List<int>>())).Callback<List<int>>(s => calls.Add(new Tuple<string, int>($"{nameof(DataApplicationService.CreateExternalDependenciesForDataset)}", ++callOrder)));

            //Act
            _ = dataApplicationService.Object.Create(dto);
            
            Assert.AreEqual(3, calls.Count);
            Assert.AreEqual(1, calls.Where(w => w.Item1 == $"{nameof(DataApplicationService.CreateWithoutSave)}").Select(s => s.Item2).FirstOrDefault(), $"{nameof(DataApplicationService.CreateWithoutSave)} call out of order");
            Assert.AreEqual(2, calls.Where(w => w.Item1 == $"{nameof(IDatasetContext.SaveChanges)}").Select(s => s.Item2).FirstOrDefault(), $"{nameof(IDatasetContext.SaveChanges)} call out of order");
            Assert.AreEqual(3, calls.Where(w => w.Item1 == $"{nameof(DataApplicationService.CreateExternalDependenciesForDataset)}").Select(s => s.Item2).FirstOrDefault(), $"{nameof(DataApplicationService.CreateExternalDependenciesForDataset)} call out of order");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Create_For_Dataset__Exception_During_Creation()
        {
            //Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            DatasetDto dto = MockClasses.MockDatasetDto(new List<Dataset>() { MockClasses.MockDataset() }).First();

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.SaveChanges(It.IsAny<bool>())).Throws(new InvalidOperationException());
            context.Setup(s => s.Clear());

            Mock<IDatasetService> datasetService = mr.Create<IDatasetService>();
            datasetService.Setup(s => s.Create(dto)).Returns(1);

            var lazyDatasetService = new Lazy<IDatasetService>(() => datasetService.Object );
            var dataApplicationService = new DataApplicationService(context.Object, lazyDatasetService, null, null, null, null, null, null, null);

            //Act
            _ = dataApplicationService.Create(dto);
        }

        [TestMethod]
        public void Create_For_Dataset__Exception_After_Creation()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            DatasetDto dto = MockClasses.MockDatasetDto(new List<Dataset>() { MockClasses.MockDataset() }).First();

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.SaveChanges(It.IsAny<bool>()));

            Mock<IDatasetService> datasetService = mr.Create<IDatasetService>();
            datasetService.Setup(s => s.Create(dto)).Returns(1);
            datasetService.Setup(s => s.CreateExternalDependencies(It.IsAny<int>())).Throws(new Exception());

            var lazyDatasetService = new Lazy<IDatasetService>(() => datasetService.Object);
            var dataApplicationService = new DataApplicationService(context.Object, lazyDatasetService, null, null, null, null, null, null, null);

            //Act
            int result = dataApplicationService.Create(dto);

            //Assert
            context.Verify(s => s.Clear(), Times.Never);
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void CreateWithoutSave_For_DatasetFileConfig()
        {
            //Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            var mockSchema = MockClasses.MockFileSchema();
            var DatasetFileConfig = MockClasses.MockDatasetFileConfig(schema: mockSchema);
            DatasetFileConfigDto datasetFileConfigDto = MockClasses.MockDatasetFileConfigDtoList(new List<DatasetFileConfig>() { DatasetFileConfig }).First();

            Mock<IConfigService> configService = mr.Create<IConfigService>();
            configService.Setup(s => s.Create(datasetFileConfigDto)).Returns(1);

            var lazyConfigService = new Lazy<IConfigService>(() => configService.Object);
            DataApplicationService dataApplicationService = new DataApplicationService(null, null, lazyConfigService, null, null, null, null, null, null);

            //Act
            int result = dataApplicationService.CreateWithoutSave(datasetFileConfigDto);

            //Assert
            mr.VerifyAll();
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateWithoutSave_For_DatasetFileConfig__ConfigService_Exception()
        {
            //Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            var mockSchema = MockClasses.MockFileSchema();
            var DatasetFileConfig = MockClasses.MockDatasetFileConfig(schema: mockSchema);
            DatasetFileConfigDto datasetFileConfigDto = MockClasses.MockDatasetFileConfigDtoList(new List<DatasetFileConfig>() { DatasetFileConfig }).First();

            Mock<IConfigService> configService = mr.Create<IConfigService>();
            configService.Setup(s => s.Create(datasetFileConfigDto)).Throws<InvalidOperationException>();

            var lazyConfigService = new Lazy<IConfigService>(() => configService.Object);
            DataApplicationService dataApplicationService = new DataApplicationService(null, null, lazyConfigService, null, null, null, null, null, null);


            //Act
            _ = dataApplicationService.CreateWithoutSave(datasetFileConfigDto);

            //Assert
            mr.VerifyAll();
        }

        [TestMethod]
        public void Create_For_DatasetFileConfig_Call_Order()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            var mockSchema = MockClasses.MockFileSchema();
            var DatasetFileConfig = MockClasses.MockDatasetFileConfig(schema: mockSchema);
            DatasetFileConfigDto dto = MockClasses.MockDatasetFileConfigDtoList(new List<DatasetFileConfig>() { DatasetFileConfig }).First();
            var calls = new List<Tuple<string, int>>();
            int callOrder = 0;

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.SaveChanges(It.IsAny<bool>())).Callback<bool>(s => calls.Add(new Tuple<string, int>($"{nameof(IDatasetContext.SaveChanges)}", ++callOrder)));

            Mock<DataApplicationService> dataApplicationService = new Mock<DataApplicationService>(context.Object, null, null, null, null, null, null, null, null);
            dataApplicationService.Setup(s => s.CreateWithoutSave(It.IsAny<DatasetFileConfigDto>())).Returns(1).Callback<DatasetFileConfigDto>(s => calls.Add(new Tuple<string, int>($"{nameof(DataApplicationService.CreateWithoutSave)}", ++callOrder)));

            //Act
            _ = dataApplicationService.Object.Create(dto);

            Assert.AreEqual(2, calls.Count);
            Assert.AreEqual(1, calls.Where(w => w.Item1 == $"{nameof(DataApplicationService.CreateWithoutSave)}").Select(s => s.Item2).FirstOrDefault(), $"{nameof(DataApplicationService.CreateWithoutSave)} call out of order");
            Assert.AreEqual(2, calls.Where(w => w.Item1 == $"{nameof(IDatasetContext.SaveChanges)}").Select(s => s.Item2).FirstOrDefault(), $"{nameof(IDatasetContext.SaveChanges)} call out of order");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Create_For_DatasetFileConfig__Exception_During_Creation()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            var mockSchema = MockClasses.MockFileSchema();
            var DatasetFileConfig = MockClasses.MockDatasetFileConfig(schema: mockSchema);
            DatasetFileConfigDto dto = MockClasses.MockDatasetFileConfigDtoList(new List<DatasetFileConfig>() { DatasetFileConfig }).First();


            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.SaveChanges(It.IsAny<bool>())).Throws<InvalidOperationException>();
            context.Setup(s => s.Clear());

            Mock<DataApplicationService> dataApplicationService = new Mock<DataApplicationService>(context.Object, null, null, null, null, null, null, null, null);

            //Act
            _ = dataApplicationService.Object.Create(dto);
        }

        [TestMethod]
        public void CreateWithoutSave_For_DataFlow()
        {
            //Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            FileSchema schema = MockClasses.MockFileSchema();
            var schemaMapDto = new SchemaMapDto() { DatasetId = 1, SchemaId = schema.SchemaId, Id = 99 };
            DataFlowDto dto = MockClasses.MockDataFlowDto(MockClasses.MockDataFlow(), schemaMapDto);
            DataFlow dataflow = MockClasses.MockDataFlow();

            Mock<IDataFlowService> dataFlowService = mr.Create<IDataFlowService>();
            dataFlowService.Setup(s => s.Create(dto)).Returns(dataflow);
            dataFlowService.Setup(s => s.CreateDataFlowRetrieverJobMetadata(dataflow));

            var lazyDataFlowService = new Lazy<IDataFlowService>(() => dataFlowService.Object);
            DataApplicationService dataApplicationService = new DataApplicationService(null, null, null, lazyDataFlowService, null, null, null, null, null);

            //Act
            int result = dataApplicationService.CreateWithoutSave(dto);

            //Assert
            mr.VerifyAll();
            Assert.AreEqual(dataflow.Id, result);

        }

        [TestMethod]
        public void Create_For_Dataflow_Call_Order()
        {
            //Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            FileSchema schema = MockClasses.MockFileSchema();
            var schemaMapDto = new SchemaMapDto() { DatasetId = 1, SchemaId = schema.SchemaId, Id = 99 };
            DataFlowDto dto = MockClasses.MockDataFlowDto(MockClasses.MockDataFlow(), schemaMapDto);

            var calls = new List<Tuple<string, int>>();
            int callOrder = 0;

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.SaveChanges(It.IsAny<bool>())).Callback<bool>(s => calls.Add(new Tuple<string, int>($"{nameof(IDatasetContext.SaveChanges)}", ++callOrder)));

            Mock<DataApplicationService> dataApplicationService = new Mock<DataApplicationService>(context.Object, null, null, null, null, null, null, null, null);
            dataApplicationService.Setup(s => s.CreateWithoutSave(It.IsAny<DataFlowDto>())).Returns(1).Callback<DataFlowDto>(s => calls.Add(new Tuple<string, int>($"{nameof(DataApplicationService.CreateWithoutSave)}", ++callOrder)));
            dataApplicationService.Setup(s => s.CreateExternalDependenciesForDataFlow(It.IsAny<List<int>>())).Callback<List<int>>(s => calls.Add(new Tuple<string, int>($"{nameof(DataApplicationService.CreateExternalDependenciesForDataFlow)}", ++callOrder)));

            //Act
            _ = dataApplicationService.Object.Create(dto);

            Assert.AreEqual(3, calls.Count);
            Assert.AreEqual(1, calls.Where(w => w.Item1 == $"{nameof(DataApplicationService.CreateWithoutSave)}").Select(s => s.Item2).FirstOrDefault(), $"{nameof(DataApplicationService.CreateWithoutSave)} call out of order");
            Assert.AreEqual(2, calls.Where(w => w.Item1 == $"{nameof(IDatasetContext.SaveChanges)}").Select(s => s.Item2).FirstOrDefault(), $"{nameof(IDatasetContext.SaveChanges)} call out of order");
            Assert.AreEqual(3, calls.Where(w => w.Item1 == $"{nameof(DataApplicationService.CreateExternalDependenciesForDataFlow)}").Select(s => s.Item2).FirstOrDefault(), $"{nameof(DataApplicationService.CreateExternalDependenciesForDataFlow)} call out of order");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Create_For_Dataflow__Exception_During_Creation()
        {
            //Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            FileSchema schema = MockClasses.MockFileSchema();
            var schemaMapDto = new SchemaMapDto() { DatasetId = 1, SchemaId = schema.SchemaId, Id = 99 };
            DataFlowDto dto = MockClasses.MockDataFlowDto(MockClasses.MockDataFlow(), schemaMapDto);

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.SaveChanges(It.IsAny<bool>())).Throws<InvalidOperationException>();
            context.Setup(s => s.Clear());

            Mock<DataApplicationService> dataApplicationService = new Mock<DataApplicationService>(context.Object, null, null, null, null, null, null, null, null);
            dataApplicationService.Setup(s => s.CreateWithoutSave(It.IsAny<DataFlowDto>())).Returns(1);

            //Act
            _ = dataApplicationService.Object.Create(dto);
        }

        [TestMethod]
        public void Create_For_DataFlow__Exception_After_Creation()
        {
            //Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            FileSchema schema = MockClasses.MockFileSchema();
            var schemaMapDto = new SchemaMapDto() { DatasetId = 1, SchemaId = schema.SchemaId, Id = 99 };
            DataFlowDto dto = MockClasses.MockDataFlowDto(MockClasses.MockDataFlow(), schemaMapDto);
            DataFlow dataFlow = MockClasses.MockDataFlow();

            Mock<IDatasetContext> context = new Mock<IDatasetContext>();
            context.Setup(s => s.SaveChanges(It.IsAny<bool>()));

            Mock<IDataFlowService> dataFlowService = mr.Create<IDataFlowService>();
            dataFlowService.Setup(s => s.Create(dto)).Returns(dataFlow);
            dataFlowService.Setup(s => s.CreateDataFlowRetrieverJobMetadata(It.IsAny<DataFlow>()));
            dataFlowService.Setup(s => s.CreateExternalDependencies(It.IsAny<int>())).Throws<InvalidOperationException>();

            var lazyDataFlowService = new Lazy<IDataFlowService>(() => dataFlowService.Object);
            DataApplicationService dataApplicationService = new DataApplicationService(context.Object, null, null, lazyDataFlowService, null, null, null, null, null);

            //Act
            _ = dataApplicationService.Create(dto);

            //Assert
            mr.VerifyAll();
            context.Verify(v => v.Clear(), Times.Never);
        }

        [TestMethod]
        public void CreateWithoutSave_For_FileSchema()
        {
            //Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            FileSchema schema = MockClasses.MockFileSchema();
            FileSchemaDto dto = MockClasses.MockFileSchemaDto(schema);

            Mock<ISchemaService> schemaService = mr.Create<ISchemaService>();
            schemaService.Setup(s => s.Create(dto)).Returns(1);

            var lazySchemaService = new Lazy<ISchemaService>(() => schemaService.Object);
            DataApplicationService dataApplicationService = new DataApplicationService(null, null, null, null, null, null, null, null, lazySchemaService);

            //Act
            int result = dataApplicationService.CreateWithoutSave(dto);

            //Assert
            mr.VerifyAll();
            Assert.AreEqual(1, result);

        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateWithoutSave_For_FileSchema__SchemaService_Exception()
        {
            //Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            FileSchema schema = MockClasses.MockFileSchema();
            FileSchemaDto dto = MockClasses.MockFileSchemaDto(schema);

            Mock<ISchemaService> schemaService = mr.Create<ISchemaService>();
            schemaService.Setup(s => s.Create(dto)).Throws<InvalidOperationException>();

            var lazySchemaService = new Lazy<ISchemaService>(() => schemaService.Object);
            DataApplicationService dataApplicationService = new DataApplicationService(null, null, null, null, null, null, null, null, lazySchemaService);

            //Act
            _ = dataApplicationService.CreateWithoutSave(dto);

            //Assert
            mr.VerifyAll();

        }

        [TestMethod]
        public void Create_For_FileSchema__Call_Order()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);
            
            Schema schema = MockClasses.MockFileSchema();
            FileSchemaDto dto = MockClasses.MockFileSchemaDto(schema);
            var calls = new List<Tuple<string, int>>();
            int callOrder = 0;

            Mock<IDatasetContext> context = new Mock<IDatasetContext>();
            context.Setup(s => s.SaveChanges(It.IsAny<bool>())).Callback<bool>(s => calls.Add(new Tuple<string, int>($"{nameof(IDatasetContext.SaveChanges)}", ++callOrder)));

            Mock<DataApplicationService> dataApplicationService = new Mock<DataApplicationService>(context.Object, null, null, null, null, null, null, null, null);
            dataApplicationService.Setup(s => s.CreateWithoutSave(dto)).Returns(1).Callback<FileSchemaDto>(s => calls.Add(new Tuple<string, int>($"{nameof(DataApplicationService.CreateWithoutSave)}", ++callOrder)));

            //Act
            _ = dataApplicationService.Object.Create(dto);

            mr.VerifyAll();
            Assert.AreEqual(2, calls.Count);
            Assert.AreEqual(1, calls.Where(w => w.Item1 == $"{nameof(DataApplicationService.CreateWithoutSave)}").Select(s => s.Item2).FirstOrDefault(), $"{nameof(DataApplicationService.CreateWithoutSave)} call out of order");
            Assert.AreEqual(2, calls.Where(w => w.Item1 == $"{nameof(IDatasetContext.SaveChanges)}").Select(s => s.Item2).FirstOrDefault(), $"{nameof(IDatasetContext.SaveChanges)} call out of order");

        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Create_For_FileSchema__Exception_During_Creation()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Schema schema = MockClasses.MockFileSchema();
            FileSchemaDto dto = MockClasses.MockFileSchemaDto(schema);

            Mock<ISchemaService> schemaService = mr.Create<ISchemaService>();
            schemaService.Setup(s => s.Create(dto)).Returns(1);

            Mock<IDatasetContext> context = new Mock<IDatasetContext>();
            context.Setup(s => s.SaveChanges(It.IsAny<bool>())).Throws(new InvalidOperationException());
            //context.Setup(s => s.Clear());

            var lazySchemaService = new Lazy<ISchemaService>(() => schemaService.Object);
            var dataApplicationService = new DataApplicationService(context.Object, null, null, null, null, null, null, null, lazySchemaService);

            //Act
            _ = dataApplicationService.Create(dto);

            //Assert
            mr.VerifyAll();
        }
    }
}
