using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class SchemaFlowServiceTests
    {
        [TestMethod]
        public void AddSchemaAsync_AddSchemaDto_SchemaResultDto()
        {
            AddSchemaDto addDto = new AddSchemaDto
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
            Assert.AreEqual("Bucket\\Key", result.DropLocation);
            Assert.AreEqual("DATA_DEV_SHORT_SCHEMANAME_COMPLETED", result.ControlMTriggerName);
            Assert.AreEqual(ObjectStatusEnum.Active, result.ObjectStatus);
            Assert.AreEqual(new DateTime(2023, 3, 1), result.CreateDateTime);
            Assert.AreEqual(new DateTime(2023, 3, 2), result.UpdateDateTime);

            mr.VerifyAll();
        }

        [TestMethod]
        public void AddSchemaAsync_AddSchemaDto_ClearContext_ThrowException()
        {
            AddSchemaDto addDto = new AddSchemaDto
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
        public void AddSchemaAsync_AddSchemaDto_FailPermission_ThrowResourceForbidden()
        {
            AddSchemaDto addDto = new AddSchemaDto
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
    }
}