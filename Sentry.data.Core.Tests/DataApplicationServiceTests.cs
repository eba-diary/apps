using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Core.Tests
{
    [TestCategory("DataApplicationServiceTests")]
    [TestClass]
    public class DataApplicationServiceTests
    {
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
            var dataApplicationService = new DataApplicationService(context.Object, lazyService, null, null, null, null, null, null, null, null, null);

            // Act
            dataApplicationService.DeleteDataset(idList, user.Object);

            // Assert
            mr.VerifyAll();
        }

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
            var dataApplicationService = new DataApplicationService(context.Object, null, lazyService, null, null, null, null, null, null, null, null);

            // Act
            dataApplicationService.DeleteDatasetFileConfig(idList, user.Object);

            // Assert
            mr.VerifyAll();
        }

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
            var dataApplicationService = new DataApplicationService(context.Object, null, null, lazyService, null, null, null, null, null, null, null);

            // Act
            dataApplicationService.DeleteDataflow(idList, user.Object);

            // Assert
            mr.VerifyAll();
        }

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
            var dataApplicationService = new DataApplicationService(context.Object, lazyDatasetService, null, null, null, null, null, null, null, null, null);

            // Act
            dataApplicationService.DeleteDataset(idList, user.Object);

            // Assert
            context.Verify(x => x.SaveChanges(true), Times.Once);
        }

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
            var dataApplicationService = new DataApplicationService(context.Object, lazyDatasetService, null, null, null, null, null, null, null, null, null);

            // Act
            dataApplicationService.DeleteDataset(idList, user.Object);

            // Assert
            datasetService.Verify(x => x.Delete(It.Is<int>(id => id == 1), It.IsAny<IApplicationUser>(), It.IsAny<bool>()), Times.Once);
            datasetService.Verify(x => x.Delete(It.Is<int>(id => id == 2), It.IsAny<IApplicationUser>(), It.IsAny<bool>()), Times.Once);
            datasetService.Verify(x => x.Delete(It.IsAny<int>(), It.IsAny<IApplicationUser>(), It.IsAny<bool>()), Times.Exactly(2));
        }

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
            var dataApplicationService = new DataApplicationService(context.Object, lazyDatasetService, null, null, null, null, null, null, null, null, null);

            // Act
            dataApplicationService.DeleteDataset(idList, user.Object);

            // Assert
            context.Verify(x => x.SaveChanges(It.IsAny<bool>()), Times.Never);
            context.Verify(x => x.Clear(), Times.Once);
        }

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
            var dataApplicationService = new DataApplicationService(context.Object, lazyDatasetService, null, null, null, null, null, null, null, null, null);


            // Act
            dataApplicationService.DeleteDataset(idList, user.Object);

            // Assert
            context.Verify(x => x.SaveChanges(It.IsAny<bool>()), Times.Never);
            context.Verify(x => x.Clear(), Times.Once);
        }

        [TestMethod]
        public async Task MigrateDataset_Dataset_Only_Call_Order()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            user.Setup(s => s.DisplayName).Returns("Me");

            DatasetMigrationRequest request = new DatasetMigrationRequest()
            {
                SourceDatasetId = 1,
                TargetDatasetNamedEnvironment = "QUAL"
            };
            Dataset sourceDataset = new Dataset() { DatasetId = 1, DatasetName = "MyDataset", Asset = new Asset() { SaidKeyCode = "ABCD" }, NamedEnvironment = "TEST" };
            Dataset dataset = MockClasses.MockDataset(user: user.Object);
            DatasetDto dto = MockClasses.MockDatasetDto(new List<Dataset>() { dataset, sourceDataset }).First();
            var calls = new List<Tuple<string, int>>();
            int callOrder = 0;

            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(s => s.GetCurrentUser()).Returns(user.Object);

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.Datasets).Returns(new List<Dataset>() { dataset, sourceDataset }.AsQueryable());
            context.Setup(s => s.GetById<Dataset>(It.IsAny<int>())).Returns(dataset);
            context.Setup(s => s.SaveChanges(It.IsAny<bool>())).Callback<bool>(s => calls.Add(new Tuple<string, int>($"{nameof(IDatasetContext.SaveChanges)}", ++callOrder)));
            
            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            securityService.Setup(s => s.GetUserSecurity(It.IsAny<ISecurable>(), It.IsAny<IApplicationUser>())).Returns(new UserSecurity() { CanEditDataset = true }).Callback<ISecurable, IApplicationUser>((s,a) => calls.Add(new Tuple<string, int>($"{nameof(SecurityService.GetUserSecurity)}", ++callOrder)));

            Mock<IDatasetService> datasetService = mr.Create<IDatasetService>();
            datasetService.Setup(s => s.GetDatasetDto(It.IsAny<int>())).Returns(dto);
            datasetService.Setup(s => s.DatasetExistsInTargetNamedEnvironment(sourceDataset.DatasetName, sourceDataset.Asset.SaidKeyCode, request.TargetDatasetNamedEnvironment)).Returns((0, false));

            Mock<IQuartermasterService> quartermasterService = mr.Create<IQuartermasterService>();
            quartermasterService.Setup(s => s.VerifyNamedEnvironmentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NamedEnvironmentType>())).Returns(Task.FromResult(new ValidationResults()));

            Mock<IDataFeatures> dataFeatures = mr.Create<IDataFeatures>();
            dataFeatures.Setup(s => s.CLA1797_DatasetSchemaMigration.GetValue()).Returns(true);

            var lazyDatasetService = new Lazy<IDatasetService>(() => datasetService.Object);
            var lazySecurityService = new Lazy<ISecurityService>(() => securityService.Object);
            var lazyUserService = new Lazy<IUserService>(() => userService.Object);
            var lazyQuartermasterService = new Lazy<IQuartermasterService>(() => quartermasterService.Object);
            var lazyDataFeatures = new Lazy<IDataFeatures>(() => dataFeatures.Object);
            Mock<DataApplicationService> dataApplicationService = new Mock<DataApplicationService>(context.Object, lazyDatasetService, null, null, null, lazyUserService, lazyDataFeatures, lazySecurityService, null, null, lazyQuartermasterService);
            dataApplicationService.Setup(s => s.CreateWithoutSave(dto)).Returns(1).Callback<DatasetDto>(s => calls.Add(new Tuple<string, int>($"{nameof(DataApplicationService.CreateWithoutSave)}", ++callOrder)));
            dataApplicationService.Setup(s => s.MigrateSchemaWithoutSave_Internal(It.IsAny<List<SchemaMigrationRequest>>())).Returns(new List<SchemaMigrationRequestResponse>()).Callback<List<SchemaMigrationRequest>>(s => calls.Add(new Tuple<string, int>($"{nameof(DataApplicationService.MigrateSchemaWithoutSave_Internal)}", ++callOrder)));
            dataApplicationService.Setup(s => s.CreateExternalDependenciesForDataset(It.IsAny<List<int>>())).Callback<List<int>>(s => calls.Add(new Tuple<string, int>($"{nameof(DataApplicationService.CreateExternalDependenciesForDataset)}", ++callOrder)));
            dataApplicationService.Setup(s => s.CreateExternalDependenciesForDataFlowBySchemaId(It.IsAny<List<int>>())).Callback<List<int>>(s => calls.Add(new Tuple<string, int>($"{nameof(DataApplicationService.CreateExternalDependenciesForDataFlowBySchemaId)}", ++callOrder)));
            
            //MOCK CreateMigrationHistory as well
            dataApplicationService.Setup(s => s.CreateMigrationHistory(It.IsAny<DatasetMigrationRequest>(), It.IsAny<DatasetMigrationRequestResponse>())).Callback( ()=> calls.Add(new Tuple<string, int>($"{nameof(DataApplicationService.CreateMigrationHistory)}", ++callOrder)));

            //Act
            await dataApplicationService.Object.MigrateDataset(request);

            //Arrage
            mr.VerifyAll();
            Assert.AreEqual(8, calls.Count);
            Assert.AreEqual(1, calls.Where(w => w.Item1 == $"{nameof(SecurityService.GetUserSecurity)}").Select(s => s.Item2).FirstOrDefault(), $"{nameof(SecurityService.GetUserSecurity)} called out of order");
            Assert.AreEqual(2, calls.Where(w => w.Item1 == $"{nameof(DataApplicationService.CreateWithoutSave)}").Select(s => s.Item2).FirstOrDefault(), $"{nameof(DataApplicationService.CreateWithoutSave)} called out of order");
            Assert.AreEqual(3, calls.Where(w => w.Item1 == $"{nameof(DataApplicationService.MigrateSchemaWithoutSave_Internal)}").Select(s => s.Item2).FirstOrDefault(), $"{nameof(DataApplicationService.MigrateSchemaWithoutSave_Internal)} called out of order");
            Assert.AreEqual(4, calls.Where(w => w.Item1 == $"{nameof(IDatasetContext.SaveChanges)}").Select(s => s.Item2).FirstOrDefault(), $"{nameof(DataApplicationService.CreateExternalDependenciesForDataFlowBySchemaId)} called out of order");
            Assert.AreEqual(5, calls.Where(w => w.Item1 == $"{nameof(DataApplicationService.CreateExternalDependenciesForDataset)}").Select(s => s.Item2).FirstOrDefault(), $"{nameof(IDatasetContext.SaveChanges)} called out of order");
            Assert.AreEqual(6, calls.Where(w => w.Item1 == $"{nameof(DataApplicationService.CreateExternalDependenciesForDataFlowBySchemaId)}").Select(s => s.Item2).FirstOrDefault(), $"{nameof(DataApplicationService.CreateExternalDependenciesForDataFlowBySchemaId)} called out of order");
            
            //VERIFY CreateMigrationHistory was called in order
            Assert.AreEqual(7, calls.Where(w => w.Item1 == $"{nameof(DataApplicationService.CreateMigrationHistory)}").Select(s => s.Item2).FirstOrDefault(), $"{nameof(DataApplicationService.CreateWithoutSave)} called out of order");
            Assert.AreEqual(8, calls.Where(w => w.Item1 == $"{nameof(IDatasetContext.SaveChanges)}").Select(s => s.Item2).LastOrDefault(), $"{nameof(DataApplicationService.CreateExternalDependenciesForDataFlowBySchemaId)} called out of order");
        }

        [TestMethod]
        public void Verify_AddMigrationHistory_Inserted()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);
            Dataset sourceDataset = new Dataset() { DatasetId = 1, DatasetName = "MyDataset", Asset = new Asset() { SaidKeyCode = "ABCD" }, NamedEnvironment = "DEV" };
          
            //MOCK _datasetContext Calls
            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.Datasets).Returns(new List<Dataset>() { sourceDataset, sourceDataset }.AsQueryable());
            
            //MAGIC HERE IS VERIFICATION THAT WHAT WAS ADDED TO CONTEXT IN UNIT TEST CALLED MATCHED the ASSERT
            context.Setup(s => s.Add(It.IsAny<MigrationHistory>())).Callback<MigrationHistory>(x => 
            {
                    Assert.AreEqual(MockClasses.MockHistoryMontana().SourceDatasetId, x.SourceDatasetId);
                    Assert.AreEqual(MockClasses.MockHistoryMontana().TargetDatasetId, x.TargetDatasetId);
                    Assert.AreEqual(MockClasses.MockHistoryMontana().TargetNamedEnvironment, x.TargetNamedEnvironment);
            });

            DataApplicationService dataApplicationService = new DataApplicationService(context.Object, null, null, null, null, null, null, null, null, null, null);

            //EXECUTE 
            dataApplicationService.AddMigrationHistory(MockClasses.MockRequestMontana(), MockClasses.MockResponseMontana());

            //VERIFY ANYTHING CALLED WAS MOCKED
            mr.VerifyAll();
        }

        [TestMethod]
        public void Verify_MigrationHistoryDetail_Dataset_Inserted()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            //MOCK _datasetContext Calls
            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();

            //MAGIC HERE IS VERIFICATION THAT WHAT WAS ADDED TO CONTEXT IN UNIT TEST CALLED MATCHED the ASSERT
            context.Setup(s => s.Add(It.IsAny<MigrationHistoryDetail>())).Callback<MigrationHistoryDetail>(x =>
            {
                Assert.AreEqual(MockClasses.MockHistoryDetailDataset().SourceDatasetId, x.SourceDatasetId);
                Assert.AreEqual(MockClasses.MockHistoryDetailDataset().DatasetId, x.DatasetId);
                Assert.AreEqual(MockClasses.MockHistoryDetailDataset().DatasetMigrationMessage, x.DatasetMigrationMessage);
                Assert.AreEqual(MockClasses.MockHistoryDetailDataset().DatasetName, x.DatasetName);
                Assert.AreEqual(MockClasses.MockHistoryDetailDataset().MigrationHistoryId, x.MigrationHistoryId);
                Assert.AreEqual(MockClasses.MockHistoryDetailDataset().DataFlowId, x.DataFlowId);
                Assert.AreEqual(MockClasses.MockHistoryDetailDataset().SchemaId, x.SchemaId);
                Assert.AreEqual(MockClasses.MockHistoryDetailDataset().SchemaRevisionId, x.SchemaRevisionId);
            });

            DataApplicationService dataApplicationService = new DataApplicationService(context.Object, null, null, null, null, null, null, null, null, null, null);

            //EXECUTE 
            dataApplicationService.AddMigrationHistoryDetailDataset(MockClasses.MockHistoryMontana(), MockClasses.MockRequestMontana(), MockClasses.MockResponseMontana());

            //VERIFY ANYTHING CALLED WAS MOCKED
            mr.VerifyAll();
        }



        [TestMethod]
        public void Verify_MigrationHistoryDetail_Schemas_Inserted()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            //MOCK _datasetContext Calls
            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();

            //MAGIC HERE IS VERIFICATION THAT WHAT WAS ADDED TO CONTEXT IN UNIT TEST CALLED MATCHED the ASSERT
            context.Setup(s => s.Add(It.Is<MigrationHistoryDetail>(x => x.SchemaId == MockClasses.MockHistoryDetailSchemaGlacier().SchemaId))).Callback<MigrationHistoryDetail>(x =>
            {
                Assert.AreEqual(MockClasses.MockHistoryDetailSchemaGlacier().SchemaId, x.SchemaId);
                Assert.AreEqual(MockClasses.MockHistoryDetailSchemaGlacier().SchemaName, x.SchemaName);
                Assert.AreEqual(MockClasses.MockHistoryDetailSchemaGlacier().SchemaRevisionName, x.SchemaRevisionName);
            });

            context.Setup(s => s.Add(It.Is<MigrationHistoryDetail>(x => x.SchemaId == MockClasses.MockHistoryDetailSchemaGreatFalls().SchemaId))).Callback<MigrationHistoryDetail>(x =>
            {
                Assert.AreEqual(MockClasses.MockHistoryDetailSchemaGreatFalls().SchemaId, x.SchemaId);
                Assert.AreEqual(MockClasses.MockHistoryDetailSchemaGreatFalls().SchemaName, x.SchemaName);
                Assert.AreEqual(MockClasses.MockHistoryDetailSchemaGreatFalls().SchemaRevisionName, x.SchemaRevisionName);
            });

            //ENSURE MigrationHistoryDetail is null
            context.Setup(s => s.Add(It.Is<MigrationHistoryDetail>(x => x.SchemaId == MockClasses.MockHistoryDetailSchemaNewYork().SchemaId))).Callback<MigrationHistoryDetail>(x =>
            {
                Assert.IsNull(x.SchemaId);
                Assert.IsNull(x.SchemaName);
                Assert.IsNull(x.SchemaRevisionName);
            });

            DataApplicationService dataApplicationService = new DataApplicationService(context.Object, null, null, null, null, null, null, null, null, null, null);

            //EXECUTE 
            dataApplicationService.AddMigrationHistoryDetailSchemas(MockClasses.MockHistoryMontana(), MockClasses.MockRequestMontana(), MockClasses.MockResponseMontana());

            //VERIFY ANYTHING CALLED WAS MOCKED
            mr.VerifyAll();
        }



        [TestMethod]
        public async Task MigrateDataset__NoPermissions_To_Migrate()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            user.Setup(s => s.DisplayName).Returns("Me");

            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(s => s.GetCurrentUser()).Returns(user.Object);

            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            securityService.Setup(s => s.GetUserSecurity(It.IsAny<ISecurable>(), It.IsAny<IApplicationUser>())).Returns(new UserSecurity() { CanEditDataset = false, CanManageSchema = true, CanCreateDataset = true, CanCreateDataFlow = true });

            Mock<IDatasetService> datasetService = mr.Create<IDatasetService>();
            datasetService.Setup(s => s.DatasetExistsInTargetNamedEnvironment(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns((0, false));

            Mock<IQuartermasterService> quartermasterService = mr.Create<IQuartermasterService>();
            quartermasterService.Setup(s => s.VerifyNamedEnvironmentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NamedEnvironmentType>())).Returns(Task.FromResult(new ValidationResults()));

            Mock<IDataFeatures> dataFeatures = mr.Create<IDataFeatures>();
            dataFeatures.Setup(s => s.CLA1797_DatasetSchemaMigration.GetValue()).Returns(true);

            Dataset dataset = MockClasses.MockDataset(user: user.Object);
            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.GetById<Dataset>(It.IsAny<int>())).Returns(dataset);
            context.Setup(s => s.Datasets).Returns(new List<Dataset>() { dataset }.AsQueryable());
            context.Setup(s => s.Clear());

            var lazySecurityService = new Lazy<ISecurityService>(() => securityService.Object);
            var lazyUserService = new Lazy<IUserService>(() => userService.Object);
            var lazyDatasetService = new Lazy<IDatasetService>(() => datasetService.Object);
            var lazyQuartermasterService = new Lazy<IQuartermasterService>(() => quartermasterService.Object);
            var lazyDataFeatures = new Lazy<IDataFeatures>(() => dataFeatures.Object);
            DataApplicationService dataApplicationService = new DataApplicationService(context.Object, lazyDatasetService, null, null, null, lazyUserService, lazyDataFeatures, lazySecurityService, null, null, lazyQuartermasterService);

            DatasetMigrationRequest request = new DatasetMigrationRequest()
            {
                SourceDatasetId = dataset.DatasetId,
                TargetDatasetNamedEnvironment = "QUAL"
            };

            //Act
            await Assert.ThrowsExceptionAsync<DatasetUnauthorizedAccessException>(() => dataApplicationService.MigrateDataset(request));
        }

        [TestMethod]
        public void MigrateScheam__NoPermissions_To_Migrate()
        {
            //Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            FileSchema sourceSchema = new FileSchema() { SchemaId = 99, Name = "MySchema_AA" };
            Asset datasetAsset = new Asset() { AssetId = 1, SaidKeyCode = "ABCD" };
            Dataset sourceDataset = new Dataset() { DatasetId = 1, Asset = datasetAsset };
            DatasetFileConfig sourceDatasetFileConfig = new DatasetFileConfig() { ConfigId = 1, ParentDataset = sourceDataset, Schema = sourceSchema };

            Dataset targetDataset = new Dataset() { DatasetId = 2, Asset = datasetAsset, NamedEnvironment = "TEST", ObjectStatus = ObjectStatusEnum.Active };

            SchemaMigrationRequest request = new SchemaMigrationRequest();
            request.SourceSchemaId = sourceSchema.SchemaId;
            request.TargetDatasetNamedEnvironment = "TEST";
            request.TargetDatasetId = targetDataset.DatasetId;
            
            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.FileSchema).Returns(new List<FileSchema>() { sourceSchema }.AsQueryable());
            context.Setup(s => s.Datasets).Returns(new List<Dataset>() { sourceDataset, targetDataset }.AsQueryable());
            context.Setup(s => s.DatasetFileConfigs).Returns(new List<DatasetFileConfig>() { sourceDatasetFileConfig }.AsQueryable());
            context.Setup(s => s.DataFlow).Returns(new List<DataFlow>().AsQueryable());

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();

            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(s => s.GetCurrentUser()).Returns(user.Object);

            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            securityService.Setup(s => s.GetUserSecurity(It.IsAny<ISecurable>(), It.IsAny<IApplicationUser>())).Returns(new UserSecurity() { CanEditDataset = false, CanManageSchema = false});

            Mock<ISchemaService> schemaService = mr.Create<ISchemaService>();
            schemaService.Setup(s => s.SchemaExistsInTargetDataset(It.IsAny<int>(), It.IsAny<string>())).Returns((0,false));

            Mock<IDataFeatures> dataFeatures = mr.Create<IDataFeatures>();
            dataFeatures.Setup(s => s.CLA1797_DatasetSchemaMigration.GetValue()).Returns(true);

            var lazySecurityService = new Lazy<ISecurityService>(() => securityService.Object);
            var lazyUserService = new Lazy<IUserService>(() => userService.Object);
            var lazySchemaService = new Lazy<ISchemaService>(() => schemaService.Object);
            var lazyDataFeatures = new Lazy<IDataFeatures>(() => dataFeatures.Object);
            DataApplicationService dataApplicationService = new DataApplicationService(context.Object, null, null, null, null, lazyUserService, lazyDataFeatures, lazySecurityService, lazySchemaService, null, null);

            //Assert
            Assert.ThrowsException<SchemaUnauthorizedAccessException>(() => dataApplicationService.MigrateSchema(request));
        }

        [TestMethod]
        public void MigrateSchema__Exists_On_Target__Only_SchemaRevision_Migrated()
        {
            //Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            FileSchema sourceSchema = new FileSchema() { SchemaId = 99, Name = "MySchema_AA" };
            Asset datasetAsset = new Asset() { AssetId = 1, SaidKeyCode = "ABCD" };
            Dataset sourceDataset = new Dataset() { DatasetId = 1, Asset = datasetAsset };
            DatasetFileConfig sourceDatasetFileConfig = new DatasetFileConfig() { ConfigId = 1, ParentDataset = sourceDataset, Schema = sourceSchema };

            FileSchema targetSchema = new FileSchema() { SchemaId = 888, Name = "MySchema_AA" };            
            Dataset targetDataset = new Dataset() { DatasetId = 2, Asset = datasetAsset, NamedEnvironment = "TEST", ObjectStatus = ObjectStatusEnum.Active };
            DatasetFileConfig targetDatasetFileConfig = new DatasetFileConfig() { ConfigId = 1, ParentDataset = targetDataset, Schema = targetSchema };

            SchemaMigrationRequest request = new SchemaMigrationRequest();
            request.SourceSchemaId = sourceSchema.SchemaId;
            request.TargetDatasetNamedEnvironment = "TEST";
            request.TargetDatasetId = targetDataset.DatasetId;

            SchemaRevisionFieldStructureDto schemaRevisionDto = new SchemaRevisionFieldStructureDto() { Revision = new SchemaRevisionDto()};


            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.FileSchema).Returns(new List<FileSchema>() { sourceSchema, targetSchema }.AsQueryable());
            context.Setup(s => s.Datasets).Returns(new List<Dataset>() { sourceDataset, targetDataset }.AsQueryable());
            context.Setup(s => s.DatasetFileConfigs).Returns(new List<DatasetFileConfig>() { sourceDatasetFileConfig, targetDatasetFileConfig }.AsQueryable());
            context.Setup(s => s.DataFlow).Returns(new List<DataFlow>().AsQueryable());
            context.Setup(s => s.SchemaRevision).Returns(new List<SchemaRevision>() { new SchemaRevision() { SchemaRevision_Name = "My_New_Revision", SchemaRevision_Id = 1 } }.AsQueryable());
            context.Setup(s => s.SaveChanges(It.IsAny<bool>()));

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();

            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(s => s.GetCurrentUser()).Returns(user.Object);

            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            securityService.Setup(s => s.GetUserSecurity(It.IsAny<ISecurable>(), It.IsAny<IApplicationUser>())).Returns(new UserSecurity() { CanEditDataset = false, CanManageSchema = false });

            Mock<ISchemaService> schemaService = mr.Create<ISchemaService>();
            schemaService.Setup(s => s.SchemaExistsInTargetDataset(It.IsAny<int>(), It.IsAny<string>())).Returns((targetSchema.SchemaId, true));
            schemaService.Setup(s => s.GetLatestSchemaRevisionFieldStructureBySchemaId(It.IsAny<int>(), It.IsAny<int>())).Returns(schemaRevisionDto);

            Mock<IDataFeatures> dataFeatures = mr.Create<IDataFeatures>();
            dataFeatures.Setup(s => s.CLA1797_DatasetSchemaMigration.GetValue()).Returns(true);

            var lazySecurityService = new Lazy<ISecurityService>(() => securityService.Object);
            var lazyUserService = new Lazy<IUserService>(() => userService.Object);
            var lazySchemaService = new Lazy<ISchemaService>(() => schemaService.Object);
            var lazyDataFeatures = new Lazy<IDataFeatures>(() => dataFeatures.Object);
            Mock<DataApplicationService> dataApplicationService = new Mock<DataApplicationService>(context.Object, null, null, null, null, lazyUserService, lazyDataFeatures, lazySecurityService, lazySchemaService, null, null);
            dataApplicationService.Setup(s => s.CheckPermissionToMigrateSchema(targetSchema.SchemaId));
            dataApplicationService.Setup(s => s.CreateWithoutSave(It.IsAny<SchemaRevisionFieldStructureDto>())).Returns(777);
            dataApplicationService.Setup(s => s.CreateExternalDependenciesForSchemaRevision(It.IsAny<List<(int, int)>>()));

            //Act
            SchemaMigrationRequestResponse response = dataApplicationService.Object.MigrateSchema(request);

            //Assert
            dataApplicationService.Verify(s => s.CreateWithoutSave(It.IsAny<DatasetFileConfigDto>()), Times.Never());
            dataApplicationService.Verify(s => s.CreateWithoutSave(It.IsAny<SchemaRevisionFieldStructureDto>()), Times.Once);
            Assert.IsFalse(response.MigratedSchema);
            Assert.AreEqual("Schema configuration existed in target", response.SchemaMigrationReason);
            Assert.AreEqual(targetSchema.SchemaId, response.TargetSchemaId);

            Assert.IsTrue(response.MigratedSchemaRevision);
            Assert.AreEqual("Success", response.SchemaRevisionMigrationReason);
            Assert.AreEqual(777, response.TargetSchemaRevisionId);

            Assert.IsFalse(response.MigratedDataFlow);
            Assert.AreEqual("Source schema is not associated with dataflow", response.DataFlowMigrationReason);
            Assert.AreEqual(0, response.TargetDataFlowId);

            //Assert.AreEqual()

        }

        [TestMethod]
        public void MigrateSchemaWithoutSaveInternal__Call_Order()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);


            int newSchemaId = 11;
            var calls = new List<Tuple<string, int>>();
            int callOrder = 0;

            FileSchema sourceFileSchema = MockClasses.MockFileSchema();
            FileSchemaDto fileSchemaDto = MockClasses.MockFileSchemaDto(sourceFileSchema);
            DataFlow dataFlow = MockClasses.MockDataFlow();
            dataFlow.SchemaId = sourceFileSchema.SchemaId;

            Dataset targetDataset = MockClasses.MockDataset();
            targetDataset.NamedEnvironment = "QUAL";

            Dataset sourceDataset = MockClasses.MockDataset();
            sourceDataset.DatasetId = 99;
            DatasetFileConfig datasetFileConfig = MockClasses.MockDatasetFileConfig(sourceDataset, sourceFileSchema);
            DatasetFileConfigDto datasetFileConfigDto = MockClasses.MockDatasetFileConfigDtoList(new List<DatasetFileConfig>() { datasetFileConfig }).First();
            SchemaRevisionFieldStructureDto schemaRevisionDto = new SchemaRevisionFieldStructureDto() { Revision = new SchemaRevisionDto()};
            DataFlowDetailDto dataFlowDetailDto2 = new DataFlowDetailDto() { SchemaMap = new List<SchemaMapDto>() { new SchemaMapDto() } };

            SchemaMigrationRequest request = new SchemaMigrationRequest()
            {
                SourceSchemaId = sourceFileSchema.SchemaId,
                TargetDatasetId = 1000,
                TargetDatasetNamedEnvironment = "QUAL",
                TargetDataFlowNamedEnvironment = "QUALV2"
            };

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.DatasetFileConfigs).Returns(new List<DatasetFileConfig>() { datasetFileConfig }.AsQueryable());
            context.Setup(s => s.DataFlow).Returns(new List<DataFlow>() { dataFlow }.AsQueryable());
            context.Setup(s => s.Datasets).Returns(new List<Dataset>() { sourceDataset, targetDataset }.AsQueryable());
            context.Setup(s => s.FileSchema).Returns(new List<FileSchema>() { sourceFileSchema }.AsQueryable());
            context.Setup(s => s.SchemaRevision).Returns(new List<SchemaRevision>() { new SchemaRevision() { SchemaRevision_Id = 1, SchemaRevision_Name = "My New Revision" } }.AsQueryable());

            Mock<ISchemaService> schemaService = mr.Create<ISchemaService>();
            schemaService.Setup(s => s.GetFileSchemaDto(It.IsAny<int>())).Returns(fileSchemaDto);
            schemaService.Setup(s => s.SchemaExistsInTargetDataset(It.IsAny<int>(), It.IsAny<string>())).Returns((0, false));
            schemaService.Setup(s => s.GetLatestSchemaRevisionFieldStructureBySchemaId(It.IsAny<int>(), It.IsAny<int>())).Returns(schemaRevisionDto);

            Mock<IConfigService> configService = mr.Create<IConfigService>();
            configService.Setup(s => s.GetDatasetFileConfigDtoByDataset(It.IsAny<int>())).Returns(new List<DatasetFileConfigDto>() { datasetFileConfigDto });

            Mock<IDataFlowService> dataFlowService = mr.Create<IDataFlowService>();
            dataFlowService.Setup(s => s.GetDataFlowDetailDtoBySchemaId(It.IsAny<int>())).Returns(new List<DataFlowDetailDto>() { dataFlowDetailDto2 });

            var lazySchemaService = new Lazy<ISchemaService>(() => schemaService.Object);
            var lazyConfigService = new Lazy<IConfigService>(() => configService.Object);
            var lazyDataFlowService = new Lazy<IDataFlowService>(() => dataFlowService.Object);
            Mock<DataApplicationService> dataApplicationService = new Mock<DataApplicationService>(context.Object, null, lazyConfigService, lazyDataFlowService, null, null, null, null, lazySchemaService, null, null);
            dataApplicationService.Setup(s => s.CreateWithoutSave(fileSchemaDto)).Returns(newSchemaId).Callback<FileSchemaDto>(s => calls.Add(new Tuple<string, int>($"{nameof(DataApplicationService.CreateWithoutSave)}_FileSchema", ++callOrder)));
            dataApplicationService.Setup(s => s.CreateWithoutSave(datasetFileConfigDto)).Returns(22).Callback<DatasetFileConfigDto>(s => calls.Add(new Tuple<string, int>($"{nameof(DataApplicationService.CreateWithoutSave)}_DatasetFileConfig", ++callOrder)));
            dataApplicationService.Setup(s => s.CreateWithoutSave(dataFlowDetailDto2)).Returns(33).Callback<DataFlowDto>(s => calls.Add(new Tuple<string, int>($"{nameof(DataApplicationService.CreateWithoutSave)}_DataFlow", ++callOrder)));
            dataApplicationService.Setup(s => s.CreateWithoutSave(schemaRevisionDto)).Returns(44).Callback<SchemaRevisionFieldStructureDto>(s => calls.Add(new Tuple<string, int>($"{nameof(DataApplicationService.CreateWithoutSave)}_SchemaRevision", ++callOrder)));

            //Act
            dataApplicationService.Object.MigrateSchemaWithoutSave_Internal(request);

            //Assert
            mr.VerifyAll();
            Assert.AreEqual(1, calls.Where(w => w.Item1 == $"{nameof(DataApplicationService.CreateWithoutSave)}_FileSchema").Select(s => s.Item2).FirstOrDefault(), $"{nameof(DataApplicationService.CreateWithoutSave)}_FileSchema called out of order");
            Assert.AreEqual(2, calls.Where(w => w.Item1 == $"{nameof(DataApplicationService.CreateWithoutSave)}_DatasetFileConfig").Select(s => s.Item2).FirstOrDefault(), $"{nameof(DataApplicationService.CreateWithoutSave)}_DatasetFileConfig called out of order");
            Assert.AreEqual(3, calls.Where(w => w.Item1 == $"{nameof(DataApplicationService.CreateWithoutSave)}_SchemaRevision").Select(s => s.Item2).FirstOrDefault(), $"{nameof(DataApplicationService.CreateWithoutSave)}_SchemaRevision called out of order");
            Assert.AreEqual(4, calls.Where(w => w.Item1 == $"{nameof(DataApplicationService.CreateWithoutSave)}_DataFlow").Select(s => s.Item2).FirstOrDefault(), $"{nameof(DataApplicationService.CreateWithoutSave)}_DataFlow called out of order");
            context.Verify(v => v.SaveChanges(It.IsAny<bool>()), Times.Never);
        }

        [TestMethod]
        public void MigrateSchemaWithoutSaveInternal__Exception()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            SchemaMigrationRequest request = new SchemaMigrationRequest()
            {
                SourceSchemaId = 1,
                TargetDatasetNamedEnvironment = "QUAL",
                TargetDataFlowNamedEnvironment = "QUALV2"
            };

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(x => x.DataFlow).Returns(new List<DataFlow>() { new DataFlow() }.AsQueryable());
            context.Setup(s => s.FileSchema).Throws<InvalidOperationException>();

            DataApplicationService dataApplicationService = new DataApplicationService(context.Object, null, null, null, null, null, null, null, null, null, null);

            //Act
            Assert.ThrowsException<InvalidOperationException>(() => dataApplicationService.MigrateSchemaWithoutSave_Internal(request));
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
            DataApplicationService dataApplicationService = new DataApplicationService(null, lazyDatasetService, null, null, null, null, null, null, null, null, null);

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
            DataApplicationService dataApplicationService = new DataApplicationService(null, lazyDatasetService, null, null, null, null, null, null, null, null, null);


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

            Mock<DataApplicationService> dataApplicationService = new Mock<DataApplicationService>(context.Object, null, null, null, null, null, null, null, null, null, null);
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
            var dataApplicationService = new DataApplicationService(context.Object, lazyDatasetService, null, null, null, null, null, null, null, null, null);

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
            var dataApplicationService = new DataApplicationService(context.Object, lazyDatasetService, null, null, null, null, null, null, null, null, null);

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
            DataApplicationService dataApplicationService = new DataApplicationService(null, null, lazyConfigService, null, null, null, null, null, null, null, null);

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
            DataApplicationService dataApplicationService = new DataApplicationService(null, null, lazyConfigService, null, null, null, null, null, null, null, null);


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

            Mock<DataApplicationService> dataApplicationService = new Mock<DataApplicationService>(context.Object, null, null, null, null, null, null, null, null, null, null);
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

            Mock<DataApplicationService> dataApplicationService = new Mock<DataApplicationService>(context.Object, null, null, null, null, null, null, null, null, null, null);

            //Act
            _ = dataApplicationService.Object.Create(dto);
        }

        [TestMethod]
        public void CreateWithoutSave_For_DataFlow_DfsPull()
        {
            //Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            FileSchema schema = MockClasses.MockFileSchema();
            var schemaMapDto = new SchemaMapDto() { DatasetId = 1, SchemaId = schema.SchemaId, Id = 99 };
            DataFlowDto dto = MockClasses.MockDataFlowDto(MockClasses.MockDataFlow(), schemaMapDto);
            dto.IngestionType = (int)IngestionType.DSC_Pull;
            dto.RetrieverJob = new RetrieverJobDto() { JobId = 1 };
            DataFlow dataflow = MockClasses.MockDataFlow();

            RetrieverJob retrieverJob = MockClasses.GetMockRetrieverJob();

            Mock<IDataFlowService> dataFlowService = mr.Create<IDataFlowService>();
            dataFlowService.Setup(s => s.Create(dto)).Returns(dataflow);

            Mock<IJobService> jobService = mr.Create<IJobService>();
            jobService.Setup(s => s.CreateRetrieverJob(It.IsAny<RetrieverJobDto>())).Returns(retrieverJob);

            var lazyDataFlowService = new Lazy<IDataFlowService>(() => dataFlowService.Object);
            var lazyJobService = new Lazy<IJobService>(() => jobService.Object);
            DataApplicationService dataApplicationService = new DataApplicationService(null, null, null, lazyDataFlowService, null, null, null, null, null, lazyJobService, null);

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

            Mock<DataApplicationService> dataApplicationService = new Mock<DataApplicationService>(context.Object, null, null, null, null, null, null, null, null, null, null);
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

            Mock<DataApplicationService> dataApplicationService = new Mock<DataApplicationService>(context.Object, null, null, null, null, null, null, null, null, null, null);
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
            dataFlow.IngestionType = (int)IngestionType.DFS_Drop;

            Mock<IDatasetContext> context = new Mock<IDatasetContext>();
            context.Setup(s => s.SaveChanges(It.IsAny<bool>()));

            Mock<IDataFlowService> dataFlowService = mr.Create<IDataFlowService>();
            dataFlowService.Setup(s => s.Create(dto)).Returns(dataFlow);
            dataFlowService.Setup(s => s.CreateExternalDependencies(It.IsAny<int>())).Throws<InvalidOperationException>();

            Mock<IJobService> jobService = mr.Create<IJobService>();
            jobService.Setup(s => s.CreateDfsRetrieverJob(It.IsAny<DataFlow>()));

            Mock<IDataFeatures> dataFeatures = mr.Create<IDataFeatures>();
            dataFeatures.Setup(s => s.CLA3241_DisableDfsDropLocation.GetValue()).Returns(false);

            var lazyDataFlowService = new Lazy<IDataFlowService>(() => dataFlowService.Object);
            var lazyJobService = new Lazy<IJobService>(() => jobService.Object);
            var lazyDataFeatures = new Lazy<IDataFeatures>(() => dataFeatures.Object);
            DataApplicationService dataApplicationService = new DataApplicationService(context.Object, null, null, lazyDataFlowService, null, null, lazyDataFeatures, null, null, lazyJobService, null);

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
            DataApplicationService dataApplicationService = new DataApplicationService(null, null, null, null, null, null, null, null, lazySchemaService, null, null);

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
            DataApplicationService dataApplicationService = new DataApplicationService(null, null, null, null, null, null, null, null, lazySchemaService, null, null);

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

            Mock<DataApplicationService> dataApplicationService = new Mock<DataApplicationService>(context.Object, null, null, null, null, null, null, null, null, null, null);
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
            var dataApplicationService = new DataApplicationService(context.Object, null, null, null, null, null, null, null, lazySchemaService, null, null);

            //Act
            _ = dataApplicationService.Create(dto);

            //Assert
            mr.VerifyAll();
        }

        [TestMethod]
        public void AreDatasetsRelated_True()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Asset asset = new Asset() { AssetId = 1, SaidKeyCode = "ABCD" };
            Dataset dataset_A = new Dataset() { DatasetId = 1, DatasetName = "YADS", Asset = asset };
            Dataset dataset_B = new Dataset() { DatasetId = 2, DatasetName = "YADS", Asset = asset };

            List<Dataset> datasetList = new List<Dataset>() { dataset_A, dataset_B };

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.Datasets).Returns(datasetList.AsQueryable());

            var dataApplicationService = new DataApplicationService(context.Object, null, null, null, null, null, null, null, null, null, null);

            //ACT
            bool result = dataApplicationService.AreDatasetsRelated(1, 2);

            //Assert
            Assert.IsTrue(result);
        }



        [TestMethod]
        public void AreDatasetsRelated_False()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Asset asset_A = new Asset() { AssetId = 1, SaidKeyCode = "ABCD" };
            Asset asset_B = new Asset() { AssetId = 1, SaidKeyCode = "YYYY" };

            Dataset dataset_A = new Dataset() { DatasetId = 1, DatasetName = "YADS", Asset = asset_A };
            Dataset dataset_B = new Dataset() { DatasetId = 2, DatasetName = "DifferentName", Asset = asset_A };
            Dataset dataset_C = new Dataset() { DatasetId = 3, DatasetName = "YADS", Asset = asset_B };

            List<Dataset> datasetList = new List<Dataset>() { dataset_A, dataset_B, dataset_C };

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.Datasets).Returns(datasetList.AsQueryable());

            var dataApplicationService = new DataApplicationService(context.Object, null, null, null, null, null, null, null, null, null, null);

            //ACT
            bool result_NameDifferent = dataApplicationService.AreDatasetsRelated(1, 2);
            bool result_AssetDifferent = dataApplicationService.AreDatasetsRelated(1, 3);

            //Assert
            Assert.IsFalse(result_NameDifferent, "Failed validation on different dataset names");
            Assert.IsFalse(result_AssetDifferent, "Failed validation on different assets");
        }

        [TestMethod]
        public void AreDatasetsRelated_DatasetNotFound()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Asset asset = new Asset() { AssetId = 1, SaidKeyCode = "ABCD" };
            Dataset dataset_A = new Dataset() { DatasetId = 1, DatasetName = "YADS", Asset = asset };
            Dataset dataset_B = new Dataset() { DatasetId = 2, DatasetName = "YADS", Asset = asset };

            List<Dataset> datasetList = new List<Dataset>() { dataset_A, dataset_B };

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.Datasets).Returns(datasetList.AsQueryable());

            var dataApplicationService = new DataApplicationService(context.Object, null, null, null, null, null, null, null, null, null, null);

            //Assert
            Assert.IsFalse(dataApplicationService.AreDatasetsRelated(1, 3));
            Assert.IsFalse(dataApplicationService.AreDatasetsRelated(3, 1));
        }

        [TestMethod]
        public void SchemaExistsInTargetDataset()
        {
            //Arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);
            Dataset dataset_A = new Dataset() { DatasetId = 1, DatasetName = "YADS" };
            FileSchema fileSchema_1 = new FileSchema() { SchemaId = 99, Name = "MySchema", ObjectStatus = ObjectStatusEnum.Active };
            DatasetFileConfig datasetFileConfig_1 = MockClasses.MockDatasetFileConfig(dataset_A, fileSchema_1);
            datasetFileConfig_1.ObjectStatus = ObjectStatusEnum.Active;

            FileSchema fileSchema_2 = new FileSchema() { SchemaId = 199, Name = "AnotherSchema", ObjectStatus = ObjectStatusEnum.Deleted };
            DatasetFileConfig datasetFileConfig_2 = MockClasses.MockDatasetFileConfig(dataset_A, fileSchema_2);
            datasetFileConfig_2.ObjectStatus = ObjectStatusEnum.Deleted;

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.DatasetFileConfigs).Returns(new List<DatasetFileConfig>() { datasetFileConfig_1, datasetFileConfig_2 }.AsQueryable());

            SchemaService schemaService = new SchemaService(context.Object, null, null, null, null, null, null, null, null, null, null, null, null);

            //Act
            (int, bool) result_Exists = schemaService.SchemaExistsInTargetDataset(1, "MySchema");
            (int, bool) result_DoesNotExist_DatasetId = schemaService.SchemaExistsInTargetDataset(2, "MySchema");
            (int, bool) result_DoesNotExist_SchemaName = schemaService.SchemaExistsInTargetDataset(2, "WrongSchema");
            (int, bool) result_DeletedSchema = schemaService.SchemaExistsInTargetDataset(2, "AnotherSchema");

            //Assert
            Assert.AreEqual((99, true), result_Exists);
            Assert.AreEqual((0, false), result_DoesNotExist_DatasetId);
            Assert.AreEqual((0, false), result_DoesNotExist_SchemaName);
            Assert.AreEqual((0, false), result_DeletedSchema);
        }

        [TestMethod]
        public async Task IsNamedEnvironmentRelatedToSaidAsset()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.Datasets).Returns(new List<Dataset>() { new Dataset() { DatasetId = 1, NamedEnvironment = "TEST", Asset = new Asset() { SaidKeyCode = "ABCD" } } }.AsQueryable());

            ValidationResults validationResults = new ValidationResults();
            validationResults.Add("failed a validation");

            Mock<IQuartermasterService> quartermasterService = mr.Create<IQuartermasterService>();
            quartermasterService.Setup(s => s.VerifyNamedEnvironmentAsync("ABCD", "QUAL", It.IsAny<NamedEnvironmentType>())).Returns(Task.FromResult(new ValidationResults()));
            quartermasterService.Setup(s => s.VerifyNamedEnvironmentAsync("ABCD", "PROD", It.IsAny<NamedEnvironmentType>())).Returns(Task.FromResult(validationResults));

            var lazyQuartermasterService = new Lazy<IQuartermasterService>(() => quartermasterService.Object);
            DataApplicationService dataApplicationService = new DataApplicationService(context.Object, null, null, null, null, null, null, null, null, null, lazyQuartermasterService);

            //Act
            bool relatedRequest = await dataApplicationService.IsNamedEnvironmentRelatedToSaidAsset(1, "QUAL");
            bool notRelatedRequest = await dataApplicationService.IsNamedEnvironmentRelatedToSaidAsset(1, "PROD");

            //Assert
            Assert.IsTrue(relatedRequest);
            Assert.IsFalse(notRelatedRequest);
        }

        [TestMethod]
        public async Task ValidateMigrationRequest_DatasetMigrationRequest_Request_with_all_defaults()
        {
            //Arrange
            DatasetMigrationRequest request = new DatasetMigrationRequest();

            DataApplicationService dataApplicationService = new DataApplicationService(null, null, null, null, null, null, null, null, null, null, null);

            //Act
            List<string> errors = await dataApplicationService.ValidateMigrationRequest(request);

            //Assert
            Assert.IsNotNull(errors);
            Assert.IsTrue(errors.Contains("SourceDatasetId is required"));
            Assert.IsTrue(errors.Contains("TargetDatasetNamedEnvironment is required"));
        }

        [TestMethod]
        public async Task ValidationMigraqtionRequest_Negative_Values()
        {
            //Arrange
            DatasetMigrationRequest request = new DatasetMigrationRequest() { SourceDatasetId = -1, TargetDatasetId = -1};

            DataApplicationService dataApplicationService = new DataApplicationService(null, null, null, null, null, null, null, null, null, null, null);

            //Act
            List<string> errors = await dataApplicationService.ValidateMigrationRequest(request);

            //Assert
            Assert.IsNotNull(errors);
            Assert.IsTrue(errors.Contains("SourceDatasetId cannot be a negative number"));
            Assert.IsTrue(errors.Contains("TargetDatasetId cannot be a negative number"));
        }

        [TestMethod]
        public void ValidateMigrationRequest_Null_Request()
        {
            DataApplicationService dataApplicationService = new DataApplicationService(null, null, null, null, null, null, null, null, null, null, null);
            Assert.ThrowsExceptionAsync<ArgumentNullException>(() => dataApplicationService.ValidateMigrationRequest(null));
        }

        [TestMethod]
        public async Task ValidationMigraqtionRequest_Invalid_NamedEnvironment()
        {
            //Arrange
            DatasetMigrationRequest request = new DatasetMigrationRequest() { TargetDatasetNamedEnvironment = "string"};

            DataApplicationService dataApplicationService = new DataApplicationService(null, null, null, null, null, null, null, null, null, null, null);

            //Act
            List<string> errors = await dataApplicationService.ValidateMigrationRequest(request);

            //Assert
            Assert.IsTrue(errors.Any());
            Assert.IsTrue(errors.Contains("Named environment must be alphanumeric, all caps, and less than 10 characters"));
        }
    }
}
