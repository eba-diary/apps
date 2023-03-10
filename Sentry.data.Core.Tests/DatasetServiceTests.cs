using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

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
            MockRepository mr = new MockRepository(MockBehavior.Strict);
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

            snowProvider.Setup(sp => sp.GetNonParquetFiles(It.IsAny<SnowCompareConfig>())).Returns(dataTable);
            snowProvider.Setup(sp => sp.CheckIfExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            AuditService auditService = new AuditService(configService.Object, snowProvider.Object);

            // Act
            BaseAuditDto baseAuditDto = auditService.GetNonParquetFiles(1,1,"query",AuditSearchType.dateSelect);

            // Assert
            mr.VerifyAll();

            Assert.AreEqual("agentevents_20220827235951657_20220828045952000.json", baseAuditDto.AuditDtos[0].DatasetFileName);
            Assert.AreEqual("agentevents_20220911155957552_20220911205958000.json", baseAuditDto.AuditDtos[1].DatasetFileName);
            Assert.AreEqual("agentevents_20220818090453182_20220818140454000.json", baseAuditDto.AuditDtos[2].DatasetFileName);
        }

        [TestMethod]
        public void GetExceptRows_Test_Null_SchemaObject()
        {
            // Arrange 
            MockRepository mr = new MockRepository(MockBehavior.Strict);
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

            snowProvider.Setup(sp => sp.GetNonParquetFiles(It.IsAny<SnowCompareConfig>())).Returns(dataTable);
            snowProvider.Setup(sp => sp.CheckIfExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            AuditService auditService = new AuditService(configService.Object, snowProvider.Object);

            // Act
            BaseAuditDto baseAuditDto = auditService.GetNonParquetFiles(1, 1, "query", AuditSearchType.dateSelect);

            // Assert
            mr.VerifyAll();

            Assert.AreEqual("agentevents_20220827235951657_20220828045952000.json", baseAuditDto.AuditDtos[0].DatasetFileName);
            Assert.AreEqual("agentevents_20220911155957552_20220911205958000.json", baseAuditDto.AuditDtos[1].DatasetFileName);
            Assert.AreEqual("agentevents_20220818090453182_20220818140454000.json", baseAuditDto.AuditDtos[2].DatasetFileName);
        }

        [TestMethod]
        public void GetExceptRows_Check_If_Table_Exists_ArgumentException_Thrown()
        {
            // Arrange 
            MockRepository mr = new MockRepository(MockBehavior.Strict);
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
            snowProvider.Setup(sp => sp.CheckIfExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());

            AuditService auditService = new AuditService(configService.Object, snowProvider.Object);

            // Act/Assert
            Assert.ThrowsException<ArgumentException>(() => auditService.GetNonParquetFiles(1, 1, "query", AuditSearchType.dateSelect));

            mr.VerifyAll();
        }

        [TestMethod]
        public void GetRowCountCompare_Test()
        {
            // Arrange 
            MockRepository mr = new MockRepository(MockBehavior.Strict);
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

            snowProvider.Setup(sp => sp.GetComparedRowCount(It.IsAny<SnowCompareConfig>())).Returns(dataTable);
            snowProvider.Setup(sp => sp.CheckIfExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            AuditService auditService = new AuditService(configService.Object, snowProvider.Object);

            // Act
            BaseAuditDto baseAuditDto = auditService.GetComparedRowCount(1, 1, "query", AuditSearchType.dateSelect);

            // Assert
            mr.VerifyAll();

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
            MockRepository mr = new MockRepository(MockBehavior.Strict);
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

            snowProvider.Setup(sp => sp.GetComparedRowCount(It.IsAny<SnowCompareConfig>())).Returns(dataTable);
            snowProvider.Setup(sp => sp.CheckIfExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            AuditService auditService = new AuditService(configService.Object, snowProvider.Object);

            // Act
            BaseAuditDto baseAuditDto = auditService.GetComparedRowCount(1, 1, "query", AuditSearchType.dateSelect);

            // Assert
            mr.VerifyAll();

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
            snowProvider.Setup(sp => sp.CheckIfExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());

            AuditService auditService = new AuditService(configService.Object, snowProvider.Object);

            // Act/Assert
            Assert.ThrowsException<ArgumentException>(() => auditService.GetComparedRowCount(1, 1, "query", AuditSearchType.dateSelect));
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
            Assert.AreEqual(1, dto.DatasetScopeTypeNames.Count);
            Assert.IsTrue(dto.DatasetScopeTypeNames.Any(x => x.Key == "ScopeTypeName" && x.Value == "ScopeTypeDescription"));
            Assert.AreEqual(0, dto.DatasetFileCount);
            Assert.AreEqual("Internal", dto.OriginationCode);
            Assert.AreEqual("Public", dto.DataClassificationDescription);
            Assert.AreEqual("Blue", dto.CategoryColor);
            Assert.AreEqual(1, dto.GroupAccessCount);
        }




        [TestMethod]
        public void Ensure_DatasetRelatives_Matches_Same_Dataset_Name()
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
                DatasetName = "Jeb",
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

            Dataset dataset2 = new Dataset()
            {
                PrimaryContactId = "000000",
                IsSecured = true,
                DatasetId = 2,
                DatasetCategories = new List<Category>() { category },
                DatasetName = "Jeb",
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
                NamedEnvironment = "QUAL",
                NamedEnvironmentType = NamedEnvironmentType.NonProd,
                Favorities = new List<Favorite>() { favorite },
                DatasetFiles = new List<DatasetFile>()
            };




            Mock<IDatasetContext> datasetContext = mockRepository.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.DatasetFileConfigs).Returns(new List<DatasetFileConfig>().AsQueryable());
            datasetContext.SetupGet(x => x.Security).Returns(new List<Security>().AsQueryable());
            datasetContext.SetupGet(x => x.SecurityTicket).Returns(new List<SecurityTicket>().AsQueryable());
            datasetContext.SetupGet(x => x.Datasets).Returns(new List<Dataset>() { dataset,dataset2 }.AsQueryable());
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

            //should be exactly 2 total relatives that match 
            Assert.AreEqual(2, dto.DatasetRelatives.Count);
        }


        [TestMethod]
        public void Ensure_Dataset_Has_Zero_Relatives_Found()
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
                DatasetName = "Jeb",
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

            Dataset dataset2 = new Dataset()
            {
                PrimaryContactId = "000000",
                IsSecured = true,
                DatasetId = 2,
                DatasetCategories = new List<Category>() { category },
                DatasetName = "Jeb1",
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
                NamedEnvironment = "QUAL",
                NamedEnvironmentType = NamedEnvironmentType.NonProd,
                Favorities = new List<Favorite>() { favorite },
                DatasetFiles = new List<DatasetFile>()
            };




            Mock<IDatasetContext> datasetContext = mockRepository.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.DatasetFileConfigs).Returns(new List<DatasetFileConfig>().AsQueryable());
            datasetContext.SetupGet(x => x.Security).Returns(new List<Security>().AsQueryable());
            datasetContext.SetupGet(x => x.SecurityTicket).Returns(new List<SecurityTicket>().AsQueryable());
            datasetContext.SetupGet(x => x.Datasets).Returns(new List<Dataset>() { dataset, dataset2 }.AsQueryable());
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

            //should be exactly 1 total relatives since this guy has no relatives, just himself
            Assert.AreEqual(1, dto.DatasetRelatives.Count);
        }


        [TestMethod]
        public void Ensure_DatasetRelatives_Bring_Active_Only()
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
                DatasetName = "Jeb",
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

            Dataset dataset2 = new Dataset()
            {
                PrimaryContactId = "000000",
                IsSecured = true,
                DatasetId = 2,
                DatasetCategories = new List<Category>() { category },
                DatasetName = "Jeb",
                ShortName = "ShortName",
                DatasetDesc = "Description",
                DatasetInformation = "Information",
                DatasetType = "Type",
                DataClassification = DataClassificationType.Public,
                ObjectStatus = ObjectStatusEnum.Deleted,
                CreationUserName = "000000",
                UploadUserName = "000001",
                DatasetDtm = new DateTime(2022, 6, 21, 8, 0, 0),
                ChangedDtm = new DateTime(2022, 6, 21, 9, 0, 0),
                CanDisplay = true,
                OriginationCode = "Internal",
                DatasetFileConfigs = new List<DatasetFileConfig>() { datasetFileConfig },
                Asset = new Asset() { SaidKeyCode = "CODE" },
                NamedEnvironment = "QUAL",
                NamedEnvironmentType = NamedEnvironmentType.NonProd,
                Favorities = new List<Favorite>() { favorite },
                DatasetFiles = new List<DatasetFile>()
            };




            Mock<IDatasetContext> datasetContext = mockRepository.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.DatasetFileConfigs).Returns(new List<DatasetFileConfig>().AsQueryable());
            datasetContext.SetupGet(x => x.Security).Returns(new List<Security>().AsQueryable());
            datasetContext.SetupGet(x => x.SecurityTicket).Returns(new List<SecurityTicket>().AsQueryable());
            datasetContext.SetupGet(x => x.Datasets).Returns(new List<Dataset>() { dataset, dataset2 }.AsQueryable());
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

            //should be exactly 1 total relatives that match 
            Assert.AreEqual(1, dto.DatasetRelatives.Count);
        }

        [TestMethod]
        public void Create_For_Dataset()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            DatasetDto dto = MockClasses.MockDatasetDto(new List<Dataset>() { MockClasses.MockDataset() }).First();

            Mock<IDatasetContext> context = new Mock<IDatasetContext>();
            context.Setup(s => s.Add(It.IsAny<Dataset>()));
            context.SetupGet(s => s.Assets).Returns(new List<Asset>() { new Asset() { AssetId = 1, SaidKeyCode = "ABCD" } }.AsQueryable());

            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(s => s.GetCurrentUser().AssociateId).Returns("123456");

            var datasetService = new DatasetService(context.Object, null, userService.Object, null, null, null, null, null, null);

            //Act
            _ = datasetService.Create(dto);

            //Assert
            mr.VerifyAll();
            context.Verify(v => v.SaveChanges(It.IsAny<bool>()),Times.Never);
        }

        [TestMethod]
        public void DatasetExistsInTargetNamedEnvironment_ArgumentNullException()
        {
            DatasetService datasetService = new DatasetService(null, null, null, null, null, null, null, null, null);

            Assert.ThrowsException<ArgumentNullException>(() => datasetService.DatasetExistsInTargetNamedEnvironment(null, "ABCD", "TEST"), "DatasetName null value check failed");
            Assert.ThrowsException<ArgumentNullException>(() => datasetService.DatasetExistsInTargetNamedEnvironment("MyDataset", null, "TEST"), "SAID asset key non value check failed");
            Assert.ThrowsException<ArgumentNullException>(() => datasetService.DatasetExistsInTargetNamedEnvironment("MyDataset", "ABCD", null), "Target Named Enviornment non value check failed");
           
        }

        [TestMethod]
        public void DatasetExistsInTargetNamedEnvironment()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            List<Dataset> datasetList = new List<Dataset>()
            {
                new Dataset()
                {
                    DatasetId = 1,
                    DatasetName = "MyDataset",
                    Asset = new Asset() { SaidKeyCode = "ABCD" },
                    NamedEnvironment = "TEST",
                    ObjectStatus = ObjectStatusEnum.Active
                },
                new Dataset()
                {
                    DatasetId = 2,
                    DatasetName = "DeletedDataset",
                    Asset = new Asset() { SaidKeyCode = "ABCD" },
                    NamedEnvironment = "TEST",
                    ObjectStatus = ObjectStatusEnum.Deleted
                }
            };

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.Datasets).Returns(datasetList.AsQueryable());

            DatasetService datasetService = new DatasetService(context.Object, null, null, null, null, null, null, null, null);

            (int targetDatasetId, bool datasetExistsInTarget) resultTrue = datasetService.DatasetExistsInTargetNamedEnvironment("MyDataset", "ABCD", "TEST");
            (int targetDatasetId, bool datasetExistsInTarget) resultFalse = datasetService.DatasetExistsInTargetNamedEnvironment("DeletedDataset", "ABCD", "TEST");
            (int targetDatasetId, bool datasetExistsInTarget) resultFalseDatasetName = datasetService.DatasetExistsInTargetNamedEnvironment("YourDataset", "ABCD", "TEST");
            (int targetDatasetId, bool datasetExistsInTarget) resultFalseSaidAssetKey = datasetService.DatasetExistsInTargetNamedEnvironment("MyDataset", "WXYZ", "TEST");
            (int targetDatasetId, bool datasetExistsInTarget) resultFalseTargetNamedEnvironment = datasetService.DatasetExistsInTargetNamedEnvironment("MyDataset", "ABCD", "QUAL");


            Assert.IsTrue(resultTrue.datasetExistsInTarget);
            Assert.IsFalse(resultFalse.datasetExistsInTarget);
            Assert.AreEqual(1, resultTrue.targetDatasetId);
            Assert.IsFalse(resultFalseDatasetName.datasetExistsInTarget);
            Assert.AreEqual(0, resultFalseDatasetName.targetDatasetId);
            Assert.IsFalse(resultFalseSaidAssetKey.datasetExistsInTarget);
            Assert.AreEqual(0, resultFalseSaidAssetKey.targetDatasetId);
            Assert.IsFalse(resultFalseTargetNamedEnvironment.datasetExistsInTarget);
            Assert.AreEqual(0, resultFalseTargetNamedEnvironment.targetDatasetId);
        }

        [TestMethod]
        public void AddDatasetAsync_DatasetDto_DatasetResultDto()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IApplicationUser> applicationUser = mr.Create<IApplicationUser>();
            applicationUser.SetupGet(x => x.AssociateId).Returns("000001");

            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(applicationUser.Object);

            UserSecurity userSecurity = new UserSecurity { CanCreateDataset = true };
            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            securityService.Setup(x => x.GetUserSecurity(null, applicationUser.Object)).Returns(userSecurity);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            Asset asset = new Asset { SaidKeyCode = "SAID" };
            datasetContext.SetupGet(x => x.Assets).Returns(new List<Asset>{ asset }.AsQueryable());

            Category category = new Category { Name = "Category" };
            datasetContext.SetupGet(x => x.Categories).Returns(new List<Category> { category }.AsQueryable());
            datasetContext.Setup(x => x.AddAsync(It.IsAny<Dataset>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Callback<Dataset, CancellationToken>((x, token) =>
            {
                x.DatasetId = 1;
                Assert.AreEqual("Name", x.DatasetName);
                Assert.AreEqual("Short", x.ShortName);
                Assert.AreEqual("Description", x.DatasetDesc);
                Assert.AreEqual("Information", x.DatasetInformation);
                Assert.AreEqual("Creator", x.CreationUserName);
                Assert.AreEqual("000002", x.PrimaryContactId);
                Assert.AreEqual("000001", x.UploadUserName);
                Assert.AreEqual(DatasetOriginationCode.Internal.ToString(), x.OriginationCode);
                Assert.AreEqual(new DateTime(2023, 02, 21, 10, 0, 0), x.DatasetDtm);
                Assert.AreEqual(new DateTime(2023, 02, 21, 11, 0, 0), x.ChangedDtm);
                Assert.AreEqual(DataEntityCodes.DATASET, x.DatasetType);
                Assert.AreEqual(DataClassificationType.InternalUseOnly, x.DataClassification);
                Assert.IsTrue(x.IsSecured);
                Assert.IsFalse(x.DeleteInd);
                Assert.AreEqual(DateTime.MaxValue, x.DeleteIssueDTM);
                Assert.AreEqual(ObjectStatusEnum.Active, x.ObjectStatus);
                Assert.AreEqual(asset, x.Asset);
                Assert.AreEqual("DEV", x.NamedEnvironment);
                Assert.AreEqual(NamedEnvironmentType.NonProd, x.NamedEnvironmentType);
                Assert.AreEqual("me@sentry.com", x.AlternateContactEmail);
                Assert.AreEqual(category, x.DatasetCategories.First());
                Assert.IsNotNull(x.Security);
                Assert.AreEqual("000001", x.Security.CreatedById);
            });
            datasetContext.Setup(x => x.SaveChangesAsync(true, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            DatasetDto dto = new DatasetDto
            {
                SAIDAssetKeyCode = "SAID",
                DatasetName = "Name",
                ShortName = "Short",
                DatasetDesc = "Description",
                DatasetInformation = "Information",
                CreationUserId = "Creator",
                PrimaryContactId = "000002",
                OriginationId = (int)DatasetOriginationCode.Internal,
                DatasetDtm = new DateTime(2023, 02, 21, 10, 0, 0),
                ChangedDtm = new DateTime(2023, 02, 21, 11, 0, 0),
                DataClassification = DataClassificationType.InternalUseOnly,
                IsSecured = true,
                NamedEnvironment = "DEV",
                NamedEnvironmentType = NamedEnvironmentType.NonProd,
                AlternateContactEmail = "me@sentry.com",
                CategoryName = "Category"
            };

            DatasetService datasetService = new DatasetService(datasetContext.Object, securityService.Object, userService.Object, null, null, null, null, null, null);

            DatasetResultDto resultDto = datasetService.AddDatasetAsync(dto).Result;

            Assert.AreEqual(1, resultDto.DatasetId);
            Assert.AreEqual("Name", resultDto.DatasetName);
            Assert.AreEqual("Description", resultDto.DatasetDescription);
            Assert.AreEqual("Category", resultDto.CategoryName);
            Assert.AreEqual("Short", resultDto.ShortName);
            Assert.AreEqual("SAID", resultDto.SaidAssetCode);
            Assert.AreEqual("DEV", resultDto.NamedEnvironment);
            Assert.AreEqual(NamedEnvironmentType.NonProd, resultDto.NamedEnvironmentType);
            Assert.AreEqual("Information", resultDto.UsageInformation);
            Assert.AreEqual(DataClassificationType.InternalUseOnly, resultDto.DataClassificationType);
            Assert.IsTrue(resultDto.IsSecured);
            Assert.AreEqual("000002", resultDto.PrimaryContactId);
            Assert.AreEqual("me@sentry.com", resultDto.AlternateContactEmail);
            Assert.AreEqual(DatasetOriginationCode.Internal, resultDto.OriginationCode);
            Assert.AreEqual("Creator", resultDto.OriginalCreator);
            Assert.AreEqual(new DateTime(2023, 02, 21, 10, 0, 0), resultDto.CreateDateTime);
            Assert.AreEqual(new DateTime(2023, 02, 21, 11, 0, 0), resultDto.UpdateDateTime);
            Assert.AreEqual(ObjectStatusEnum.Active, resultDto.ObjectStatus);

            mr.VerifyAll();
        }

        [TestMethod]
        public void AddDatasetAsync_Forbidden_ThrowsResourceForbiddenException()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IApplicationUser> applicationUser = mr.Create<IApplicationUser>();

            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(applicationUser.Object);

            UserSecurity userSecurity = new UserSecurity { CanCreateDataset = false };
            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            securityService.Setup(x => x.GetUserSecurity(null, applicationUser.Object)).Returns(userSecurity);

            DatasetService datasetService = new DatasetService(null, securityService.Object, userService.Object, null, null, null, null, null, null);

            Assert.ThrowsExceptionAsync<ResourceForbiddenException>(() => datasetService.AddDatasetAsync(null));

            mr.VerifyAll();
        }

        [TestMethod]
        public void UpdateDatasetAsync_DatasetDto_DatasetResultDto()
        {
            DateTime now = DateTime.Now;

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IApplicationUser> applicationUser = mr.Create<IApplicationUser>();
            applicationUser.SetupGet(x => x.AssociateId).Returns("000003");

            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(applicationUser.Object);

            Category category = new Category { Name = "Category" };
            Asset asset = new Asset { SaidKeyCode = "SAID" };
            Security security = new Security { CreatedById = "000001" };
            Dataset ds = new Dataset
            {
                DatasetId = 1,
                DatasetName = "Name",
                ShortName = "Short",
                DatasetDesc = "Description",
                DatasetInformation = "Information",
                CreationUserName = "Creator",
                PrimaryContactId = "000002",
                UploadUserName = "000001",
                OriginationCode = DatasetOriginationCode.Internal.ToString(),
                DatasetDtm = new DateTime(2023, 02, 21, 10, 0, 0),
                ChangedDtm = new DateTime(2023, 02, 21, 11, 0, 0),
                DatasetType = DataEntityCodes.DATASET,
                DataClassification = DataClassificationType.InternalUseOnly,
                IsSecured = true,
                DeleteIssueDTM = DateTime.MaxValue,
                Asset = asset,
                NamedEnvironment = "DEV",
                NamedEnvironmentType = NamedEnvironmentType.NonProd,
                AlternateContactEmail = "me@sentry.com",
                DatasetCategories = new List<Category> { category },
                Security = security,
                ObjectStatus = ObjectStatusEnum.Active
            };

            UserSecurity userSecurity = new UserSecurity { CanEditDataset = true };
            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            securityService.Setup(x => x.GetUserSecurity(ds, applicationUser.Object)).Returns(userSecurity);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();

            datasetContext.Setup(x => x.GetById<Dataset>(1)).Returns(ds);

            Category other = new Category { Name = "Other" };
            List<Category> categories = new List<Category>
            {
                category,
                other
            };
            datasetContext.SetupGet(x => x.Categories).Returns(categories.AsQueryable());

            datasetContext.Setup(x => x.SaveChangesAsync(true, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Callback(() =>
            {
                Assert.AreEqual(1, ds.DatasetId);
                Assert.AreEqual("Name", ds.DatasetName);
                Assert.AreEqual("Short", ds.ShortName);
                Assert.AreEqual("Description New", ds.DatasetDesc);
                Assert.AreEqual("Information New", ds.DatasetInformation);
                Assert.AreEqual("Creator New", ds.CreationUserName);
                Assert.AreEqual("000003", ds.PrimaryContactId);
                Assert.AreEqual("000001", ds.UploadUserName);
                Assert.AreEqual(DatasetOriginationCode.External.ToString(), ds.OriginationCode);
                Assert.AreEqual(new DateTime(2023, 02, 21, 10, 0, 0), ds.DatasetDtm);
                Assert.IsTrue(ds.ChangedDtm >= now);
                Assert.AreEqual(DataEntityCodes.DATASET, ds.DatasetType);
                Assert.AreEqual(DataClassificationType.Public, ds.DataClassification);
                Assert.IsFalse(ds.IsSecured);
                Assert.IsFalse(ds.DeleteInd);
                Assert.AreEqual(DateTime.MaxValue, ds.DeleteIssueDTM);
                Assert.AreEqual(ObjectStatusEnum.Active, ds.ObjectStatus);
                Assert.AreEqual(asset, ds.Asset);
                Assert.AreEqual("DEV", ds.NamedEnvironment);
                Assert.AreEqual(NamedEnvironmentType.NonProd, ds.NamedEnvironmentType);
                Assert.AreEqual("you@sentry.com", ds.AlternateContactEmail);
                Assert.AreEqual(other, ds.DatasetCategories.First());
                Assert.AreEqual(security, ds.Security);
                Assert.AreEqual("000001", ds.Security.CreatedById);
                Assert.IsTrue(ds.Security.RemovedDate >= now);
                Assert.AreEqual("000003", ds.Security.UpdatedById);
            });

            DatasetDto dto = new DatasetDto
            {
                DatasetId = 1,
                DatasetDesc = "Description New",
                DatasetInformation = "Information New",
                CreationUserId = "Creator New",
                PrimaryContactId = "000003",
                OriginationId = (int)DatasetOriginationCode.External,
                DataClassification = DataClassificationType.Public,
                IsSecured = true,
                AlternateContactEmail = "you@sentry.com",
                CategoryName = "Other"
            };

            DatasetService datasetService = new DatasetService(datasetContext.Object, securityService.Object, userService.Object, null, null, null, null, null, null);

            DatasetResultDto resultDto = datasetService.UpdateDatasetAsync(dto).Result;

            Assert.AreEqual(1, resultDto.DatasetId);
            Assert.AreEqual("Name", resultDto.DatasetName);
            Assert.AreEqual("Description New", resultDto.DatasetDescription);
            Assert.AreEqual("Other", resultDto.CategoryName);
            Assert.AreEqual("Short", resultDto.ShortName);
            Assert.AreEqual("SAID", resultDto.SaidAssetCode);
            Assert.AreEqual("DEV", resultDto.NamedEnvironment);
            Assert.AreEqual(NamedEnvironmentType.NonProd, resultDto.NamedEnvironmentType);
            Assert.AreEqual("Information New", resultDto.UsageInformation);
            Assert.AreEqual(DataClassificationType.Public, resultDto.DataClassificationType);
            Assert.IsFalse(resultDto.IsSecured);
            Assert.AreEqual("000003", resultDto.PrimaryContactId);
            Assert.AreEqual("you@sentry.com", resultDto.AlternateContactEmail);
            Assert.AreEqual(DatasetOriginationCode.External, resultDto.OriginationCode);
            Assert.AreEqual("Creator New", resultDto.OriginalCreator);
            Assert.AreEqual(new DateTime(2023, 02, 21, 10, 0, 0), resultDto.CreateDateTime);
            Assert.IsTrue(resultDto.UpdateDateTime >= now);
            Assert.AreEqual(ObjectStatusEnum.Active, resultDto.ObjectStatus);

            mr.VerifyAll();
        }

        [TestMethod]
        public void UpdateDatasetAsync_Forbidden_ThrowsResourceForbiddenException()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IApplicationUser> applicationUser = mr.Create<IApplicationUser>();
            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(applicationUser.Object);

            Dataset ds = new Dataset();

            UserSecurity userSecurity = new UserSecurity { CanEditDataset = false };
            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            securityService.Setup(x => x.GetUserSecurity(ds, applicationUser.Object)).Returns(userSecurity);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();

            datasetContext.Setup(x => x.GetById<Dataset>(1)).Returns(ds);

            DatasetDto dto = new DatasetDto
            {
                DatasetId = 1
            };

            DatasetService datasetService = new DatasetService(datasetContext.Object, securityService.Object, userService.Object, null, null, null, null, null, null);

            Assert.ThrowsExceptionAsync<ResourceForbiddenException>(() => datasetService.UpdateDatasetAsync(dto));

            mr.VerifyAll();
        }



        [TestMethod]
        public void UpdateDatasetAsync_NotFound_ThrowsResourceNotFoundException()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();

            Dataset ds = null;
            datasetContext.Setup(x => x.GetById<Dataset>(1)).Returns(ds);

            DatasetDto dto = new DatasetDto
            {
                DatasetId = 1
            };

            DatasetService datasetService = new DatasetService(datasetContext.Object, null, null, null, null, null, null, null, null);

            Assert.ThrowsExceptionAsync<ResourceNotFoundException>(() => datasetService.UpdateDatasetAsync(dto));

            mr.VerifyAll();
        }
    }
}
