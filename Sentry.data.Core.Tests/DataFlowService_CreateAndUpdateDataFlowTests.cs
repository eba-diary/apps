using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nest;
using Sentry.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class DataFlowService_CreateAndUpdateDataFlowTests
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
            dataFeatures.Setup(x => x.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()).Returns(string.Empty);

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
        public void CreateDataFlow_S3Drop_ParquetSchema_Id()
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

            Mock<IDatasetContext> datasetContext = mockRepository.Create<IDatasetContext>();

            datasetContext.Setup(x => x.GetById<Dataset>(1)).Returns(dataset);
            datasetContext.SetupGet(x => x.Datasets).Returns(new List<Dataset>() { dataset }.AsQueryable());

            DataFlow dataFlow = new DataFlow() { Id = 3, DatasetId = 1 };
            datasetContext.SetupGet(x => x.DataFlow).Returns(new List<DataFlow>() { dataFlow }.AsQueryable());

            FileSchema fileSchema = new FileSchema()
            {
                SchemaId = 2,
                StorageCode = "000001",
                Extension = new FileExtension() { Name = GlobalConstants.ExtensionNames.PARQUET }
            };
            datasetContext.SetupGet(x => x.FileSchema).Returns(new List<FileSchema>() { fileSchema }.AsQueryable());
            datasetContext.Setup(x => x.GetById<FileSchema>(2)).Returns(fileSchema);

            DataFlow resultDataFlow = null;
            datasetContext.Setup(x => x.Add(It.IsAny<DataFlow>())).Callback<DataFlow>(x => 
            {
                x.Id = 3;
                resultDataFlow = x;
            });

            datasetContext.Setup(x => x.SaveChanges(true));

            datasetContext.SetupGet(x => x.ProducerS3DropAction).Returns(new List<ProducerS3DropAction>() { new ProducerS3DropAction() { Id = 15 } }.AsQueryable());
            datasetContext.SetupGet(x => x.RawStorageAction).Returns(new List<RawStorageAction>() { new RawStorageAction() { Id = 22 } }.AsQueryable());
            datasetContext.SetupGet(x => x.SchemaLoadAction).Returns(new List<SchemaLoadAction>() { new SchemaLoadAction() { Id = 32 } }.AsQueryable());
            datasetContext.SetupGet(x => x.QueryStorageAction).Returns(new List<QueryStorageAction>() { new QueryStorageAction() { Id = 23 } }.AsQueryable());
            datasetContext.SetupGet(x => x.CopyToParquetAction).Returns(new List<CopyToParquetAction>() { new CopyToParquetAction() { Id = 56, TriggerPrefix = "copytoparquet/", TargetStoragePrefix = "parquet/" } }.AsQueryable());

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
                DataAction_Type_Id = DataActionType.CopyToParquet,
                SchemaMappings = schemaMaps                
            };
            datasetContext.SetupGet(x => x.DataFlowStep).Returns(new List<DataFlowStep>() { dataFlowStep, dataFlowStep2, dataFlowStep3 }.AsQueryable());

            Mock<IDataFeatures> dataFeatures = mockRepository.Create<IDataFeatures>();
            dataFeatures.Setup(x => x.CLA3241_DisableDfsDropLocation.GetValue()).Returns(false);
            dataFeatures.Setup(x => x.CLA4433_SEND_S3_SINK_CONNECTOR_REQUEST_EMAIL.GetValue()).Returns(false);
            dataFeatures.Setup(x => x.CLA3718_Authorization.GetValue()).Returns(true);
            dataFeatures.Setup(x => x.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()).Returns(string.Empty);

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
            Assert.AreEqual(5, resultDataFlow.Steps.Count);

            DataFlowStep copyToParquet = resultDataFlow.Steps.Last();
            Assert.AreEqual(DataActionType.CopyToParquet, copyToParquet.DataAction_Type_Id);
            Assert.AreEqual("temp-file/copytoparquet/SAID/DEV/000001/", copyToParquet.TriggerKey);
            Assert.AreEqual("parquet/SAID/DEV/000001/", copyToParquet.TargetPrefix);

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
            dataFeatures.Setup(x => x.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()).Returns(string.Empty);

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
        public void EditDataFlow_EnsureS3SinkConnectorNotCalled()
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

            //AUSTIN
            Mock<IApplicationUser> appUser = GetApplicationUser(mockRepository);
            Mock<IUserService> userService = GetUserService(mockRepository, appUser.Object);
            Mock<ISecurityService> securityService = GetSecurityService(mockRepository, dataset, appUser.Object);
            securityService.Setup(x => x.EnqueueCreateDefaultSecurityForDataFlow(3));

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mockRepository, dataset);

            //Create an extra DataFlow in the datasetContext which is a "deleted" version of the dataflow with Fred as topic name, basically this simulates that someone
            //created a dataflow with a topicname=Fred and a s3SinkConnector was created for Fred Topic already, therefore if they create another dataflow with Fred, we will NOT want to recreated S3SinkConnector
            DataFlow dataFlowDuplicate = new DataFlow() { DatasetId = 1, SchemaId = 2, ObjectStatus = GlobalEnums.ObjectStatusEnum.Deleted,TopicName="Fred"};
            datasetContext.SetupGet(x => x.DataFlow).Returns(new List<DataFlow>() { dataFlowDuplicate }.AsQueryable());

            Mock<IDataFeatures> dataFeatures = mockRepository.Create<IDataFeatures>();
            dataFeatures.Setup(x => x.CLA3241_DisableDfsDropLocation.GetValue()).Returns(false);
            dataFeatures.Setup(x => x.CLA4433_SEND_S3_SINK_CONNECTOR_REQUEST_EMAIL.GetValue()).Returns(true);
            dataFeatures.Setup(x => x.CLA3718_Authorization.GetValue()).Returns(true);
            dataFeatures.Setup(x => x.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()).Returns("NonProd");

            DataFlowService service = new DataFlowService(datasetContext.Object, userService.Object, null, securityService.Object, null, dataFeatures.Object, null, null, null);

            //create a dataflow with a topicname=Fred which should NOT create a S3SinkConnector if code is working properly
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
                TopicName = "Fred"
            };

            int result = service.CreateDataFlow(dto);

            Assert.AreEqual(3, result);

            //Since we DIDN'T mock up CreateS3SinkConnectorAsync, essentially what this is doing is verifying ALL endpoints executed were mocked up
            //therefore since we DID NOT mock up CreateS3SinkConnectorAsync this ensures that it was NOT executed in this unit test
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
            dataFeatures.Setup(x => x.CLA3241_DisableDfsDropLocation.GetValue()).Returns(false);
            dataFeatures.Setup(x => x.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()).Returns(string.Empty);

            Mock<IJobService> jobService = mockRepository.Create<IJobService>();
            jobService.Setup(x => x.CreateRetrieverJob(It.Is<RetrieverJobDto>(j => j.DataFlow == 3 && j.FileSchema == 2))).Returns(new RetrieverJob());

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
            jobService.Setup(x => x.CreateRetrieverJob(It.Is<RetrieverJobDto>(j => j.DataFlow == 3 && j.FileSchema == 2))).Throws(new ValidationException("invalid"));

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

        [TestMethod]
        public void UpdateDataFlow_S3Drop_Id()
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

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mockRepository, dataset);

            DataFlow dataFlow = new DataFlow()
            {
                Id = 2,
                ObjectStatus = ObjectStatusEnum.Active,
                DeleteIssueDTM = new DateTime()
            };
            datasetContext.Setup(x => x.GetById<DataFlow>(2)).Returns(dataFlow);

            RetrieverJob retrieverJob = new RetrieverJob()
            {
                Id = 4,
                DataFlow = dataFlow
            };
            datasetContext.SetupGet(x => x.RetrieverJob).Returns(new List<RetrieverJob>() { retrieverJob }.AsQueryable());

            Mock<IJobService> jobService = mockRepository.Create<IJobService>();
            jobService.Setup(x => x.Delete(It.Is<List<int>>(i => i.Contains(4)), appUser.Object, false)).Returns(true);

            Mock<IDataFeatures> dataFeatures = mockRepository.Create<IDataFeatures>();
            dataFeatures.Setup(x => x.CLA3241_DisableDfsDropLocation.GetValue()).Returns(false);
            dataFeatures.Setup(x => x.CLA4433_SEND_S3_SINK_CONNECTOR_REQUEST_EMAIL.GetValue()).Returns(false);
            dataFeatures.Setup(x => x.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()).Returns(string.Empty);

            DataFlowService service = new DataFlowService(datasetContext.Object, userService.Object, jobService.Object, null, null, dataFeatures.Object, null, null, null);

            DataFlowDto dto = new DataFlowDto()
            {
                Id = 2,
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

            int result = service.UpdateDataFlow(dto);

            Assert.AreEqual(ObjectStatusEnum.Deleted, dataFlow.ObjectStatus);
            Assert.AreEqual(3, result);

            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void AddDataFlowAsync_DataFlowDto_ResultDataFlowDto()
        {
            DateTime start = DateTime.Now;

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Dataset dataset = new Dataset()
            {
                DatasetId = 1,
                DatasetName = "Dataset Name",
                DatasetCategories = new List<Category>(),
                Asset = new Asset() { SaidKeyCode = "SAID" },
                NamedEnvironment = "DEV",
                NamedEnvironmentType = NamedEnvironmentType.NonProd
            };

            Mock<IApplicationUser> appUser = GetApplicationUser(mr);
            Mock<IUserService> userService = GetUserService(mr, appUser.Object);
            
            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();

            UserSecurity userSecurity = new UserSecurity();
            securityService.Setup(x => x.GetUserSecurity(It.IsAny<DataFlow>(), appUser.Object)).Returns(userSecurity);

            Mock<IDataFeatures> dataFeatures = mr.Create<IDataFeatures>();
            dataFeatures.Setup(x => x.CLA3241_DisableDfsDropLocation.GetValue()).Returns(false);
            dataFeatures.Setup(x => x.CLA4433_SEND_S3_SINK_CONNECTOR_REQUEST_EMAIL.GetValue()).Returns(true);
            dataFeatures.Setup(x => x.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()).Returns(string.Empty);

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr, dataset);
            datasetContext.SetupGet(x => x.RetrieverJob).Returns(new List<RetrieverJob>().AsQueryable());

            DataFlowService dataFlowService = new DataFlowService(datasetContext.Object, userService.Object, null, securityService.Object, null, dataFeatures.Object, null, null, null);

            DataFlowDto dto = new DataFlowDto
            {
                Name = "Name",
                SaidKeyCode = "SAID",
                ObjectStatus = ObjectStatusEnum.Active,
                IngestionType = (int)IngestionType.S3_Drop,
                NamedEnvironment = "DEV",
                NamedEnvironmentType = NamedEnvironmentType.NonProd,
                PrimaryContactId = "000000",
                IsSecured = true,
                PreProcessingOption = 0,
                DatasetId = 1,
                SchemaMap = new List<SchemaMapDto>
                {
                    new SchemaMapDto { DatasetId = 1, SchemaId = 2}
                }
            };

            DataFlowDto result = dataFlowService.AddDataFlowAsync(dto).Result;

            Assert.AreEqual(3, result.Id);
            Assert.AreEqual("Name", result.Name);
            Assert.AreEqual("SAID", result.SaidKeyCode);
            Assert.IsTrue(result.CreateDTM >= start);
            Assert.AreEqual("000000", result.CreatedBy);
            Assert.AreEqual("000001", result.FlowStorageCode);
            Assert.AreEqual(2, result.MappedSchema.First());
            Assert.AreEqual(ObjectStatusEnum.Active, result.ObjectStatus);
            Assert.IsNull(result.DeleteIssuer);
            Assert.AreEqual(DateTime.MaxValue, result.DeleteIssueDTM);
            Assert.AreEqual((int)IngestionType.S3_Drop, result.IngestionType);
            Assert.IsFalse(result.IsCompressed);
            Assert.IsFalse(result.IsBackFillRequired);
            Assert.IsNull(result.CompressionType);
            Assert.IsFalse(result.IsPreProcessingRequired);
            Assert.AreEqual(0, result.PreProcessingOption);
            Assert.AreEqual("DEV", result.NamedEnvironment);
            Assert.AreEqual(NamedEnvironmentType.NonProd, result.NamedEnvironmentType);
            Assert.AreEqual("000000", result.PrimaryContactId);
            Assert.IsTrue(result.IsSecured);
            Assert.AreEqual(userSecurity, result.Security);
            Assert.IsNull(result.TopicName);
            Assert.IsNull(result.S3ConnectorName);
            Assert.AreEqual(1, result.SchemaMap.First().DatasetId);
            Assert.AreEqual(2, result.SchemaMap.First().SchemaId);
            Assert.AreEqual(1, result.DatasetId);
            Assert.AreEqual(2, result.SchemaId);

            mr.VerifyAll();
        }

        [TestMethod]
        public void UpdateDataFlowAsync_HasUpdates_DataFlowDto()
        {
            DateTime start = DateTime.Now;

            DataFlowDto dto = new DataFlowDto
            {
                IngestionType = (int)IngestionType.S3_Drop,
                PrimaryContactId = "100000",
                IsPreProcessingRequired = true,
                PreProcessingOption = (int)DataFlowPreProcessingTypes.googleapi,
                IsCompressed = true,
                CompressionType = (int)CompressionTypes.ZIP,
                CompressionJob = new CompressionJobDto { CompressionType = CompressionTypes.ZIP },
            };

            DataFlow dataFlow = new DataFlow
            {
                Id = 2,
                SaidKeyCode = "SAID",
                NamedEnvironment = "DEV",
                NamedEnvironmentType = NamedEnvironmentType.NonProd,
                ObjectStatus = ObjectStatusEnum.Active,
                DatasetId = 1,
                Name = "Name",
                IsSecured = true,
                SchemaId = 2,
                DeleteIssueDTM = DateTime.MaxValue,
                IngestionType = (int)IngestionType.Topic,
                TopicName = "TopicName",
                PrimaryContactId = "000000",
            };

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IApplicationUser> appUser = GetApplicationUser(mr);
            Mock<IUserService> userService = GetUserService(mr, appUser.Object);

            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();

            UserSecurity userSecurity = new UserSecurity();
            securityService.Setup(x => x.GetUserSecurity(It.IsAny<DataFlow>(), appUser.Object)).Returns(userSecurity);

            Mock<IDataFeatures> dataFeatures = mr.Create<IDataFeatures>();
            dataFeatures.Setup(x => x.CLA3241_DisableDfsDropLocation.GetValue()).Returns(false);
            dataFeatures.Setup(x => x.CLA4433_SEND_S3_SINK_CONNECTOR_REQUEST_EMAIL.GetValue()).Returns(true);
            dataFeatures.Setup(x => x.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()).Returns(string.Empty);

            Mock<IJobService> jobService = mr.Create<IJobService>();
            jobService.Setup(x => x.Delete(It.Is<List<int>>(l => !l.Any()), appUser.Object, false)).Returns(true);

            Dataset dataset = new Dataset()
            {
                DatasetId = 1,
                DatasetName = "Dataset Name",
                DatasetCategories = new List<Category>(),
                Asset = new Asset() { SaidKeyCode = "SAID" },
                NamedEnvironment = "DEV",
                NamedEnvironmentType = NamedEnvironmentType.NonProd
            };

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr, dataset);
            datasetContext.Setup(x => x.GetById<DataFlow>(2)).Returns(dataFlow);
            datasetContext.SetupGet(x => x.RetrieverJob).Returns(new List<RetrieverJob>().AsQueryable());
            datasetContext.SetupGet(x => x.UncompressZipAction).Returns(new List<UncompressZipAction>() { new UncompressZipAction() { Id = 25 } }.AsQueryable());
            datasetContext.SetupGet(x => x.GoogleApiAction).Returns(new List<GoogleApiAction>() { new GoogleApiAction() { Id = 26 } }.AsQueryable());

            DataFlowService dataFlowService = new DataFlowService(datasetContext.Object, userService.Object, jobService.Object, securityService.Object, null, dataFeatures.Object, null, null, null);
            DataFlowDto result = dataFlowService.UpdateDataFlowAsync(dto, dataFlow).Result;

            //deleted old dataflow
            Assert.AreEqual(ObjectStatusEnum.Deleted, dataFlow.ObjectStatus);
            Assert.AreEqual("000000", dataFlow.DeleteIssuer);
            Assert.IsTrue(dataFlow.DeleteIssueDTM >= start);

            //created new dataflow
            Assert.AreEqual(3, result.Id);
            Assert.AreEqual("Name", result.Name);
            Assert.AreEqual("SAID", result.SaidKeyCode);
            Assert.IsTrue(result.CreateDTM >= start);
            Assert.AreEqual("000000", result.CreatedBy);
            Assert.AreEqual("000001", result.FlowStorageCode);
            Assert.AreEqual(2, result.MappedSchema.First());
            Assert.AreEqual(ObjectStatusEnum.Active, result.ObjectStatus);
            Assert.IsNull(result.DeleteIssuer);
            Assert.AreEqual(DateTime.MaxValue, result.DeleteIssueDTM);
            Assert.AreEqual((int)IngestionType.S3_Drop, result.IngestionType);
            Assert.IsTrue(result.IsCompressed);
            Assert.IsFalse(result.IsBackFillRequired);
            Assert.AreEqual((int)CompressionTypes.ZIP, result.CompressionType);
            Assert.IsTrue(result.IsPreProcessingRequired);
            Assert.AreEqual((int)DataFlowPreProcessingTypes.googleapi, result.PreProcessingOption);
            Assert.AreEqual("DEV", result.NamedEnvironment);
            Assert.AreEqual(NamedEnvironmentType.NonProd, result.NamedEnvironmentType);
            Assert.AreEqual("100000", result.PrimaryContactId);
            Assert.IsTrue(result.IsSecured);
            Assert.AreEqual(userSecurity, result.Security);
            Assert.IsNull(result.TopicName);
            Assert.IsNull(result.S3ConnectorName);
            Assert.AreEqual(1, result.SchemaMap.First().DatasetId);
            Assert.AreEqual(2, result.SchemaMap.First().SchemaId);
            Assert.AreEqual(1, result.DatasetId);
            Assert.AreEqual(2, result.SchemaId);
        }

        [TestMethod]
        public void UpdateDataFlowAsync_NoUpdates_DataFlowDto()
        {
            DataFlowDto dto = new DataFlowDto();

            DataFlow dataFlow = new DataFlow
            {
                Id = 3,
                SaidKeyCode = "SAID",
                NamedEnvironment = "DEV",
                NamedEnvironmentType = NamedEnvironmentType.NonProd,
                ObjectStatus = ObjectStatusEnum.Active,
                DatasetId = 1,
                Name = "Name",
                IsSecured = true,
                SchemaId = 2,
                DeleteIssueDTM = DateTime.MaxValue,
                IngestionType = (int)IngestionType.S3_Drop,
                PrimaryContactId = "000000",
                CreatedBy = "000000",
                CreatedDTM = new DateTime(2023, 1, 1),
                FlowStorageCode = "1234567",
                Steps = new List<DataFlowStep>(),
                PreProcessingOption = 0
            };

            MockRepository mr = new MockRepository(MockBehavior.Strict);
            
            Mock<IApplicationUser> appUser = GetApplicationUser(mr);
            Mock<IUserService> userService = GetUserService(mr, appUser.Object);
            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();

            UserSecurity userSecurity = new UserSecurity();
            securityService.Setup(x => x.GetUserSecurity(It.IsAny<DataFlow>(), appUser.Object)).Returns(userSecurity);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.RetrieverJob).Returns(new List<RetrieverJob>().AsQueryable());

            DataFlowService dataFlowService = new DataFlowService(datasetContext.Object, userService.Object, null, securityService.Object, null, null, null, null, null);
            DataFlowDto result = dataFlowService.UpdateDataFlowAsync(dto, dataFlow).Result;

            Assert.AreEqual(3, result.Id);
            Assert.AreEqual("Name", result.Name);
            Assert.AreEqual("SAID", result.SaidKeyCode);
            Assert.AreEqual(new DateTime(2023, 1, 1), result.CreateDTM);
            Assert.AreEqual("000000", result.CreatedBy);
            Assert.AreEqual("1234567", result.FlowStorageCode);
            Assert.AreEqual(2, result.MappedSchema.First());
            Assert.AreEqual(ObjectStatusEnum.Active, result.ObjectStatus);
            Assert.IsNull(result.DeleteIssuer);
            Assert.AreEqual(DateTime.MaxValue, result.DeleteIssueDTM);
            Assert.AreEqual((int)IngestionType.S3_Drop, result.IngestionType);
            Assert.IsFalse(result.IsCompressed);
            Assert.IsFalse(result.IsBackFillRequired);
            Assert.IsNull(result.CompressionType);
            Assert.IsFalse(result.IsPreProcessingRequired);
            Assert.AreEqual(0, result.PreProcessingOption);
            Assert.AreEqual("DEV", result.NamedEnvironment);
            Assert.AreEqual(NamedEnvironmentType.NonProd, result.NamedEnvironmentType);
            Assert.AreEqual("000000", result.PrimaryContactId);
            Assert.IsTrue(result.IsSecured);
            Assert.AreEqual(userSecurity, result.Security);
            Assert.IsNull(result.TopicName);
            Assert.IsNull(result.S3ConnectorName);
            Assert.AreEqual(1, result.DatasetId);
            Assert.AreEqual(2, result.SchemaId);
        }

        [TestMethod]
        public void UpdateDataFlowAsync_DataFlowStepUpdateRequired_DataFlowDto()
        {
            DateTime start = DateTime.Now;

            DataFlowDto dto = new DataFlowDto
            {
                DataFlowStepUpdateRequired = true,
                PreProcessingOption = 0
            };

            DataFlow dataFlow = new DataFlow
            {
                Id = 2,
                SaidKeyCode = "SAID",
                NamedEnvironment = "DEV",
                NamedEnvironmentType = NamedEnvironmentType.NonProd,
                ObjectStatus = ObjectStatusEnum.Active,
                DatasetId = 1,
                Name = "Name",
                IsSecured = true,
                SchemaId = 2,
                DeleteIssueDTM = DateTime.MaxValue,
                IngestionType = (int)IngestionType.S3_Drop,
                PrimaryContactId = "000000",
            };

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IApplicationUser> appUser = GetApplicationUser(mr);
            Mock<IUserService> userService = GetUserService(mr, appUser.Object);

            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();

            UserSecurity userSecurity = new UserSecurity();
            securityService.Setup(x => x.GetUserSecurity(It.IsAny<DataFlow>(), appUser.Object)).Returns(userSecurity);

            Mock<IDataFeatures> dataFeatures = mr.Create<IDataFeatures>();
            dataFeatures.Setup(x => x.CLA3241_DisableDfsDropLocation.GetValue()).Returns(false);
            dataFeatures.Setup(x => x.CLA4433_SEND_S3_SINK_CONNECTOR_REQUEST_EMAIL.GetValue()).Returns(true);
            dataFeatures.Setup(x => x.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()).Returns(string.Empty);

            Mock<IJobService> jobService = mr.Create<IJobService>();
            jobService.Setup(x => x.Delete(It.Is<List<int>>(l => !l.Any()), appUser.Object, false)).Returns(true);

            Dataset dataset = new Dataset()
            {
                DatasetId = 1,
                DatasetName = "Dataset Name",
                DatasetCategories = new List<Category>(),
                Asset = new Asset() { SaidKeyCode = "SAID" },
                NamedEnvironment = "DEV",
                NamedEnvironmentType = NamedEnvironmentType.NonProd
            };

            Mock<IDatasetContext> datasetContext = GetDatasetContext(mr, dataset);
            datasetContext.Setup(x => x.GetById<DataFlow>(2)).Returns(dataFlow);
            datasetContext.SetupGet(x => x.RetrieverJob).Returns(new List<RetrieverJob>().AsQueryable());

            DataFlowService dataFlowService = new DataFlowService(datasetContext.Object, userService.Object, jobService.Object, securityService.Object, null, dataFeatures.Object, null, null, null);
            DataFlowDto result = dataFlowService.UpdateDataFlowAsync(dto, dataFlow).Result;

            //deleted old dataflow
            Assert.AreEqual(ObjectStatusEnum.Deleted, dataFlow.ObjectStatus);
            Assert.AreEqual("000000", dataFlow.DeleteIssuer);
            Assert.IsTrue(dataFlow.DeleteIssueDTM >= start);

            //created new dataflow
            Assert.AreEqual(3, result.Id);
            Assert.AreEqual("Name", result.Name);
            Assert.AreEqual("SAID", result.SaidKeyCode);
            Assert.IsTrue(result.CreateDTM >= start);
            Assert.AreEqual("000000", result.CreatedBy);
            Assert.AreEqual("000001", result.FlowStorageCode);
            Assert.AreEqual(2, result.MappedSchema.First());
            Assert.AreEqual(ObjectStatusEnum.Active, result.ObjectStatus);
            Assert.IsNull(result.DeleteIssuer);
            Assert.AreEqual(DateTime.MaxValue, result.DeleteIssueDTM);
            Assert.AreEqual((int)IngestionType.S3_Drop, result.IngestionType);
            Assert.IsFalse(result.IsCompressed);
            Assert.IsFalse(result.IsBackFillRequired);
            Assert.IsNull(result.CompressionType);
            Assert.IsFalse(result.IsPreProcessingRequired);
            Assert.AreEqual(0, result.PreProcessingOption);
            Assert.AreEqual("DEV", result.NamedEnvironment);
            Assert.AreEqual(NamedEnvironmentType.NonProd, result.NamedEnvironmentType);
            Assert.AreEqual("000000", result.PrimaryContactId);
            Assert.IsTrue(result.IsSecured);
            Assert.AreEqual(userSecurity, result.Security);
            Assert.IsNull(result.TopicName);
            Assert.IsNull(result.S3ConnectorName);
            Assert.AreEqual(1, result.SchemaMap.First().DatasetId);
            Assert.AreEqual(2, result.SchemaMap.First().SchemaId);
            Assert.AreEqual(1, result.DatasetId);
            Assert.AreEqual(2, result.SchemaId);
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

            RetrieverJob retrieverJob = MockClasses.GetMockRetrieverJob();
            retrieverJob.DataFlow = new DataFlow() { Id = 3 };
            retrieverJob.DataSource = dataSource;
            datasetContext.SetupGet(x => x.RetrieverJob).Returns(new List<RetrieverJob>() { retrieverJob }.AsQueryable());

            Mock<IJobService> jobService = mockRepository.Create<IJobService>();
            jobService.Setup(x => x.CreateDfsRetrieverJob(It.Is<DataFlow>(d => d.Id == 3), dataSource)).Returns(retrieverJob);
            jobService.Setup(x => x.CreateDropLocation(retrieverJob));

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
