﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.GlobalEnums;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class DataFlowService_CreateDataFlowTests
    {
        [TestMethod]
        public void CreateDataFlow_FailCanCreateDataFlow_DataFlowUnauthorizedAccessException()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Mock<IApplicationUser> appUser = mockRepository.Create<IApplicationUser>();

            Mock<IUserService> userService = mockRepository.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(appUser.Object);

            Mock<ISecurityService> securityService = mockRepository.Create<ISecurityService>();
            UserSecurity userSecurity = new UserSecurity() { CanCreateDataFlow = false };
            securityService.Setup(x => x.GetUserSecurity(null, appUser.Object)).Returns(userSecurity);

            DataFlowService service = new DataFlowService(null, userService.Object, null, securityService.Object, null, null, null, null, null);

            DataFlowDto dto = new DataFlowDto();

            Assert.ThrowsException<DataFlowUnauthorizedAccessException>(() => service.CreateDataFlow(dto));

            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void CreateDataFlow_FailCanManageSchemaAndCanEditDataset_DatasetUnauthorizedAccessException()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mockRepository.Create<IDatasetContext>();
            Dataset dataset = new Dataset() { DatasetName = "Dataset Name" };
            datasetContext.Setup(x => x.GetById<Dataset>(1)).Returns(dataset);

            Mock<IApplicationUser> appUser = mockRepository.Create<IApplicationUser>();

            Mock<IUserService> userService = mockRepository.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(appUser.Object);

            Mock<ISecurityService> securityService = mockRepository.Create<ISecurityService>();
            UserSecurity userSecurity = new UserSecurity() { CanCreateDataFlow = true };
            securityService.Setup(x => x.GetUserSecurity(null, appUser.Object)).Returns(userSecurity);
            UserSecurity userSecurity2 = new UserSecurity() { CanManageSchema = false, CanEditDataset = false };
            securityService.Setup(x => x.GetUserSecurity(dataset, appUser.Object)).Returns(userSecurity2);

            DataFlowService service = new DataFlowService(datasetContext.Object, userService.Object, null, securityService.Object, null, null, null, null, null);

            DataFlowDto dto = new DataFlowDto()
            {
                SchemaMap = new List<SchemaMapDto>()
                {
                    new SchemaMapDto() { DatasetId = 1 }
                }
            };

            Assert.ThrowsException<DatasetUnauthorizedAccessException>(() => service.CreateDataFlow(dto), "No permissions to push data to Dataset Name");

            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void CreateDataFlow_FailSchemaInUse_SchemaInUseException()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mockRepository.Create<IDatasetContext>();
            Dataset dataset = new Dataset() { DatasetName = "Dataset Name" };
            datasetContext.Setup(x => x.GetById<Dataset>(1)).Returns(dataset);
            DataFlow dataFlow = new DataFlow() { DatasetId = 1, SchemaId = 2, ObjectStatus = GlobalEnums.ObjectStatusEnum.Active };
            datasetContext.SetupGet(x => x.DataFlow).Returns(new List<DataFlow>() { dataFlow }.AsQueryable());

            Mock<IApplicationUser> appUser = mockRepository.Create<IApplicationUser>();

            Mock<IUserService> userService = mockRepository.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(appUser.Object);

            Mock<ISecurityService> securityService = mockRepository.Create<ISecurityService>();
            UserSecurity userSecurity = new UserSecurity() { CanCreateDataFlow = true };
            securityService.Setup(x => x.GetUserSecurity(null, appUser.Object)).Returns(userSecurity);
            UserSecurity userSecurity2 = new UserSecurity() { CanManageSchema = true, CanEditDataset = true };
            securityService.Setup(x => x.GetUserSecurity(dataset, appUser.Object)).Returns(userSecurity2);

            DataFlowService service = new DataFlowService(datasetContext.Object, userService.Object, null, securityService.Object, null, null, null, null, null);

            DataFlowDto dto = new DataFlowDto()
            {
                SchemaMap = new List<SchemaMapDto>()
                {
                    new SchemaMapDto() { DatasetId = 1, SchemaId = 2 }
                }
            };

            Assert.ThrowsException<SchemaInUseException>(() => service.CreateDataFlow(dto), "Schema ID 2 is already associated to another DataFlow.");

            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void CreateDataFlow_DFSDrop_NamedEnvironmentTypeFeatureOff_Id()
        {
            int result = RunCreateDataFlowTestForDfsDrop(new DfsDataFlowBasic(), NamedEnvironmentType.NonProd, "NonProd");
            Assert.AreEqual(3, result);
        }

        [TestMethod]
        public void CreateDataFlow_DFSDrop_NamedEnvironmentTypeFeatureOn_NonProd_Id()
        {
            int result = RunCreateDataFlowTestForDfsDrop(new DfsNonProdSource(), NamedEnvironmentType.NonProd, "");
            Assert.AreEqual(3, result);
        }

        [TestMethod]
        public void CreateDataFlow_DFSDrop_NamedEnvironmentTypeFeatureOn_Prod_Id()
        {
            int result = RunCreateDataFlowTestForDfsDrop(new DfsProdSource(), NamedEnvironmentType.Prod, "");
            Assert.AreEqual(3, result);
        }

        [TestMethod]
        public void CreateDataFlow_S3Drop_Id()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Dataset dataset = new Dataset()
            {
                DatasetId = 1,
                DatasetName = "Dataset Name",
                DatasetCategories = new List<Category>(),
                Asset = new Asset() { SaidKeyCode = "SAID" },
                NamedEnvironment = "DEV",
                NamedEnvironmentType = NamedEnvironmentType.NonProd
            };

            Mock<IApplicationUser> appUser = GetApplicationUser(mockRepository);
            Mock<IUserService> userService = GetUserService(mockRepository, appUser.Object);
            Mock<ISecurityService> securityService = GetSecurityService(mockRepository, dataset, appUser.Object);
            securityService.Setup(x => x.EnqueueCreateDefaultSecurityForDataFlow(3));

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mockRepository, dataset);

            Mock<IDataFeatures> dataFeatures = mockRepository.Create<IDataFeatures>();
            dataFeatures.Setup(x => x.CLA3241_DisableDfsDropLocation.GetValue()).Returns(false);
            dataFeatures.Setup(x => x.CLA4433_SEND_S3_SINK_CONNECTOR_REQUEST_EMAIL.GetValue()).Returns(false);
            dataFeatures.Setup(x => x.CLA3718_Authorization.GetValue()).Returns(true);

            DataFlowService service = new DataFlowService(datasetContext.Object, userService.Object, null, securityService.Object, null, dataFeatures.Object, null, null, null);

            DataFlowDto dto = new DataFlowDto()
            {
                SchemaMap = new List<SchemaMapDto>()
                {
                    new SchemaMapDto() { DatasetId = 1, SchemaId = 2 }
                },
                IngestionType = 3,
                PreProcessingOption = 0,
                DatasetId = 1,
                SaidKeyCode = "SAID",
                NamedEnvironment = "DEV",
                NamedEnvironmentType = NamedEnvironmentType.NonProd
            };

            int result = service.CreateDataFlow(dto);

            Assert.AreEqual(3, result);

            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void CreateDataFlow_Topic_Id()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Dataset dataset = new Dataset()
            {
                DatasetId = 1,
                DatasetName = "Dataset Name",
                DatasetCategories = new List<Category>(),
                Asset = new Asset() { SaidKeyCode = "SAID" },
                NamedEnvironment = "DEV",
                NamedEnvironmentType = NamedEnvironmentType.NonProd
            };

            Mock<IApplicationUser> appUser = GetApplicationUser(mockRepository);
            Mock<IUserService> userService = GetUserService(mockRepository, appUser.Object);
            Mock<ISecurityService> securityService = GetSecurityService(mockRepository, dataset, appUser.Object);
            securityService.Setup(x => x.EnqueueCreateDefaultSecurityForDataFlow(3));

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mockRepository, dataset);

            Mock<IDataFeatures> dataFeatures = mockRepository.Create<IDataFeatures>();
            dataFeatures.Setup(x => x.CLA3241_DisableDfsDropLocation.GetValue()).Returns(false);
            dataFeatures.Setup(x => x.CLA4433_SEND_S3_SINK_CONNECTOR_REQUEST_EMAIL.GetValue()).Returns(true);
            dataFeatures.Setup(x => x.CLA3718_Authorization.GetValue()).Returns(true);

            Mock<IKafkaConnectorService> connectorService = mockRepository.Create<IKafkaConnectorService>();
            ConnectorCreateResponseDto connectorCreateResponseDto = new ConnectorCreateResponseDto();
            connectorService.Setup(x => x.CreateS3SinkConnectorAsync(It.IsAny<ConnectorCreateRequestDto>())).ReturnsAsync(connectorCreateResponseDto);

            Mock<IEmailService> emailService = mockRepository.Create<IEmailService>();
            emailService.Setup(x => x.SendS3SinkConnectorRequestEmail(It.IsAny<DataFlow>(), It.IsAny<ConnectorCreateRequestDto>(), connectorCreateResponseDto));

            DataFlowService service = new DataFlowService(datasetContext.Object, userService.Object, null, securityService.Object, null, dataFeatures.Object, null, emailService.Object, connectorService.Object);

            DataFlowDto dto = new DataFlowDto()
            {
                SchemaMap = new List<SchemaMapDto>()
                {
                    new SchemaMapDto() { DatasetId = 1, SchemaId = 2 }
                },
                IngestionType = 4,
                PreProcessingOption = 0,
                DatasetId = 1,
                SaidKeyCode = "SAID",
                NamedEnvironment = "DEV",
                NamedEnvironmentType = NamedEnvironmentType.NonProd,
                TopicName = "TopicName"
            };

            int result = service.CreateDataFlow(dto);

            Assert.AreEqual(3, result);

            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void CreateDataFlow_DSCPull_Id()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Dataset dataset = new Dataset()
            {
                DatasetId = 1,
                DatasetName = "Dataset Name",
                DatasetCategories = new List<Category>(),
                Asset = new Asset() { SaidKeyCode = "SAID" },
                NamedEnvironment = "DEV",
                NamedEnvironmentType = NamedEnvironmentType.NonProd
            };

            Mock<IApplicationUser> appUser = GetApplicationUser(mockRepository);
            Mock<IUserService> userService = GetUserService(mockRepository, appUser.Object);
            Mock<ISecurityService> securityService = GetSecurityService(mockRepository, dataset, appUser.Object);
            securityService.Setup(x => x.EnqueueCreateDefaultSecurityForDataFlow(3));

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mockRepository, dataset);

            Mock<IDataFeatures> dataFeatures = mockRepository.Create<IDataFeatures>();
            dataFeatures.Setup(x => x.CLA4433_SEND_S3_SINK_CONNECTOR_REQUEST_EMAIL.GetValue()).Returns(true);
            dataFeatures.Setup(x => x.CLA3718_Authorization.GetValue()).Returns(true);

            Mock<IJobService> jobService = mockRepository.Create<IJobService>();
            jobService.Setup(x => x.CreateAndSaveRetrieverJob(It.Is<RetrieverJobDto>(j => j.DataFlow == 3 && j.FileSchema == 2))).Returns(new RetrieverJob());

            DataFlowService service = new DataFlowService(datasetContext.Object, userService.Object, jobService.Object, securityService.Object, null, dataFeatures.Object, null, null, null);

            DataFlowDto dto = new DataFlowDto()
            {
                SchemaMap = new List<SchemaMapDto>()
                {
                    new SchemaMapDto() { DatasetId = 1, SchemaId = 2 }
                },
                IngestionType = 2,
                PreProcessingOption = 0,
                DatasetId = 1,
                SaidKeyCode = "SAID",
                NamedEnvironment = "DEV",
                NamedEnvironmentType = NamedEnvironmentType.NonProd,
                RetrieverJob = new RetrieverJobDto()
            };

            int result = service.CreateDataFlow(dto);

            Assert.AreEqual(3, result);

            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void CreateDataFlow_ValidationException_ClearContext()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Dataset dataset = new Dataset()
            {
                DatasetId = 1,
                DatasetName = "Dataset Name",
                DatasetCategories = new List<Category>(),
                Asset = new Asset() { SaidKeyCode = "SAID" },
                NamedEnvironment = "DEV",
                NamedEnvironmentType = NamedEnvironmentType.NonProd
            };

            Mock<IApplicationUser> appUser = GetApplicationUser(mockRepository);
            Mock<IUserService> userService = GetUserService(mockRepository, appUser.Object);
            Mock<ISecurityService> securityService = GetSecurityService(mockRepository, dataset, appUser.Object);
            Mock<IDatasetContext> datasetContext = mockRepository.Create<IDatasetContext>();

            datasetContext.Setup(x => x.GetById<Dataset>(1)).Returns(dataset);

            DataFlow dataFlow = new DataFlow() { Id = 3, DatasetId = 1 };
            datasetContext.SetupGet(x => x.DataFlow).Returns(new List<DataFlow>() { dataFlow }.AsQueryable());

            FileSchema fileSchema = new FileSchema()
            {
                SchemaId = 2,
                StorageCode = "000001",
                Extension = new FileExtension() { Name = GlobalConstants.ExtensionNames.JSON }
            };
            datasetContext.SetupGet(x => x.FileSchema).Returns(new List<FileSchema>() { fileSchema }.AsQueryable());

            datasetContext.Setup(x => x.Add(It.IsAny<DataFlow>())).Callback<DataFlow>(x => x.Id = 3);

            datasetContext.Setup(x => x.Clear());

            Mock<IJobService> jobService = mockRepository.Create<IJobService>();
            jobService.Setup(x => x.CreateAndSaveRetrieverJob(It.Is<RetrieverJobDto>(j => j.DataFlow == 3 && j.FileSchema == 2))).Throws(new ValidationException("invalid"));

            DataFlowService service = new DataFlowService(datasetContext.Object, userService.Object, jobService.Object, securityService.Object, null, null, null, null, null);

            DataFlowDto dto = new DataFlowDto()
            {
                SchemaMap = new List<SchemaMapDto>()
                {
                    new SchemaMapDto() { DatasetId = 1, SchemaId = 2 }
                },
                IngestionType = 2,
                PreProcessingOption = 0,
                DatasetId = 1,
                SaidKeyCode = "SAID",
                NamedEnvironment = "DEV",
                NamedEnvironmentType = NamedEnvironmentType.NonProd,
                RetrieverJob = new RetrieverJobDto()
            };

            Assert.ThrowsException<ValidationException>(() => service.CreateDataFlow(dto));

            mockRepository.VerifyAll();
        }

        #region Helpers
        private Mock<IDatasetContext> GetDatasetContext(MockRepository mockRepository, Dataset dataset)
        {
            Mock<IDatasetContext> datasetContext = mockRepository.Create<IDatasetContext>();

            datasetContext.Setup(x => x.GetById<Dataset>(1)).Returns(dataset);
            datasetContext.SetupGet(x => x.Datasets).Returns(new List<Dataset>() { dataset }.AsQueryable());

            DataFlow dataFlow = new DataFlow() { Id = 3, DatasetId = 1 };
            datasetContext.SetupGet(x => x.DataFlow).Returns(new List<DataFlow>() { dataFlow }.AsQueryable());

            FileSchema fileSchema = new FileSchema()
            {
                SchemaId = 2,
                StorageCode = "000001",
                Extension = new FileExtension() { Name = GlobalConstants.ExtensionNames.JSON }
            };
            datasetContext.SetupGet(x => x.FileSchema).Returns(new List<FileSchema>() { fileSchema }.AsQueryable());
            datasetContext.Setup(x => x.GetById<FileSchema>(2)).Returns(fileSchema);

            datasetContext.Setup(x => x.Add(It.IsAny<DataFlow>())).Callback<DataFlow>(x => x.Id = 3);

            datasetContext.Setup(x => x.SaveChanges(true));

            datasetContext.SetupGet(x => x.ProducerS3DropAction).Returns(new List<ProducerS3DropAction>() { new ProducerS3DropAction() { Id = 15 } }.AsQueryable());
            datasetContext.SetupGet(x => x.RawStorageAction).Returns(new List<RawStorageAction>() { new RawStorageAction() { Id = 22 } }.AsQueryable());
            datasetContext.SetupGet(x => x.SchemaLoadAction).Returns(new List<SchemaLoadAction>() { new SchemaLoadAction() { Id = 32 } }.AsQueryable());
            datasetContext.SetupGet(x => x.QueryStorageAction).Returns(new List<QueryStorageAction>() { new QueryStorageAction() { Id = 23 } }.AsQueryable());
            datasetContext.SetupGet(x => x.ConvertToParquetAction).Returns(new List<ConvertToParquetAction>() { new ConvertToParquetAction() { Id = 24 } }.AsQueryable());

            datasetContext.Setup(x => x.Add(It.IsAny<DataFlowStep>()));

            DatasetFileConfig datasetFileConfig = new DatasetFileConfig()
            {
                ParentDataset = dataset,
                Schema = fileSchema
            };
            datasetContext.SetupGet(x => x.DatasetFileConfigs).Returns(new List<DatasetFileConfig>() { datasetFileConfig }.AsQueryable());

            datasetContext.Setup(x => x.Add(It.IsAny<SchemaMap>()));

            DataFlowStep dataFlowStep = new DataFlowStep()
            {
                DataFlow = dataFlow,
                DataAction_Type_Id = DataActionType.QueryStorage
            };
            List<SchemaMap> schemaMaps = new List<SchemaMap>() { new SchemaMap() { MappedSchema = fileSchema } };
            DataFlowStep dataFlowStep2 = new DataFlowStep()
            {
                DataFlow = dataFlow,
                DataAction_Type_Id = DataActionType.SchemaLoad,
                SchemaMappings = schemaMaps
            };
            DataFlowStep dataFlowStep3 = new DataFlowStep()
            {
                DataFlow = dataFlow,
                DataAction_Type_Id = DataActionType.ConvertParquet,
                SchemaMappings = schemaMaps
            };
            datasetContext.SetupGet(x => x.DataFlowStep).Returns(new List<DataFlowStep>() { dataFlowStep, dataFlowStep2, dataFlowStep3 }.AsQueryable());

            return datasetContext;
        }

        private Mock<IApplicationUser> GetApplicationUser(MockRepository mockRepository)
        {
            Mock<IApplicationUser> appUser = mockRepository.Create<IApplicationUser>();
            appUser.SetupGet(x => x.AssociateId).Returns("000000");

            return appUser;
        }

        private Mock<IUserService> GetUserService(MockRepository mockRepository, IApplicationUser appUser)
        {
            Mock<IUserService> userService = mockRepository.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(appUser);

            return userService;
        }

        private Mock<ISecurityService> GetSecurityService(MockRepository mockRepository, Dataset dataset, IApplicationUser appUser)
        {
            Mock<ISecurityService> securityService = mockRepository.Create<ISecurityService>();

            UserSecurity userSecurity = new UserSecurity() { CanCreateDataFlow = true };
            securityService.Setup(x => x.GetUserSecurity(null, appUser)).Returns(userSecurity);

            UserSecurity userSecurity2 = new UserSecurity() { CanManageSchema = true, CanEditDataset = true };
            securityService.Setup(x => x.GetUserSecurity(dataset, appUser)).Returns(userSecurity2);

            return securityService;
        }

        private int RunCreateDataFlowTestForDfsDrop(DataSource dataSource, NamedEnvironmentType namedEnvironmentType, string CLA4260_QuartermasterNamedEnvironmentTypeFilter)
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Strict);

            Dataset dataset = new Dataset()
            {
                DatasetId = 1,
                DatasetName = "Dataset Name",
                DatasetCategories = new List<Category>(),
                Asset = new Asset() { SaidKeyCode = "SAID" },
                NamedEnvironment = "DEV",
                NamedEnvironmentType = namedEnvironmentType
            };

            Mock<IApplicationUser> appUser = GetApplicationUser(mockRepository);
            Mock<IUserService> userService = GetUserService(mockRepository, appUser.Object);
            Mock<ISecurityService> securityService = GetSecurityService(mockRepository, dataset, appUser.Object);
            securityService.Setup(x => x.EnqueueCreateDefaultSecurityForDataFlow(3));

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mockRepository, dataset);

            datasetContext.SetupGet(x => x.DataSources).Returns(new List<DataSource>() { dataSource }.AsQueryable());

            Mock<IJobService> jobService = mockRepository.Create<IJobService>();
            RetrieverJob retrieverJob = new RetrieverJob();
            jobService.Setup(x => x.InstantiateJobsForCreation(It.Is<DataFlow>(d => d.Id == 3), dataSource)).Returns(retrieverJob);
            jobService.Setup(x => x.CreateDropLocation(retrieverJob));

            datasetContext.Setup(x => x.Add(retrieverJob));

            Mock<IDataFeatures> dataFeatures = mockRepository.Create<IDataFeatures>();
            dataFeatures.Setup(x => x.CLA3241_DisableDfsDropLocation.GetValue()).Returns(false);
            dataFeatures.Setup(x => x.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()).Returns(CLA4260_QuartermasterNamedEnvironmentTypeFilter);
            dataFeatures.Setup(x => x.CLA4433_SEND_S3_SINK_CONNECTOR_REQUEST_EMAIL.GetValue()).Returns(false);
            dataFeatures.Setup(x => x.CLA3718_Authorization.GetValue()).Returns(true);

            DataFlowService service = new DataFlowService(datasetContext.Object, userService.Object, jobService.Object, securityService.Object, null, dataFeatures.Object, null, null, null);

            DataFlowDto dto = new DataFlowDto()
            {
                SchemaMap = new List<SchemaMapDto>()
                {
                    new SchemaMapDto() { DatasetId = 1, SchemaId = 2 }
                },
                IngestionType = 1,
                PreProcessingOption = 0,
                DatasetId = 1,
                SaidKeyCode = "SAID",
                NamedEnvironment = "DEV",
                NamedEnvironmentType = namedEnvironmentType
            };

            int result = service.CreateDataFlow(dto);

            mockRepository.VerifyAll();

            return result;
        }
        #endregion
    }
}
