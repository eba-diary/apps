using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.Jira;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class SchemaFlowServiceTests
    {
        [TestMethod]
        public void AddSchemaAsync_SchemaFlowDto_SchemaResultDto()
        {
            SchemaFlowDto addDto = new SchemaFlowDto
            {
                SchemaDto = new FileSchemaDto
                {
                    ParentDatasetId = 1
                },
                DatasetFileConfigDto = new DatasetFileConfigDto
                {
                    DatasetScopeTypeName = "Appending"
                },
                DataFlowDto = new DataFlowDto
                {
                    DatasetId = 1
                }
            };

            MockRepository mr = new MockRepository(MockBehavior.Strict);
            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();

            Dataset dataset = new Dataset
            {
                ShortName = "Short"
            };
            datasetContext.Setup(x => x.GetById(1)).Returns(dataset);

            List<DataFlowStep> dataFlowSteps = new List<DataFlowStep>
            {
                new DataFlowStep
                {
                    DataAction_Type_Id = DataActionType.ProducerS3Drop,
                    DataFlow = new DataFlow { Id = 5 },
                    TriggerBucket = "Bucket",
                    TriggerKey = "Key"
                }
            };
            datasetContext.SetupGet(x => x.DataFlowStep).Returns(dataFlowSteps.AsQueryable());

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(user.Object);

            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            UserSecurity userSecurity = new UserSecurity { CanManageSchema = true };
            securityService.Setup(x => x.GetUserSecurity(dataset, user.Object)).Returns(userSecurity);

            Mock<ISchemaService> schemaService = mr.Create<ISchemaService>();
            FileSchemaDto resultFileSchemaDto = new FileSchemaDto
            {
                SchemaId = 2,
                Name = "SchemaName",
                FileExtensionName = ExtensionNames.CSV,
                FileExtensionId = 3,
                Delimiter = ",",
                HasHeader = true,
                Description = "Description",
                CreateCurrentView = true,
                ObjectStatus = ObjectStatusEnum.Active,
                SchemaRootPath = "root,path",
                ParentDatasetId = 1,
                StorageCode = "1234567",
                ControlMTriggerName = "DATA_DEV_SHORT_SCHEMANAME_COMPLETED",
                CreateDateTime = new DateTime(2023, 3, 1),
                UpdateDateTime = new DateTime(2023, 3, 1)
            };
            schemaService.Setup(x => x.AddSchemaAsync(addDto.SchemaDto)).ReturnsAsync(resultFileSchemaDto);

            Mock<IConfigService> configService = mr.Create<IConfigService>();
            configService.Setup(x => x.Create(addDto.DatasetFileConfigDto)).Returns(4);

            Mock<IDataFlowService> dataFlowService = mr.Create<IDataFlowService>();
            DataFlowDto resultDataFlowDto = new DataFlowDto
            {
                Name = "Name",
                SaidKeyCode = "SAID",
                ObjectStatus = ObjectStatusEnum.Active,
                IngestionType = (int)IngestionType.S3_Drop,
                NamedEnvironment = "DEV",
                NamedEnvironmentType = NamedEnvironmentType.NonProd,
                PrimaryContactId = "000001",
                IsSecured = true,
                PreProcessingOption = 0,
                DatasetId = 1,
                FlowStorageCode = "1234567",
                CreateDTM = new DateTime(2023, 3, 2),
                Id = 5
            };
            dataFlowService.Setup(x => x.AddDataFlowAsync(addDto.DataFlowDto)).ReturnsAsync(resultDataFlowDto);

            SchemaFlowService schemaFlowService = new SchemaFlowService(configService.Object, schemaService.Object, dataFlowService.Object, datasetContext.Object, userService.Object, securityService.Object);

            SchemaResultDto result = schemaFlowService.AddSchemaAsync(addDto).Result;

            Assert.AreEqual(2, result.SchemaId);
            Assert.AreEqual("Description", result.SchemaDescription);
            Assert.AreEqual(",", result.Delimiter);
            Assert.IsTrue(result.HasHeader);
            Assert.AreEqual("Appending", result.ScopeTypeCode);
            Assert.AreEqual(ExtensionNames.CSV, result.FileTypeCode);
            Assert.AreEqual("root,path", result.SchemaRootPath);
            Assert.IsTrue(result.CreateCurrentView);
            Assert.AreEqual(IngestionType.S3_Drop, result.IngestionType);
            Assert.IsFalse(result.IsCompressed);
            Assert.IsNull(result.CompressionTypeCode);
            Assert.IsFalse(result.IsPreprocessingRequired);
            Assert.IsNull(result.PreprocessingTypeCode);
            Assert.AreEqual(1, result.DatasetId);
            Assert.AreEqual("SchemaName", result.SchemaName);
            Assert.AreEqual("SAID", result.SaidAssetCode);
            Assert.AreEqual("DEV", result.NamedEnvironment);
            Assert.AreEqual(NamedEnvironmentType.NonProd, result.NamedEnvironmentType);
            Assert.IsNull(result.KafkaTopicName);
            Assert.AreEqual("000001", result.PrimaryContactId);
            Assert.AreEqual("1234567", result.StorageCode);
            Assert.AreEqual("Bucket/Key", result.DropLocation);
            Assert.AreEqual("DATA_DEV_SHORT_SCHEMANAME_COMPLETED", result.ControlMTriggerName);
            Assert.AreEqual(ObjectStatusEnum.Active, result.ObjectStatus);
            Assert.AreEqual(new DateTime(2023, 3, 1), result.CreateDateTime);
            Assert.AreEqual(new DateTime(2023, 3, 2), result.UpdateDateTime);

            mr.VerifyAll();
        }

        [TestMethod]
        public void AddSchemaAsync_SchemaFlowDto_ClearContext_ThrowException()
        {
            SchemaFlowDto addDto = new SchemaFlowDto
            {
                SchemaDto = new FileSchemaDto { ParentDatasetId = 1 }
            };

            MockRepository mr = new MockRepository(MockBehavior.Strict);
            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();

            Dataset dataset = new Dataset
            {
                ShortName = "Short"
            };
            datasetContext.Setup(x => x.GetById(1)).Returns(dataset);
            datasetContext.Setup(x => x.Clear());

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(user.Object);

            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            UserSecurity userSecurity = new UserSecurity { CanManageSchema = true };
            securityService.Setup(x => x.GetUserSecurity(dataset, user.Object)).Returns(userSecurity);

            Mock<ISchemaService> schemaService = mr.Create<ISchemaService>();
            schemaService.Setup(x => x.AddSchemaAsync(addDto.SchemaDto)).ThrowsAsync(new Exception());

            SchemaFlowService schemaFlowService = new SchemaFlowService(null, schemaService.Object, null, datasetContext.Object, userService.Object, securityService.Object);

            Assert.ThrowsExceptionAsync<Exception>(() => schemaFlowService.AddSchemaAsync(addDto));

            mr.VerifyAll();
        }

        [TestMethod]
        public void AddSchemaAsync_SchemaFlowDto_FailPermission_ThrowResourceForbidden()
        {
            SchemaFlowDto addDto = new SchemaFlowDto
            {
                SchemaDto = new FileSchemaDto { ParentDatasetId = 1 }
            };

            MockRepository mr = new MockRepository(MockBehavior.Strict);
            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();

            Dataset dataset = new Dataset
            {
                ShortName = "Short"
            };
            datasetContext.Setup(x => x.GetById(1)).Returns(dataset);

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(user.Object);

            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            UserSecurity userSecurity = new UserSecurity { CanManageSchema = false };
            securityService.Setup(x => x.GetUserSecurity(dataset, user.Object)).Returns(userSecurity);

            SchemaFlowService schemaFlowService = new SchemaFlowService(null, null, null, datasetContext.Object, userService.Object, securityService.Object);

            Assert.ThrowsExceptionAsync<ResourceNotFoundException>(() => schemaFlowService.AddSchemaAsync(addDto));

            mr.VerifyAll();
        }

        [TestMethod]
        public void UpdateSchemaAsync_SchemaFlowDto_GenerateConsumptionLayerEvents_SchemaResultDto()
        {
            SchemaFlowDto updateDto = new SchemaFlowDto
            {
                SchemaDto = new FileSchemaDto
                {
                    SchemaId = 2,
                    SchemaRootPath = "root,path",
                    CreateCurrentView = true,
                },
                DatasetFileConfigDto = new DatasetFileConfigDto
                {
                    DatasetScopeTypeName = "Appending"
                },
                DataFlowDto = new DataFlowDto()
            };

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();

            FileSchema schema = new FileSchema
            {
                SchemaId = 2,
                ObjectStatus = ObjectStatusEnum.Active,
                CLA1286_KafkaFlag = true,
            };
            datasetContext.SetupGet(x => x.FileSchema).Returns(new List<FileSchema> { schema }.AsQueryable());

            Mock<ISchemaService> schemaService = mr.Create<ISchemaService>();
            FileSchemaDto resultFileSchemaDto = new FileSchemaDto
            {
                SchemaId = 2,
                Name = "SchemaName",
                FileExtensionName = ExtensionNames.CSV,
                FileExtensionId = 3,
                Delimiter = ",",
                HasHeader = true,
                Description = "Description",
                CreateCurrentView = true,
                ObjectStatus = ObjectStatusEnum.Active,
                SchemaRootPath = "root,path",
                ParentDatasetId = 1,
                StorageCode = "1234567",
                CLA1286_KafkaFlag = true,
                ControlMTriggerName = "DATA_DEV_SHORT_SCHEMANAME_COMPLETED",
                CreateDateTime = new DateTime(2023, 3, 1),
                UpdateDateTime = new DateTime(2023, 3, 3)
            };
            schemaService.Setup(x => x.UpdateSchemaAsync(updateDto.SchemaDto, schema)).ReturnsAsync(resultFileSchemaDto);
            schemaService.Setup(x => x.GenerateConsumptionLayerEvents(schema, It.Is<JObject>(j => j["createcurrentview"].Value<bool>())));

            DatasetFileConfig fileConfig = new DatasetFileConfig
            {
                ParentDataset = new Dataset { DatasetId = 1 },
                Schema = schema
            };
            datasetContext.SetupGet(x => x.DatasetFileConfigs).Returns(new List<DatasetFileConfig> { fileConfig }.AsQueryable());

            Mock<IConfigService> configService = mr.Create<IConfigService>();
            configService.Setup(x => x.UpdateDatasetFileConfig(updateDto.DatasetFileConfigDto, fileConfig));

            DataFlow dataFlow = new DataFlow
            {
                SchemaId = 2,
                ObjectStatus = ObjectStatusEnum.Active
            };
            datasetContext.SetupGet(x => x.DataFlow).Returns(new List<DataFlow> { dataFlow }.AsQueryable());

            Mock<IDataFlowService> dataFlowService = mr.Create<IDataFlowService>();
            DataFlowDto resultDataFlowDto = new DataFlowDto
            {
                Name = "Name",
                SaidKeyCode = "SAID",
                ObjectStatus = ObjectStatusEnum.Active,
                IngestionType = (int)IngestionType.S3_Drop,
                NamedEnvironment = "DEV",
                NamedEnvironmentType = NamedEnvironmentType.NonProd,
                PrimaryContactId = "000001",
                IsSecured = true,
                PreProcessingOption = 0,
                DatasetId = 1,
                FlowStorageCode = "1234567",
                CreateDTM = new DateTime(2023, 3, 2),
                Id = 5
            };
            dataFlowService.Setup(x => x.UpdateDataFlowAsync(updateDto.DataFlowDto, dataFlow)).ReturnsAsync(resultDataFlowDto);

            List<DataFlowStep> dataFlowSteps = new List<DataFlowStep>
            {
                new DataFlowStep
                {
                    DataAction_Type_Id = DataActionType.ProducerS3Drop,
                    DataFlow = new DataFlow { Id = 5 },
                    TriggerBucket = "Bucket",
                    TriggerKey = "Key"
                }
            };
            datasetContext.SetupGet(x => x.DataFlowStep).Returns(dataFlowSteps.AsQueryable());
            datasetContext.Setup(x => x.SaveChangesAsync(true, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(user.Object);

            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            UserSecurity userSecurity = new UserSecurity { CanManageSchema = true };
            securityService.Setup(x => x.GetUserSecurity(fileConfig.ParentDataset, user.Object)).Returns(userSecurity);

            SchemaFlowService schemaFlowService = new SchemaFlowService(configService.Object, schemaService.Object, dataFlowService.Object, datasetContext.Object, userService.Object, securityService.Object);

            SchemaResultDto result = schemaFlowService.UpdateSchemaAsync(updateDto).Result;

            Assert.IsTrue(updateDto.DataFlowDto.DataFlowStepUpdateRequired);
            Assert.IsTrue(updateDto.SchemaDto.CLA1286_KafkaFlag);
            Assert.AreEqual(3, updateDto.DatasetFileConfigDto.FileExtensionId);

            Assert.AreEqual(2, result.SchemaId);
            Assert.AreEqual("Description", result.SchemaDescription);
            Assert.AreEqual(",", result.Delimiter);
            Assert.IsTrue(result.HasHeader);
            Assert.AreEqual("Appending", result.ScopeTypeCode);
            Assert.AreEqual(ExtensionNames.CSV, result.FileTypeCode);
            Assert.AreEqual("root,path", result.SchemaRootPath);
            Assert.IsTrue(result.CreateCurrentView);
            Assert.AreEqual(IngestionType.S3_Drop, result.IngestionType);
            Assert.IsFalse(result.IsCompressed);
            Assert.IsNull(result.CompressionTypeCode);
            Assert.IsFalse(result.IsPreprocessingRequired);
            Assert.IsNull(result.PreprocessingTypeCode);
            Assert.AreEqual(1, result.DatasetId);
            Assert.AreEqual("SchemaName", result.SchemaName);
            Assert.AreEqual("SAID", result.SaidAssetCode);
            Assert.AreEqual("DEV", result.NamedEnvironment);
            Assert.AreEqual(NamedEnvironmentType.NonProd, result.NamedEnvironmentType);
            Assert.IsNull(result.KafkaTopicName);
            Assert.AreEqual("000001", result.PrimaryContactId);
            Assert.AreEqual("1234567", result.StorageCode);
            Assert.AreEqual("Bucket/Key", result.DropLocation);
            Assert.AreEqual("DATA_DEV_SHORT_SCHEMANAME_COMPLETED", result.ControlMTriggerName);
            Assert.AreEqual(ObjectStatusEnum.Active, result.ObjectStatus);
            Assert.AreEqual(new DateTime(2023, 3, 1), result.CreateDateTime);
            Assert.AreEqual(new DateTime(2023, 3, 3), result.UpdateDateTime);

            mr.VerifyAll();
        }

        [TestMethod]
        public void UpdateSchemaAsync_SchemaFlowDto_SchemaResultDto()
        {
            SchemaFlowDto updateDto = new SchemaFlowDto
            {
                SchemaDto = new FileSchemaDto
                {
                    SchemaId = 2,
                    SchemaRootPath = "root"
                },
                DatasetFileConfigDto = new DatasetFileConfigDto
                {
                    DatasetScopeTypeName = "Appending"
                },
                DataFlowDto = new DataFlowDto
                {
                    IngestionType = (int)IngestionType.DFS_Drop
                }
            };

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();

            FileSchema schema = new FileSchema
            {
                SchemaId = 2,
                ObjectStatus = ObjectStatusEnum.Active,
                SchemaRootPath = "path"
            };
            datasetContext.SetupGet(x => x.FileSchema).Returns(new List<FileSchema> { schema }.AsQueryable());

            Mock<ISchemaService> schemaService = mr.Create<ISchemaService>();
            FileSchemaDto resultFileSchemaDto = new FileSchemaDto
            {
                SchemaId = 2,
                Name = "SchemaName",
                FileExtensionName = ExtensionNames.CSV,
                FileExtensionId = 3,
                Delimiter = ",",
                HasHeader = true,
                Description = "Description",
                CreateCurrentView = true,
                ObjectStatus = ObjectStatusEnum.Active,
                ParentDatasetId = 1,
                StorageCode = "1234567",
                CLA1286_KafkaFlag = true,
                ControlMTriggerName = "DATA_DEV_SHORT_SCHEMANAME_COMPLETED",
                CreateDateTime = new DateTime(2023, 3, 1),
                UpdateDateTime = new DateTime(2023, 3, 2)
            };
            schemaService.Setup(x => x.UpdateSchemaAsync(updateDto.SchemaDto, schema)).ReturnsAsync(resultFileSchemaDto);

            DatasetFileConfig fileConfig = new DatasetFileConfig
            {
                ParentDataset = new Dataset { DatasetId = 1 },
                Schema = schema
            };
            datasetContext.SetupGet(x => x.DatasetFileConfigs).Returns(new List<DatasetFileConfig> { fileConfig }.AsQueryable());

            Mock<IConfigService> configService = mr.Create<IConfigService>();
            configService.Setup(x => x.UpdateDatasetFileConfig(updateDto.DatasetFileConfigDto, fileConfig));

            DataFlow dataFlow = new DataFlow
            {
                SchemaId = 2,
                ObjectStatus = ObjectStatusEnum.Active,
                Id = 5
            };
            datasetContext.SetupGet(x => x.DataFlow).Returns(new List<DataFlow> { dataFlow }.AsQueryable());

            Mock<IDataFlowService> dataFlowService = mr.Create<IDataFlowService>();
            DataFlowDto resultDataFlowDto = new DataFlowDto
            {
                Name = "Name",
                SaidKeyCode = "SAID",
                ObjectStatus = ObjectStatusEnum.Active,
                IngestionType = (int)IngestionType.DFS_Drop,
                NamedEnvironment = "DEV",
                NamedEnvironmentType = NamedEnvironmentType.NonProd,
                PrimaryContactId = "000001",
                IsSecured = true,
                PreProcessingOption = 0,
                DatasetId = 1,
                FlowStorageCode = "1234567",
                CreateDTM = new DateTime(2023, 3, 3),
                Id = 5
            };
            dataFlowService.Setup(x => x.UpdateDataFlowAsync(updateDto.DataFlowDto, dataFlow)).ReturnsAsync(resultDataFlowDto);

            List<RetrieverJob> jobs = new List<RetrieverJob>
            {
                new RetrieverJob
                {
                    DataFlow = dataFlow,
                    ObjectStatus = ObjectStatusEnum.Active,
                    DataSource = new DfsNonProdSource()
                    {
                        BaseUri = new Uri("c:/tmp/nonprod/")
                    },
                    RelativeUri = "relative"                   
                }
            };
            datasetContext.SetupGet(x => x.RetrieverJob).Returns(jobs.AsQueryable());
            datasetContext.Setup(x => x.SaveChangesAsync(true, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(user.Object);

            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            UserSecurity userSecurity = new UserSecurity { CanManageSchema = true };
            securityService.Setup(x => x.GetUserSecurity(fileConfig.ParentDataset, user.Object)).Returns(userSecurity);

            SchemaFlowService schemaFlowService = new SchemaFlowService(configService.Object, schemaService.Object, dataFlowService.Object, datasetContext.Object, userService.Object, securityService.Object);

            SchemaResultDto result = schemaFlowService.UpdateSchemaAsync(updateDto).Result;

            Assert.IsFalse(updateDto.DataFlowDto.DataFlowStepUpdateRequired);
            Assert.IsFalse(updateDto.SchemaDto.CLA1286_KafkaFlag);
            Assert.AreEqual(3, updateDto.DatasetFileConfigDto.FileExtensionId);

            Assert.AreEqual(2, result.SchemaId);
            Assert.AreEqual("Description", result.SchemaDescription);
            Assert.AreEqual(",", result.Delimiter);
            Assert.IsTrue(result.HasHeader);
            Assert.AreEqual("Appending", result.ScopeTypeCode);
            Assert.AreEqual(ExtensionNames.CSV, result.FileTypeCode);
            Assert.IsNull(result.SchemaRootPath);
            Assert.IsTrue(result.CreateCurrentView);
            Assert.AreEqual(IngestionType.DFS_Drop, result.IngestionType);
            Assert.IsFalse(result.IsCompressed);
            Assert.IsNull(result.CompressionTypeCode);
            Assert.IsFalse(result.IsPreprocessingRequired);
            Assert.IsNull(result.PreprocessingTypeCode);
            Assert.AreEqual(1, result.DatasetId);
            Assert.AreEqual("SchemaName", result.SchemaName);
            Assert.AreEqual("SAID", result.SaidAssetCode);
            Assert.AreEqual("DEV", result.NamedEnvironment);
            Assert.AreEqual(NamedEnvironmentType.NonProd, result.NamedEnvironmentType);
            Assert.IsNull(result.KafkaTopicName);
            Assert.AreEqual("000001", result.PrimaryContactId);
            Assert.AreEqual("1234567", result.StorageCode);
            Assert.AreEqual("file:///c:/tmp/nonprod/relative", result.DropLocation);
            Assert.AreEqual("DATA_DEV_SHORT_SCHEMANAME_COMPLETED", result.ControlMTriggerName);
            Assert.AreEqual(ObjectStatusEnum.Active, result.ObjectStatus);
            Assert.AreEqual(new DateTime(2023, 3, 1), result.CreateDateTime);
            Assert.AreEqual(new DateTime(2023, 3, 3), result.UpdateDateTime);

            mr.VerifyAll();
        }

        [TestMethod]
        public void UpdateSchemaAsync_SchemaFlowDto_FailPermission_ThrowResourceForbidden()
        {
            SchemaFlowDto updateDto = new SchemaFlowDto
            {
                SchemaDto = new FileSchemaDto
                {
                    SchemaId = 2
                },
                DataFlowDto = new DataFlowDto
                {
                    IngestionType = (int)IngestionType.DFS_Drop
                }
            };

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();

            FileSchema schema = new FileSchema
            {
                SchemaId = 2,
                ObjectStatus = ObjectStatusEnum.Active
            };
            datasetContext.SetupGet(x => x.FileSchema).Returns(new List<FileSchema> { schema }.AsQueryable());

            DatasetFileConfig fileConfig = new DatasetFileConfig
            {
                ParentDataset = new Dataset { DatasetId = 1 },
                Schema = schema
            };
            datasetContext.SetupGet(x => x.DatasetFileConfigs).Returns(new List<DatasetFileConfig> { fileConfig }.AsQueryable());

            DataFlow dataFlow = new DataFlow
            {
                SchemaId = 2,
                ObjectStatus = ObjectStatusEnum.Active
            };
            datasetContext.SetupGet(x => x.DataFlow).Returns(new List<DataFlow> { dataFlow }.AsQueryable());

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(user.Object);

            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            UserSecurity userSecurity = new UserSecurity { CanManageSchema = false };
            securityService.Setup(x => x.GetUserSecurity(fileConfig.ParentDataset, user.Object)).Returns(userSecurity);

            SchemaFlowService schemaFlowService = new SchemaFlowService(null, null, null, datasetContext.Object, userService.Object, securityService.Object);

            Assert.ThrowsExceptionAsync<ResourceForbiddenException>(() => schemaFlowService.UpdateSchemaAsync(updateDto));

            mr.VerifyAll();
        }

        [TestMethod]
        public void UpdateSchemaAsync_AddSchemaDto_ClearContext_ThrowException()
        {
            SchemaFlowDto updateDto = new SchemaFlowDto
            {
                SchemaDto = new FileSchemaDto
                {
                    SchemaId = 2
                },
                DataFlowDto = new DataFlowDto
                {
                    IngestionType = (int)IngestionType.DFS_Drop
                }
            };

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();

            FileSchema schema = new FileSchema
            {
                SchemaId = 2,
                ObjectStatus = ObjectStatusEnum.Active
            };
            datasetContext.SetupGet(x => x.FileSchema).Returns(new List<FileSchema> { schema }.AsQueryable());

            Mock<ISchemaService> schemaService = mr.Create<ISchemaService>();
            schemaService.Setup(x => x.UpdateSchemaAsync(updateDto.SchemaDto, schema)).ThrowsAsync(new Exception());

            DatasetFileConfig fileConfig = new DatasetFileConfig
            {
                ParentDataset = new Dataset { DatasetId = 1 },
                Schema = schema
            };
            datasetContext.SetupGet(x => x.DatasetFileConfigs).Returns(new List<DatasetFileConfig> { fileConfig }.AsQueryable());

            DataFlow dataFlow = new DataFlow
            {
                SchemaId = 2,
                ObjectStatus = ObjectStatusEnum.Active
            };
            datasetContext.SetupGet(x => x.DataFlow).Returns(new List<DataFlow> { dataFlow }.AsQueryable());

            datasetContext.Setup(x => x.Clear());

            Mock<IApplicationUser> user = mr.Create<IApplicationUser>();
            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(user.Object);

            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            UserSecurity userSecurity = new UserSecurity { CanManageSchema = true };
            securityService.Setup(x => x.GetUserSecurity(fileConfig.ParentDataset, user.Object)).Returns(userSecurity);

            SchemaFlowService schemaFlowService = new SchemaFlowService(null, schemaService.Object, null, datasetContext.Object, userService.Object, securityService.Object);

            Assert.ThrowsExceptionAsync<Exception>(() => schemaFlowService.UpdateSchemaAsync(updateDto));

            mr.VerifyAll();
        }

        [TestMethod]
        public void UpdateSchemaAsync_SchemaFlowDto_ThrowResourceNotFound_FileSchema()
        {
            SchemaFlowDto updateDto = new SchemaFlowDto
            {
                SchemaDto = new FileSchemaDto
                {
                    SchemaId = 2
                },
                DataFlowDto = new DataFlowDto
                {
                    IngestionType = (int)IngestionType.DFS_Drop
                }
            };

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();

            datasetContext.SetupGet(x => x.FileSchema).Returns(new List<FileSchema>().AsQueryable());

            DatasetFileConfig fileConfig = new DatasetFileConfig
            {
                ParentDataset = new Dataset { DatasetId = 1 },
                Schema = new FileSchema { SchemaId = 2 }
            };
            datasetContext.SetupGet(x => x.DatasetFileConfigs).Returns(new List<DatasetFileConfig> { fileConfig }.AsQueryable());

            DataFlow dataFlow = new DataFlow
            {
                SchemaId = 2,
                ObjectStatus = ObjectStatusEnum.Active
            };
            datasetContext.SetupGet(x => x.DataFlow).Returns(new List<DataFlow> { dataFlow }.AsQueryable());

            SchemaFlowService schemaFlowService = new SchemaFlowService(null, null, null, datasetContext.Object, null, null);

            Assert.ThrowsExceptionAsync<ResourceNotFoundException>(() => schemaFlowService.UpdateSchemaAsync(updateDto));

            mr.VerifyAll();
        }

        [TestMethod]
        public void UpdateSchemaAsync_SchemaFlowDto_ThrowResourceNotFound_DatasetFileConfig()
        {
            SchemaFlowDto updateDto = new SchemaFlowDto
            {
                SchemaDto = new FileSchemaDto
                {
                    SchemaId = 2
                },
                DataFlowDto = new DataFlowDto
                {
                    IngestionType = (int)IngestionType.DFS_Drop
                }
            };

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();

            FileSchema schema = new FileSchema
            {
                SchemaId = 2,
                ObjectStatus = ObjectStatusEnum.Active
            };
            datasetContext.SetupGet(x => x.FileSchema).Returns(new List<FileSchema> { schema }.AsQueryable());

            datasetContext.SetupGet(x => x.DatasetFileConfigs).Returns(new List<DatasetFileConfig>().AsQueryable());

            DataFlow dataFlow = new DataFlow
            {
                SchemaId = 2,
                ObjectStatus = ObjectStatusEnum.Active
            };
            datasetContext.SetupGet(x => x.DataFlow).Returns(new List<DataFlow> { dataFlow }.AsQueryable());

            SchemaFlowService schemaFlowService = new SchemaFlowService(null, null, null, datasetContext.Object, null, null);

            Assert.ThrowsExceptionAsync<ResourceNotFoundException>(() => schemaFlowService.UpdateSchemaAsync(updateDto));

            mr.VerifyAll();
        }

        [TestMethod]
        public void UpdateSchemaAsync_SchemaFlowDto_ThrowResourceNotFound_DataFlow()
        {
            SchemaFlowDto updateDto = new SchemaFlowDto
            {
                SchemaDto = new FileSchemaDto
                {
                    SchemaId = 2
                },
                DataFlowDto = new DataFlowDto
                {
                    IngestionType = (int)IngestionType.DFS_Drop
                }
            };

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();

            FileSchema schema = new FileSchema
            {
                SchemaId = 2,
                ObjectStatus = ObjectStatusEnum.Active
            };
            datasetContext.SetupGet(x => x.FileSchema).Returns(new List<FileSchema> { schema }.AsQueryable());

            DatasetFileConfig fileConfig = new DatasetFileConfig
            {
                ParentDataset = new Dataset { DatasetId = 1 },
                Schema = new FileSchema { SchemaId = 2 }
            };
            datasetContext.SetupGet(x => x.DatasetFileConfigs).Returns(new List<DatasetFileConfig> { fileConfig }.AsQueryable());

            datasetContext.SetupGet(x => x.DataFlow).Returns(new List<DataFlow>().AsQueryable());

            SchemaFlowService schemaFlowService = new SchemaFlowService(null, null, null, datasetContext.Object, null, null);

            Assert.ThrowsExceptionAsync<ResourceNotFoundException>(() => schemaFlowService.UpdateSchemaAsync(updateDto));

            mr.VerifyAll();
        }
    }
}