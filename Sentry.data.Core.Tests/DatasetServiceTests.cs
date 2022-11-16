using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
            var datasets = new[] { new Dataset() {
                DatasetName = "Foo",
                DatasetType = GlobalConstants.DataEntityCodes.DATASET,
                DatasetCategories = new List<Category> { new Category() { Id=1 } },
                NamedEnvironment = "PROD"
            } };
            context.Setup(f => f.Datasets).Returns(datasets.AsQueryable());

            var quartermasterService = new Mock<IQuartermasterService>();
            var validationResults = new ValidationResults();
            quartermasterService.Setup(f => f.VerifyNamedEnvironmentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NamedEnvironmentType>()).Result).Returns(validationResults);

            var datasetService = new DatasetService(context.Object, null, null, null, null, quartermasterService.Object, null, null,null);
            var dataset = new DatasetSchemaDto()
            {
                DatasetName = "Foo",
                DatasetType = GlobalConstants.DataEntityCodes.DATASET,
                DatasetCategoryIds = new List<int> { 1 },
                NamedEnvironment = "PROD"
            };

            // Act
            var result = await datasetService.ValidateAsync(dataset);

            // Assert
            Assert.IsTrue(result.ValidationResults.GetAll().Count > 0);
            Assert.IsTrue(result.ValidationResults.Contains(Dataset.ValidationErrors.datasetNameDuplicate));
            Assert.IsTrue(result.ValidationResults.Contains(Dataset.ValidationErrors.datasetShortNameRequired));
        }


        /// <summary>
        /// Test that the DatasetService.Validate() method will not raise a "Duplicate Dataset" validation error
        /// when datasets are named the same, as long as they're for different Named Environments
        /// </summary>
        [TestCategory("Core DatasetService")]
        [TestMethod]
        public async Task Validate_DuplicateName_ButInDifferentEnvironments()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            var datasets = new[] { new Dataset() {
                DatasetName = "Foo",
                ShortName = "Foo",
                DatasetType = GlobalConstants.DataEntityCodes.DATASET,
                DatasetCategories = new List<Category> { new Category() { Id=1 } },
                NamedEnvironment = "QUAL"
            } };
            context.Setup(f => f.Datasets).Returns(datasets.AsQueryable());

            var quartermasterService = new Mock<IQuartermasterService>();
            var validationResults = new ValidationResults();
            quartermasterService.Setup(f => f.VerifyNamedEnvironmentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NamedEnvironmentType>()).Result).Returns(validationResults);

            var datasetService = new DatasetService(context.Object, null, null, null, null, quartermasterService.Object, null, null, null);
            var dataset = new DatasetSchemaDto()
            {
                DatasetName = "Foo",
                ShortName = "Foo",
                DatasetType = GlobalConstants.DataEntityCodes.DATASET,
                DatasetCategoryIds = new List<int> { 1 },
                NamedEnvironment = "PROD"
            };

            // Act
            var result = await datasetService.ValidateAsync(dataset);

            // Assert
            Assert.IsFalse(result.ValidationResults.Contains(Dataset.ValidationErrors.datasetNameDuplicate));
        }

        [TestCategory("Core DatasetService")]
        [TestMethod]
        public async Task Validate_AlternateContactEmailIsNotSentry()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            var datasets = new[] { new Dataset() {
                DatasetName = "Foo",
                DatasetType = GlobalConstants.DataEntityCodes.DATASET,
                AlternateContactEmail = "jeb@gmail.com"
            } };
            context.Setup(f => f.Datasets).Returns(datasets.AsQueryable());

            var quartermasterService = new Mock<IQuartermasterService>();
            var validationResults = new ValidationResults();
            quartermasterService.Setup(f => f.VerifyNamedEnvironmentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NamedEnvironmentType>()).Result).Returns(validationResults);


            var datasetService = new DatasetService(context.Object, null, null, null, null, quartermasterService.Object, null, null, null);
            var dataset = new DatasetSchemaDto()
            {
                DatasetName = "Foo",
                DatasetType = GlobalConstants.DataEntityCodes.DATASET,
                DatasetCategoryIds = new List<int> { 1 },
                AlternateContactEmail = "jeb@gmail.com"

            };

            // Act
            var result = await datasetService.ValidateAsync(dataset);

            // Assert
            Assert.IsTrue(result.ValidationResults.GetAll().Count > 0);
            Assert.IsTrue(result.ValidationResults.Contains(Dataset.ValidationErrors.datasetAlternateContactEmailFormatInvalid));
        }


        [TestCategory("Core DatasetService")]
        [TestMethod]
        public async Task Validate_AlternateContactEmailIsInvalid()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            var datasets = new[] { new Dataset() {
                DatasetName = "Foo",
                DatasetType = GlobalConstants.DataEntityCodes.DATASET,
                AlternateContactEmail = "jeb@@@@@gmail.com"
            } };
            context.Setup(f => f.Datasets).Returns(datasets.AsQueryable());

            var quartermasterService = new Mock<IQuartermasterService>();
            var validationResults = new ValidationResults();
            quartermasterService.Setup(f => f.VerifyNamedEnvironmentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NamedEnvironmentType>()).Result).Returns(validationResults);


            var datasetService = new DatasetService(context.Object, null, null, null, null, quartermasterService.Object, null, null, null);
            var dataset = new DatasetSchemaDto()
            {
                DatasetName = "Foo",
                DatasetType = GlobalConstants.DataEntityCodes.DATASET,
                DatasetCategoryIds = new List<int> { 1 },
                AlternateContactEmail = "jeb@@@@@gmail.com"

            };

            // Act
            var result = await datasetService.ValidateAsync(dataset);

            // Assert
            Assert.IsTrue(result.ValidationResults.GetAll().Count > 0);
            Assert.IsTrue(result.ValidationResults.Contains(Dataset.ValidationErrors.datasetAlternateContactEmailFormatInvalid));
        }



        [TestCategory("Core DatasetService")]
        [TestMethod]
        public async Task Validate_AlternateContactEmailIsValid()
        {
            // Arrange
            var context = new Mock<IDatasetContext>();
            var datasets = new[] { new Dataset() {
                DatasetName = "Foo",
                DatasetType = GlobalConstants.DataEntityCodes.DATASET,
                AlternateContactEmail = "jeb@sentry.com"
            } };
            context.Setup(f => f.Datasets).Returns(datasets.AsQueryable());

            var quartermasterService = new Mock<IQuartermasterService>();
            var validationResults = new ValidationResults();
            quartermasterService.Setup(f => f.VerifyNamedEnvironmentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NamedEnvironmentType>()).Result).Returns(validationResults);


            var datasetService = new DatasetService(context.Object, null, null, null, null, quartermasterService.Object, null, null, null);
            var dataset = new DatasetSchemaDto()
            {
                DatasetName = "Foo",
                DatasetType = GlobalConstants.DataEntityCodes.DATASET,
                DatasetCategoryIds = new List<int> { 1 },
                AlternateContactEmail = "jeb@sentry.com"
            };

            // Act
            var result = await datasetService.ValidateAsync(dataset);

            // Assert
            Assert.IsFalse(result.ValidationResults.Contains(Dataset.ValidationErrors.datasetAlternateContactEmailFormatInvalid));
        }



        [TestCategory("Core DatasetService")]
        [TestMethod]
        public async Task Validate_ShortName_Regex()
        {
            //Arrange
            var context = new Mock<IDatasetContext>();
            var quartermasterService = new Mock<IQuartermasterService>();
            var validationResults = new ValidationResults();
            quartermasterService.Setup(f => f.VerifyNamedEnvironmentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NamedEnvironmentType>()).Result).Returns(validationResults);
            var datasetService = new DatasetService(context.Object, null, null, null, null, quartermasterService.Object, null, null, null);
            var dataset = new DatasetSchemaDto()
            {
                DatasetName = "Foo",
                DatasetType = GlobalConstants.DataEntityCodes.DATASET,
                DatasetCategoryIds = new List<int> { 1 },
                ShortName = "Spec!@lCh@rsL*ngN@me$"
            };

            // Act
            var result = await datasetService.ValidateAsync(dataset);

            // Assert
            Assert.IsTrue(result.ValidationResults.GetAll().Count > 1); //should have at least two errors - short name invalid regex, and from short name being > 12 chars
            Assert.IsTrue(result.ValidationResults.Contains(Dataset.ValidationErrors.datasetShortNameInvalid));
        }


        [TestCategory("Core DatasetService")]
        [TestMethod]
        public async Task Validate_ShortName_Default()
        {
            //Arrange
            var context = new Mock<IDatasetContext>();
            var quartermasterService = new Mock<IQuartermasterService>();
            var validationResults = new ValidationResults();
            quartermasterService.Setup(f => f.VerifyNamedEnvironmentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NamedEnvironmentType>()).Result).Returns(validationResults);
            var datasetService = new DatasetService(context.Object, null, null, null, null, quartermasterService.Object, null, null, null);
            var dataset = new DatasetSchemaDto()
            {
                DatasetName = "Foo",
                DatasetType = GlobalConstants.DataEntityCodes.DATASET,
                DatasetCategoryIds = new List<int> { 1 },
                ShortName = GlobalConstants.SecurityConstants.ASSET_LEVEL_GROUP_NAME
            };

            // Act
            var result = await datasetService.ValidateAsync(dataset);

            // Assert
            Assert.IsTrue(result.ValidationResults.GetAll().Count >= 1); 
            Assert.IsTrue(result.ValidationResults.Contains(Dataset.ValidationErrors.datasetShortNameInvalid));
        }

        [TestCategory("Core DatasetService")]
        [TestMethod]
        public async Task Validate_ShortName_Duplicate()
        {
            //Arrange
            var context = new Mock<IDatasetContext>();
            var datasets = new[] { new Dataset() {
                DatasetId = 17,
                DatasetName = "Foo",
                ShortName = "Andrew",
                DatasetType = GlobalConstants.DataEntityCodes.DATASET,
                DatasetCategories = new List<Category> { new Category() { Id=1 } }
            } };
            context.Setup(f => f.Datasets).Returns(datasets.AsQueryable());

            var quartermasterService = new Mock<IQuartermasterService>();
            var validationResults = new ValidationResults();
            quartermasterService.Setup(f => f.VerifyNamedEnvironmentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NamedEnvironmentType>()).Result).Returns(validationResults);

            var datasetService = new DatasetService(context.Object, null, null, null, null, quartermasterService.Object, null, null, null);
            var dataset = new DatasetSchemaDto()
            {
                DatasetId = 0,
                DatasetName = "FooBar",
                DatasetType = GlobalConstants.DataEntityCodes.DATASET,
                DatasetCategoryIds = new List<int> { 1 },
                ShortName = "Andrew"
            };

            // Act
            var result = await datasetService.ValidateAsync(dataset);

            // Assert
            Assert.IsTrue(result.ValidationResults.GetAll().Count > 0);
            Assert.IsTrue(result.ValidationResults.Contains(Dataset.ValidationErrors.datasetShortNameDuplicate));
        }

        [TestCategory("Core DatasetService")]
        [TestMethod]
        public async Task Validate_ShortName_Duplicate_ButInDifferentEnvironments()
        {
            //Arrange
            var context = new Mock<IDatasetContext>();
            var datasets = new[] { new Dataset() {
                DatasetId = 17,
                DatasetName = "Foo",
                ShortName = "Andrew",
                DatasetType = GlobalConstants.DataEntityCodes.DATASET,
                DatasetCategories = new List<Category> { new Category() { Id=1 } },
                NamedEnvironment = "QUAL"
            } };
            context.Setup(f => f.Datasets).Returns(datasets.AsQueryable());

            var quartermasterService = new Mock<IQuartermasterService>();
            var validationResults = new ValidationResults();
            quartermasterService.Setup(f => f.VerifyNamedEnvironmentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NamedEnvironmentType>()).Result).Returns(validationResults);

            var datasetService = new DatasetService(context.Object, null, null, null, null, quartermasterService.Object, null, null, null);
            var dataset = new DatasetSchemaDto()
            {
                DatasetId = 0,
                DatasetName = "FooBar",
                DatasetType = GlobalConstants.DataEntityCodes.DATASET,
                DatasetCategoryIds = new List<int> { 1 },
                ShortName = "Andrew",
                NamedEnvironment = "PROD"
            };

            // Act
            var result = await datasetService.ValidateAsync(dataset);

            // Assert
            Assert.IsFalse(result.ValidationResults.Contains(Dataset.ValidationErrors.datasetShortNameDuplicate));
        }

        [TestCategory("Core DatasetService")]
        [TestMethod]
        public async Task Validate_Success()
        {
            //Arrange
            var context = new Mock<IDatasetContext>();
            var datasets = new[] { new Dataset() {
                DatasetId = 1000,
                DatasetName = "FooBar",
                DatasetType = GlobalConstants.DataEntityCodes.DATASET,
                DatasetCategories = new List<Category> { new Category() { Id=1 } },
                ShortName = "Andrew",
                PrimaryContactId = "067664",
                Asset = new Asset() {SaidKeyCode="ABCD"},
                OriginationCode = ((int)DatasetOriginationCode.Internal).ToString()
            } };
            context.Setup(f => f.Datasets).Returns(datasets.AsQueryable());
            var quartermasterService = new Mock<IQuartermasterService>();
            var validationResults = new ValidationResults();
            quartermasterService.Setup(f => f.VerifyNamedEnvironmentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NamedEnvironmentType>()).Result).Returns(validationResults);

            var datasetService = new DatasetService(context.Object, null, null, null, null, quartermasterService.Object, null, null, null);
            var dataset = new DatasetSchemaDto()
            {
                DatasetId = 1000,
                DatasetName = "FooBar",
                DatasetType = GlobalConstants.DataEntityCodes.DATASET,
                DatasetCategoryIds = new List<int> { 1 },
                ShortName = "Andrew",
                PrimaryContactId = "067664",
                SAIDAssetKeyCode = "ABCD",
                OriginationId = (int)DatasetOriginationCode.Internal
            };

            // Act
            var result = await datasetService.ValidateAsync(dataset);

            // Assert
            Assert.IsTrue(result.ValidationResults.GetAll().Count == 0);
        }

        [TestCategory("Core DatasetService")]
        [TestMethod]
        public void UpdateAndSaveDataset_DatasetName_Idempotent()
        {
            //Arrange
            Setup_UpdateAndSaveDataset(out var newDataset, out var datasetService, (DatasetSchemaDto a) => a.DatasetName = "NewName");

            //Act/Assert
            Assert.ThrowsException<ValidationException>(() => datasetService.UpdateAndSaveDataset(newDataset));
        }

        [TestCategory("Core DatasetService")]
        [TestMethod]
        public void UpdateAndSaveDataset_ShortName_Idempotent()
        {
            //Arrange
            Setup_UpdateAndSaveDataset(out var newDataset, out var datasetService, (DatasetSchemaDto a) => a.ShortName = "NewName");

            //Act/Assert
            Assert.ThrowsException<ValidationException>(() => datasetService.UpdateAndSaveDataset(newDataset));
        }

        [TestCategory("Core DatasetService")]
        [TestMethod]
        public void UpdateAndSaveDataset_SAIDKeyCode_Idempotent()
        {
            //Arrange
            Setup_UpdateAndSaveDataset(out var newDataset, out var datasetService, (DatasetSchemaDto a) => a.SAIDAssetKeyCode = "NEWN");

            //Act/Assert
            Assert.ThrowsException<ValidationException>(() => datasetService.UpdateAndSaveDataset(newDataset));
        }

        [TestCategory("Core DatasetService")]
        [TestMethod]
        public void UpdateAndSaveDataset_NamedEnvironment_Idempotent()
        {
            //Arrange
            Setup_UpdateAndSaveDataset(out var newDataset, out var datasetService, (DatasetSchemaDto a) => a.NamedEnvironment = "PROD");

            //Act/Assert
            Assert.ThrowsException<ValidationException>(() => datasetService.UpdateAndSaveDataset(newDataset));
        }

        [TestCategory("Core DatasetService")]
        [TestMethod]
        public void UpdateAndSaveDataset_NamedEnvironmentType_Idempotent()
        {
            //Arrange
            Setup_UpdateAndSaveDataset(out var newDataset, out var datasetService, (DatasetSchemaDto a) => a.NamedEnvironmentType = NamedEnvironmentType.Prod);

            //Act/Assert
            Assert.ThrowsException<ValidationException>(() => datasetService.UpdateAndSaveDataset(newDataset));
        }

        private static void Setup_UpdateAndSaveDataset(out DatasetSchemaDto newDataset, out DatasetService datasetService, Action<DatasetSchemaDto> datasetDtoUpdateAction)
        {
            var context = new Mock<IDatasetContext>();
            var dataset = new Dataset()
            {
                DatasetId = 1,
                DatasetName = "Foo",
                ShortName = "Foo",
                DatasetType = GlobalConstants.DataEntityCodes.DATASET,
                Asset = new Asset() { SaidKeyCode = "ABCD" },
                NamedEnvironment = "TEST",
                NamedEnvironmentType = NamedEnvironmentType.NonProd
            };
            newDataset = new DatasetSchemaDto()
            {
                DatasetId = 1,
                DatasetName = "Foo",
                ShortName = "Foo",
                DatasetType = GlobalConstants.DataEntityCodes.DATASET,
                SAIDAssetKeyCode = "ABCD",
                NamedEnvironment = "TEST",
                NamedEnvironmentType = NamedEnvironmentType.NonProd
            };
            //run the requested update
            datasetDtoUpdateAction.Invoke(newDataset);

            context.Setup(f => f.GetById<Dataset>(It.IsAny<int>())).Returns(dataset);
            datasetService = new DatasetService(context.Object, null, null, null, null, null, null, null, null);
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


        [TestMethod]
        public void GetExceptRows_Test()
        {
            // Arrange 
            MockRepository mr = new MockRepository(MockBehavior.Default);
            Mock<IConfigService> configService = mr.Create<IConfigService>();
            Mock<ISnowProvider> snowProvider = new Mock<ISnowProvider>();

            SchemaConsumptionSnowflakeDto schemaConsumptionDto = new SchemaConsumptionSnowflakeDto()
            {
                SnowflakeDatabase = "db_test",
                SnowflakeSchema = "schema",
                SnowflakeTable = "table",
                SnowflakeType = SnowflakeConsumptionType.DatasetSchemaParquet
            };

            List<SchemaConsumptionDto> schemaConsumptionDtos = new List<SchemaConsumptionDto>() { schemaConsumptionDto };

            FileSchemaDto fileSchemaDto = new FileSchemaDto()
            {
                SchemaId = 1,
                ConsumptionDetails = schemaConsumptionDtos
            };

            List<FileSchemaDto> fileSchemaDtos = new List<FileSchemaDto>() { fileSchemaDto };

            List<DatasetFileConfigDto> datasetFileConfigDtos = new List<DatasetFileConfigDto>();

            DatasetFileConfigDto configDto = new DatasetFileConfigDto()
            {
                Schema = fileSchemaDto
            };

            datasetFileConfigDtos.Add(configDto);

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("ETL_FILE_NAME");
            dataTable.Rows.Add("agentevents_20220827235951657_20220828045952000.json");
            dataTable.Rows.Add("agentevents_20220911155957552_20220911205958000.json");
            dataTable.Rows.Add("agentevents_20220818090453182_20220818140454000.json");

            configService.Setup(cs => cs.GetDatasetFileConfigDtoByDataset(It.IsAny<int>())).Returns(datasetFileConfigDtos);

            snowProvider.Setup(sp => sp.GetExceptRows(It.IsAny<SnowCompareConfig>())).Returns(dataTable);
            snowProvider.Setup(sp => sp.CheckIfExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            AuditService auditService = new AuditService(configService.Object, snowProvider.Object);

            // Act
            BaseAuditDto baseAuditDto = auditService.GetExceptRows(1,1,"query",AuditSearchType.dateSelect);

            // Assert
            Assert.AreEqual("agentevents_20220827235951657_20220828045952000.json", baseAuditDto.AuditDtos[0].DatasetFileName);
            Assert.AreEqual("agentevents_20220911155957552_20220911205958000.json", baseAuditDto.AuditDtos[1].DatasetFileName);
            Assert.AreEqual("agentevents_20220818090453182_20220818140454000.json", baseAuditDto.AuditDtos[2].DatasetFileName);
        }

        [TestMethod]
        public void GetExceptRows_Test_Null_SchemaObject()
        {
            // Arrange 
            MockRepository mr = new MockRepository(MockBehavior.Default);
            Mock<IConfigService> configService = mr.Create<IConfigService>();
            Mock<ISnowProvider> snowProvider = new Mock<ISnowProvider>();

            SchemaConsumptionSnowflakeDto schemaConsumptionDto = new SchemaConsumptionSnowflakeDto()
            {
                SnowflakeDatabase = "db_test",
                SnowflakeSchema = "schema",
                SnowflakeTable = "table",
                SnowflakeType = SnowflakeConsumptionType.DatasetSchemaRaw
            };

            List<SchemaConsumptionDto> schemaConsumptionDtos = new List<SchemaConsumptionDto>() { schemaConsumptionDto };

            FileSchemaDto fileSchemaDto = new FileSchemaDto()
            {
                SchemaId = 1,
                ConsumptionDetails = schemaConsumptionDtos
            };

            List<FileSchemaDto> fileSchemaDtos = new List<FileSchemaDto>() { fileSchemaDto };

            List<DatasetFileConfigDto> datasetFileConfigDtos = new List<DatasetFileConfigDto>();

            DatasetFileConfigDto configDto = new DatasetFileConfigDto()
            {
                Schema = fileSchemaDto
            };

            datasetFileConfigDtos.Add(configDto);

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("ETL_FILE_NAME");
            dataTable.Rows.Add("agentevents_20220827235951657_20220828045952000.json");
            dataTable.Rows.Add("agentevents_20220911155957552_20220911205958000.json");
            dataTable.Rows.Add("agentevents_20220818090453182_20220818140454000.json");

            configService.Setup(cs => cs.GetDatasetFileConfigDtoByDataset(It.IsAny<int>())).Returns(datasetFileConfigDtos);

            snowProvider.Setup(sp => sp.GetExceptRows(It.IsAny<SnowCompareConfig>())).Returns(dataTable);
            snowProvider.Setup(sp => sp.CheckIfExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            AuditService auditService = new AuditService(configService.Object, snowProvider.Object);

            // Act
            BaseAuditDto baseAuditDto = auditService.GetExceptRows(1, 1, "query", AuditSearchType.dateSelect);

            // Assert
            Assert.AreEqual("agentevents_20220827235951657_20220828045952000.json", baseAuditDto.AuditDtos[0].DatasetFileName);
            Assert.AreEqual("agentevents_20220911155957552_20220911205958000.json", baseAuditDto.AuditDtos[1].DatasetFileName);
            Assert.AreEqual("agentevents_20220818090453182_20220818140454000.json", baseAuditDto.AuditDtos[2].DatasetFileName);
        }

        [TestMethod]
        public void GetExceptRows_Check_If_Table_Exists_ArgumentException_Thrown()
        {
            // Arrange 
            MockRepository mr = new MockRepository(MockBehavior.Default);
            Mock<IConfigService> configService = mr.Create<IConfigService>();
            Mock<ISnowProvider> snowProvider = new Mock<ISnowProvider>();

            SchemaConsumptionSnowflakeDto schemaConsumptionDto = new SchemaConsumptionSnowflakeDto()
            {
                SnowflakeDatabase = "db_test",
                SnowflakeSchema = "schema",
                SnowflakeTable = "table",
                SnowflakeType = SnowflakeConsumptionType.DatasetSchemaParquet
            };

            List<SchemaConsumptionDto> schemaConsumptionDtos = new List<SchemaConsumptionDto>() { schemaConsumptionDto };

            FileSchemaDto fileSchemaDto = new FileSchemaDto()
            {
                SchemaId = 1,
                ConsumptionDetails = schemaConsumptionDtos
            };

            List<FileSchemaDto> fileSchemaDtos = new List<FileSchemaDto>() { fileSchemaDto };

            List<DatasetFileConfigDto> datasetFileConfigDtos = new List<DatasetFileConfigDto>();

            DatasetFileConfigDto configDto = new DatasetFileConfigDto()
            {
                Schema = fileSchemaDto
            };

            datasetFileConfigDtos.Add(configDto);

            configService.Setup(cs => cs.GetDatasetFileConfigDtoByDataset(It.IsAny<int>())).Returns(datasetFileConfigDtos);
            snowProvider.Setup(sp => sp.CheckIfExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            AuditService auditService = new AuditService(configService.Object, snowProvider.Object);

            // Act/Assert
            Assert.ThrowsException<ArgumentException>(() => auditService.GetExceptRows(1, 1, "query", AuditSearchType.dateSelect));
        }

        [TestMethod]
        public void GetRowCountCompare_Test()
        {
            // Arrange 
            MockRepository mr = new MockRepository(MockBehavior.Default);
            Mock<IConfigService> configService = mr.Create<IConfigService>();
            Mock<ISnowProvider> snowProvider = new Mock<ISnowProvider>();

            SchemaConsumptionSnowflakeDto schemaConsumptionDto = new SchemaConsumptionSnowflakeDto()
            {
                SnowflakeDatabase = "db_test",
                SnowflakeSchema = "schema",
                SnowflakeTable = "table",
                SnowflakeType = SnowflakeConsumptionType.DatasetSchemaParquet
            };

            List<SchemaConsumptionDto> schemaConsumptionDtos = new List<SchemaConsumptionDto>() { schemaConsumptionDto };

            FileSchemaDto fileSchemaDto = new FileSchemaDto()
            {
                SchemaId = 1,
                ConsumptionDetails = schemaConsumptionDtos
            };

            List<FileSchemaDto> fileSchemaDtos = new List<FileSchemaDto>() { fileSchemaDto };

            DatasetFileConfigDto configDto = new DatasetFileConfigDto()
            {
                Schema = fileSchemaDto
            };

            List<DatasetFileConfigDto> datasetFileConfigDtos = new List<DatasetFileConfigDto>() { configDto };

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("ETL_FILE_NAME");
            dataTable.Columns.Add("PAR_COUNT");
            dataTable.Columns.Add("RAW_COUNT");
            dataTable.Rows.Add("agentevents_20220827235951657_20220828045952000.json",10,10);
            dataTable.Rows.Add("agentevents_20220911155957552_20220911205958000.json",20,20);
            dataTable.Rows.Add("agentevents_20220818090453182_20220818140454000.json",20,0);

            configService.Setup(cs => cs.GetDatasetFileConfigDtoByDataset(It.IsAny<int>())).Returns(datasetFileConfigDtos);

            snowProvider.Setup(sp => sp.GetCompareRows(It.IsAny<SnowCompareConfig>())).Returns(dataTable);
            snowProvider.Setup(sp => sp.CheckIfExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            AuditService auditService = new AuditService(configService.Object, snowProvider.Object);

            // Act
            BaseAuditDto baseAuditDto = auditService.GetRowCountCompare(1, 1, "query", AuditSearchType.dateSelect);

            // Assert
            Assert.AreEqual("agentevents_20220827235951657_20220828045952000.json", baseAuditDto.AuditDtos[0].DatasetFileName);
            Assert.AreEqual("agentevents_20220911155957552_20220911205958000.json", baseAuditDto.AuditDtos[1].DatasetFileName);
            Assert.AreEqual("agentevents_20220818090453182_20220818140454000.json", baseAuditDto.AuditDtos[2].DatasetFileName);

            Assert.AreEqual(10, baseAuditDto.AuditDtos[0].ParquetRowCount);
            Assert.AreEqual(20, baseAuditDto.AuditDtos[1].ParquetRowCount);
            Assert.AreEqual(20, baseAuditDto.AuditDtos[2].ParquetRowCount);

            Assert.AreEqual(10, baseAuditDto.AuditDtos[0].RawqueryRowCount);
            Assert.AreEqual(20, baseAuditDto.AuditDtos[1].RawqueryRowCount);
            Assert.AreEqual(0,  baseAuditDto.AuditDtos[2].RawqueryRowCount);
        }

        [TestMethod]
        public void GetRowCountCompare_Test_Null_SchemaObject()
        {
            // Arrange 
            MockRepository mr = new MockRepository(MockBehavior.Default);
            Mock<IConfigService> configService = mr.Create<IConfigService>();
            Mock<ISnowProvider> snowProvider = new Mock<ISnowProvider>();

            SchemaConsumptionSnowflakeDto schemaConsumptionDto = new SchemaConsumptionSnowflakeDto()
            {
                SnowflakeDatabase = "db_test",
                SnowflakeSchema = "schema",
                SnowflakeTable = "table",
                SnowflakeType = SnowflakeConsumptionType.DatasetSchemaRawQuery
            };

            List<SchemaConsumptionDto> schemaConsumptionDtos = new List<SchemaConsumptionDto>() { schemaConsumptionDto };

            FileSchemaDto fileSchemaDto = new FileSchemaDto()
            {
                SchemaId = 1,
                ConsumptionDetails = schemaConsumptionDtos
            };

            List<FileSchemaDto> fileSchemaDtos = new List<FileSchemaDto>() { fileSchemaDto };

            DatasetFileConfigDto configDto = new DatasetFileConfigDto()
            {
                Schema = fileSchemaDto
            };

            List<DatasetFileConfigDto> datasetFileConfigDtos = new List<DatasetFileConfigDto>() { configDto };

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("ETL_FILE_NAME");
            dataTable.Columns.Add("PAR_COUNT");
            dataTable.Columns.Add("RAW_COUNT");
            dataTable.Rows.Add("agentevents_20220827235951657_20220828045952000.json", 10, 10);
            dataTable.Rows.Add("agentevents_20220911155957552_20220911205958000.json", 20, 20);
            dataTable.Rows.Add("agentevents_20220818090453182_20220818140454000.json", 20, 0);

            configService.Setup(cs => cs.GetDatasetFileConfigDtoByDataset(It.IsAny<int>())).Returns(datasetFileConfigDtos);

            snowProvider.Setup(sp => sp.GetCompareRows(It.IsAny<SnowCompareConfig>())).Returns(dataTable);
            snowProvider.Setup(sp => sp.CheckIfExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            AuditService auditService = new AuditService(configService.Object, snowProvider.Object);

            // Act
            BaseAuditDto baseAuditDto = auditService.GetRowCountCompare(1, 1, "query", AuditSearchType.dateSelect);

            // Assert
            Assert.AreEqual("agentevents_20220827235951657_20220828045952000.json", baseAuditDto.AuditDtos[0].DatasetFileName);
            Assert.AreEqual("agentevents_20220911155957552_20220911205958000.json", baseAuditDto.AuditDtos[1].DatasetFileName);
            Assert.AreEqual("agentevents_20220818090453182_20220818140454000.json", baseAuditDto.AuditDtos[2].DatasetFileName);

            Assert.AreEqual(10, baseAuditDto.AuditDtos[0].ParquetRowCount);
            Assert.AreEqual(20, baseAuditDto.AuditDtos[1].ParquetRowCount);
            Assert.AreEqual(20, baseAuditDto.AuditDtos[2].ParquetRowCount);

            Assert.AreEqual(10, baseAuditDto.AuditDtos[0].RawqueryRowCount);
            Assert.AreEqual(20, baseAuditDto.AuditDtos[1].RawqueryRowCount);
            Assert.AreEqual(0, baseAuditDto.AuditDtos[2].RawqueryRowCount);
        }

        [TestMethod]
        public void GetCompareRows_Check_If_Table_Exists_ArgumentException_Thrown()
        {
            // Arrange 
            MockRepository mr = new MockRepository(MockBehavior.Default);
            Mock<IConfigService> configService = mr.Create<IConfigService>();
            Mock<ISnowProvider> snowProvider = new Mock<ISnowProvider>();

            SchemaConsumptionSnowflakeDto schemaConsumptionDto = new SchemaConsumptionSnowflakeDto()
            {
                SnowflakeDatabase = "db_test",
                SnowflakeSchema = "schema",
                SnowflakeTable = "table",
                SnowflakeType = SnowflakeConsumptionType.DatasetSchemaParquet
            };

            List<SchemaConsumptionDto> schemaConsumptionDtos = new List<SchemaConsumptionDto>() { schemaConsumptionDto };

            FileSchemaDto fileSchemaDto = new FileSchemaDto()
            {
                SchemaId = 1,
                ConsumptionDetails = schemaConsumptionDtos
            };

            List<FileSchemaDto> fileSchemaDtos = new List<FileSchemaDto>() { fileSchemaDto };

            List<DatasetFileConfigDto> datasetFileConfigDtos = new List<DatasetFileConfigDto>();

            DatasetFileConfigDto configDto = new DatasetFileConfigDto()
            {
                Schema = fileSchemaDto
            };

            datasetFileConfigDtos.Add(configDto);

            configService.Setup(cs => cs.GetDatasetFileConfigDtoByDataset(It.IsAny<int>())).Returns(datasetFileConfigDtos);
            snowProvider.Setup(sp => sp.CheckIfExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            AuditService auditService = new AuditService(configService.Object, snowProvider.Object);

            // Act/Assert
            Assert.ThrowsException<ArgumentException>(() => auditService.GetRowCountCompare(1, 1, "query", AuditSearchType.dateSelect));
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
            var datasetService = new DatasetService(context.Object, new Mock<ISecurityService>().Object, null, null, null, null, new Mock<ISAIDService>().Object, null, null);

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

        [TestMethod]
        public void GetDatasetFileTableQueryable_1_ActiveNonBundled()
        {
            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);

            datasetContext.SetupGet(x => x.DatasetFileStatusActive).Returns(new List<DatasetFile>() {
                new DatasetFile()
                {
                    DatasetFileId = 1,
                    IsBundled = false,
                    DatasetFileConfig = new DatasetFileConfig() { ConfigId = 1 },
                },
                new DatasetFile()
                {
                    DatasetFileId = 2,
                    IsBundled = true,
                    DatasetFileConfig = new DatasetFileConfig() { ConfigId = 1 },
                },
                new DatasetFile()
                {
                    DatasetFileId = 4,
                    IsBundled = false,
                    DatasetFileConfig = new DatasetFileConfig() { ConfigId = 1 },
                    ParentDatasetFileId = 32
                },
                new DatasetFile()
                {
                    DatasetFileId = 5,
                    IsBundled = false,
                    DatasetFileConfig = new DatasetFileConfig() { ConfigId = 2 }
                }
            }.AsQueryable());

            DatasetService datasetService = new DatasetService(datasetContext.Object, null, null, null, null, null, null, null, null);

            List<DatasetFile> datasetFiles = datasetService.GetDatasetFileTableQueryable(1).ToList();

            Assert.AreEqual(1, datasetFiles.Count);

            DatasetFile datasetFile = datasetFiles.First();
            Assert.AreEqual(1, datasetFile.DatasetFileId);
            Assert.AreEqual(false, datasetFile.IsBundled);
            Assert.AreEqual(1, datasetFile.DatasetFileConfig.ConfigId);

            datasetContext.VerifyAll();
        }

        [TestMethod]
        public void GetDatasetDetailDto_1_DatasetDetailDto()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Category category = new Category() 
            { 
                Id = 6,
                Color = "Blue",
                Name = "CategoryName"
            };

            DatasetFileConfig datasetFileConfig = new DatasetFileConfig()
            {
                ConfigId = 2,
                Name = "FileConfigName",
                Description = "FileConfigDescription",
                Schema = new FileSchema()
                {
                    SchemaId = 3,
                    Delimiter = ","                    
                },
                FileExtension = new FileExtension()
                {
                    Id = 4                    
                },
                DatasetScopeType = new DatasetScopeType()
                {
                    ScopeTypeId = 5,
                    Name = "ScopeTypeName",
                    Description = "ScopeTypeDescription"
                },
                DeleteInd = false
            };

            Favorite favorite = new Favorite() { UserId = "000002" };

            Dataset dataset = new Dataset()
            {
                PrimaryContactId = "000000",
                IsSecured = true,
                DatasetId = 1,
                DatasetCategories = new List<Category>() { category },
                DatasetName = "DatasetName",
                ShortName = "ShortName",
                DatasetDesc = "Description",
                DatasetInformation = "Information",
                DatasetType = "Type",
                DataClassification = DataClassificationType.Public,
                ObjectStatus = ObjectStatusEnum.Active,
                CreationUserName = "000000",
                UploadUserName = "000001",
                DatasetDtm = new DateTime(2022, 6, 21, 8, 0, 0),
                ChangedDtm = new DateTime(2022, 6, 21, 9, 0, 0),
                CanDisplay = true,
                OriginationCode = "Internal",
                DatasetFileConfigs = new List<DatasetFileConfig>() { datasetFileConfig },
                Asset = new Asset() { SaidKeyCode = "CODE" },
                NamedEnvironment = "TEST",
                NamedEnvironmentType = NamedEnvironmentType.NonProd,
                Favorities = new List<Favorite>() { favorite },
                DatasetFiles = new List<DatasetFile>()
            };

            Mock<IDatasetContext> datasetContext = mockRepository.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.DatasetFileConfigs).Returns(new List<DatasetFileConfig>().AsQueryable());
            datasetContext.SetupGet(x => x.Security).Returns(new List<Security>().AsQueryable());
            datasetContext.SetupGet(x => x.SecurityTicket).Returns(new List<SecurityTicket>().AsQueryable());
            datasetContext.SetupGet(x => x.Datasets).Returns(new List<Dataset>() { dataset }.AsQueryable());
            datasetContext.SetupGet(x => x.Events).Returns(new List<Event>().AsQueryable());
            datasetContext.Setup(x => x.IsUserSubscribedToDataset("000002", 1)).Returns(false);
            datasetContext.Setup(x => x.GetAllUserSubscriptionsForDataset("000002", 1)).Returns(new List<DatasetSubscription>());

            Mock<IExtendedUserInfo> extendedUserInfo = mockRepository.Create<IExtendedUserInfo>();
            extendedUserInfo.SetupGet(x => x.FamiliarName).Returns("");
            extendedUserInfo.SetupGet(x => x.FirstName).Returns("Foo");
            extendedUserInfo.SetupGet(x => x.LastName).Returns("Bar");
            extendedUserInfo.SetupGet(x => x.EmailAddress).Returns("foobar@gmail.com");
            ApplicationUser applicationUser = new ApplicationUser(null, extendedUserInfo.Object);

            Mock<IExtendedUserInfo> extendedUserInfo2 = mockRepository.Create<IExtendedUserInfo>();
            extendedUserInfo2.SetupGet(x => x.FamiliarName).Returns("Lorem");
            extendedUserInfo2.SetupGet(x => x.LastName).Returns("Ipsum");
            ApplicationUser applicationUser2 = new ApplicationUser(null, extendedUserInfo2.Object);

            Mock<IExtendedUserInfo> extendedUserInfo3 = mockRepository.Create<IExtendedUserInfo>();
            extendedUserInfo3.SetupGet(x => x.UserId).Returns("000002");
            ApplicationUser applicationUser3 = new ApplicationUser(null, extendedUserInfo3.Object);

            Mock<IExtendedUserInfo> extendedUserInfo4 = mockRepository.Create<IExtendedUserInfo>();
            ApplicationUser applicationUser4 = new ApplicationUser(null, extendedUserInfo4.Object);

            Mock<IUserService> userService = mockRepository.Create<IUserService>();
            userService.Setup(x => x.GetByAssociateId("000000")).Returns(applicationUser);
            userService.Setup(x => x.GetByAssociateId("000001")).Returns(applicationUser2);
            userService.SetupSequence(x => x.GetCurrentUser()).Returns(applicationUser4).Returns(applicationUser3);

            Mock<ISecurityService> securityService = mockRepository.Create<ISecurityService>();
            UserSecurity userSecurity = new UserSecurity();
            securityService.Setup(x => x.GetGroupAccessCount(dataset)).Returns(1);
            securityService.Setup(x => x.GetUserSecurity(dataset, applicationUser4)).Returns(userSecurity);

            DatasetService datasetService = new DatasetService(datasetContext.Object, securityService.Object, userService.Object, null, null, null, null, null, null);

            DatasetDetailDto dto = datasetService.GetDatasetDetailDto(1);

            mockRepository.VerifyAll();

            Assert.AreEqual(userSecurity, dto.Security);
            Assert.AreEqual("000000", dto.PrimaryContactId);
            Assert.IsTrue(dto.IsSecured);
            Assert.AreEqual(1, dto.DatasetId);
            Assert.AreEqual(1, dto.DatasetCategoryIds.Count);
            Assert.IsTrue(dto.DatasetCategoryIds.Any(x => x == 6));
            Assert.AreEqual("DatasetName", dto.DatasetName);
            Assert.AreEqual("ShortName", dto.ShortName);
            Assert.AreEqual("Description", dto.DatasetDesc);
            Assert.AreEqual("Information", dto.DatasetInformation);
            Assert.AreEqual("Type", dto.DatasetType);
            Assert.AreEqual(DataClassificationType.Public, dto.DataClassification);
            Assert.AreEqual("Blue", dto.CategoryColor);
            Assert.AreEqual(ObjectStatusEnum.Active, dto.ObjectStatus);
            Assert.AreEqual("000000", dto.CreationUserId);
            Assert.AreEqual("000000", dto.CreationUserName);
            Assert.AreEqual("Foo Bar", dto.PrimaryContactName);
            Assert.AreEqual("foobar@gmail.com", dto.PrimaryContactEmail);
            Assert.AreEqual("000001", dto.UploadUserId);
            Assert.AreEqual("Lorem Ipsum", dto.UploadUserName);
            Assert.AreEqual(new DateTime(2022, 6, 21, 8, 0, 0), dto.DatasetDtm);
            Assert.AreEqual(new DateTime(2022, 6, 21, 9, 0, 0), dto.ChangedDtm);
            Assert.IsTrue(dto.CanDisplay);
            Assert.AreEqual(1, dto.OriginationId);
            Assert.AreEqual("FileConfigDescription", dto.ConfigFileDesc);
            Assert.AreEqual("FileConfigName", dto.ConfigFileName);
            Assert.AreEqual(",", dto.Delimiter);
            Assert.AreEqual(4, dto.FileExtensionId);
            Assert.AreEqual(5, dto.DatasetScopeTypeId);
            Assert.AreEqual("CategoryName", dto.CategoryName);
            Assert.AreEqual(1, dto.CategoryNames.Count);
            Assert.IsTrue(dto.CategoryNames.Any(x => x == "CategoryName"));
            Assert.AreEqual("CODE", dto.SAIDAssetKeyCode);
            Assert.AreEqual("TEST", dto.NamedEnvironment);
            Assert.AreEqual(NamedEnvironmentType.NonProd, dto.NamedEnvironmentType);
            Assert.AreEqual(0, dto.Downloads);
            Assert.IsFalse(dto.IsSubscribed);
            Assert.AreEqual(0, dto.AmountOfSubscriptions);
            Assert.AreEqual(0, dto.Views);
            Assert.IsTrue(dto.IsFavorite);
            Assert.AreEqual(1, dto.DatasetFileConfigSchemas.Count);
            Assert.IsTrue(dto.DatasetFileConfigSchemas.Any(x => x.ConfigId == 2 && x.SchemaId == 3 && x.SchemaName == "FileConfigName"));
            Assert.AreEqual(1, dto.DatasetScopeTypeNames.Count);
            Assert.IsTrue(dto.DatasetScopeTypeNames.Any(x => x.Key == "ScopeTypeName" && x.Value == "ScopeTypeDescription"));
            Assert.AreEqual(0, dto.DatasetFileCount);
            Assert.AreEqual("Internal", dto.OriginationCode);
            Assert.AreEqual("Public", dto.DataClassificationDescription);
            Assert.AreEqual("Blue", dto.CategoryColor);
            Assert.AreEqual(1, dto.GroupAccessCount);
        }
    }
}
