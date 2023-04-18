using Hangfire;
using Hangfire.Client;
using Hangfire.States;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class GlobalDatasetAdminServiceTests
    {
        [TestMethod]
        public async Task IndexGlobalDatasetsAsync_IndexAll_BackgroundJobId()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDataFeatures> dataFeatures = mr.Create<IDataFeatures>();
            dataFeatures.Setup(x => x.CLA4789_ImprovedSearchCapability.GetValue()).Returns(true);

            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser().IsAdmin).Returns(true);

            Mock<IBackgroundJobFactory> backgroundJobFactory = mr.Create<IBackgroundJobFactory>();
            BackgroundJob backgroundJob = new BackgroundJob("123", null, DateTime.Now);
            backgroundJobFactory.Setup(x => x.Create(It.IsAny<CreateContext>())).Returns(backgroundJob);

            Mock<IBackgroundJobStateChanger> backgroundJobStateChanger = mr.Create<IBackgroundJobStateChanger>();

            BackgroundJobClient backgroundJobClient = new BackgroundJobClient(new MockJobStorage(), backgroundJobFactory.Object, backgroundJobStateChanger.Object);

            Mock<IReindexService> reindexService = mr.Create<IReindexService>();

            GlobalDatasetAdminService service = new GlobalDatasetAdminService(userService.Object, backgroundJobClient, null, null, reindexService.Object, dataFeatures.Object);

            IndexGlobalDatasetsDto dto = new IndexGlobalDatasetsDto
            {
                IndexAll = true
            };

            IndexGlobalDatasetsResultDto result = await service.IndexGlobalDatasetsAsync(dto);

            Assert.AreEqual("123", result.BackgroundJobId);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task IndexGlobalDatasetsAsync_IndexGlobalDatasetIds_DeleteAndIndexCount()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDataFeatures> dataFeatures = mr.Create<IDataFeatures>();
            dataFeatures.Setup(x => x.CLA4789_ImprovedSearchCapability.GetValue()).Returns(true);

            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser().IsAdmin).Returns(true);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();

            List<Dataset> datasets = new List<Dataset>
            {
                new Dataset
                {
                    GlobalDatasetId = 1,
                    ObjectStatus = ObjectStatusEnum.Deleted
                },
                new Dataset
                {
                    GlobalDatasetId = 1,
                    ObjectStatus = ObjectStatusEnum.Deleted
                },
                new Dataset
                {
                    GlobalDatasetId = 2,
                    DatasetId = 3,
                    ObjectStatus = ObjectStatusEnum.Active,
                    DatasetName = "Name",
                    DatasetDesc = "Description",
                    Asset = new Asset { SaidKeyCode = "SAID" },
                    DatasetCategories = new List<Category>
                    {
                        new Category { Name = "Category" }
                    },
                    NamedEnvironment = "DEV",
                    NamedEnvironmentType = NamedEnvironmentType.NonProd,
                    OriginationCode = DatasetOriginationCode.Internal.ToString()
                },
                new Dataset
                {
                    GlobalDatasetId = 2,
                    DatasetId = 4,
                    ObjectStatus = ObjectStatusEnum.Deleted
                },
                new Dataset
                {
                    GlobalDatasetId = 2,
                    DatasetId = 5,
                    ObjectStatus = ObjectStatusEnum.Active,
                    DatasetName = "Name",
                    DatasetDesc = "Description",
                    Asset = new Asset { SaidKeyCode = "SAID" },
                    DatasetCategories = new List<Category>
                    {
                        new Category { Name = "Category" }
                    },
                    NamedEnvironment = "TEST",
                    NamedEnvironmentType = NamedEnvironmentType.NonProd,
                    OriginationCode = DatasetOriginationCode.Internal.ToString()
                }
            };

            datasetContext.SetupGet(x => x.Datasets).Returns(datasets.AsQueryable());

            List<DatasetFileConfig> fileConfigs = new List<DatasetFileConfig>
            {
                new DatasetFileConfig
                {
                    ParentDataset = new Dataset { DatasetId = 3 },
                    ObjectStatus = ObjectStatusEnum.Active,
                    Schema = new FileSchema
                    {
                        SchemaId = 6,
                        Name = "Schema Name 1",
                        Description = "Schema Description 1"
                    }
                },
                new DatasetFileConfig
                {
                    ParentDataset = new Dataset { DatasetId = 3 },
                    ObjectStatus = ObjectStatusEnum.Active,
                    Schema = new FileSchema
                    {
                        SchemaId = 7,
                        Name = "Schema Name 2",
                        Description = "Schema Description 2"
                    }
                },
                new DatasetFileConfig
                {
                    ParentDataset = new Dataset { DatasetId = 5 },
                    ObjectStatus = ObjectStatusEnum.Active,
                    Schema = new FileSchema
                    {
                        SchemaId = 8,
                        Name = "Schema Name 3",
                        Description = "Schema Description 3"
                    }
                },
                new DatasetFileConfig
                {
                    ParentDataset = new Dataset { DatasetId = 5 },
                    ObjectStatus = ObjectStatusEnum.Deleted
                }
            };

            datasetContext.SetupGet(x => x.DatasetFileConfigs).Returns(fileConfigs.AsQueryable());

            List<DataFlow> dataFlows = new List<DataFlow>
            {
                new DataFlow
                {
                    DatasetId = 3,
                    ObjectStatus = ObjectStatusEnum.Active,
                    SchemaId = 6,
                    SaidKeyCode = "SAID"
                },
                new DataFlow
                {
                    DatasetId = 3,
                    ObjectStatus = ObjectStatusEnum.Deleted,
                    SchemaId = 7
                },
                new DataFlow
                {
                    DatasetId = 5,
                    ObjectStatus = ObjectStatusEnum.Active,
                    SchemaId = 8,
                    SaidKeyCode = "SAID"
                }
            };

            datasetContext.SetupGet(x => x.DataFlow).Returns(dataFlows.AsQueryable());

            Mock<IGlobalDatasetProvider> globalDatasetProvider = mr.Create<IGlobalDatasetProvider>();
            globalDatasetProvider.Setup(x => x.DeleteGlobalDatasetsAsync(It.IsAny<List<int>>())).Returns(Task.CompletedTask).Callback<List<int>>(x =>
            {
                Assert.AreEqual(1, x.Count);
                Assert.AreEqual(1, x.First());
            });
            globalDatasetProvider.Setup(x => x.AddUpdateGlobalDatasetsAsync(It.IsAny<List<GlobalDataset>>())).Returns(Task.CompletedTask).Callback<List<GlobalDataset>>(x =>
            {
                Assert.AreEqual(1, x.Count);

                GlobalDataset result = x.First();
                Assert.AreEqual(2, result.GlobalDatasetId);
                Assert.AreEqual("Name", result.DatasetName);
                Assert.AreEqual("SAID", result.DatasetSaidAssetCode);
                Assert.AreEqual(2, result.EnvironmentDatasets.Count);

                EnvironmentDataset environmentDataset = result.EnvironmentDatasets.First();
                Assert.AreEqual(3, environmentDataset.DatasetId);
                Assert.AreEqual("Description", environmentDataset.DatasetDescription);
                Assert.AreEqual("Category", environmentDataset.CategoryCode);
                Assert.AreEqual("DEV", environmentDataset.NamedEnvironment);
                Assert.AreEqual(NamedEnvironmentType.NonProd.ToString(), environmentDataset.NamedEnvironmentType);
                Assert.AreEqual(DatasetOriginationCode.Internal.ToString(), environmentDataset.OriginationCode);
                Assert.IsFalse(environmentDataset.IsSecured);
                Assert.IsFalse(environmentDataset.FavoriteUserIds.Any());
                Assert.AreEqual(2, environmentDataset.EnvironmentSchemas.Count);

                EnvironmentSchema environmentSchema = environmentDataset.EnvironmentSchemas.First();
                Assert.AreEqual(6, environmentSchema.SchemaId);
                Assert.AreEqual("Schema Name 1", environmentSchema.SchemaName);
                Assert.AreEqual("Schema Description 1", environmentSchema.SchemaDescription);
                Assert.AreEqual("SAID", environmentSchema.SchemaSaidAssetCode);

                environmentSchema = environmentDataset.EnvironmentSchemas.Last();
                Assert.AreEqual(7, environmentSchema.SchemaId);
                Assert.AreEqual("Schema Name 2", environmentSchema.SchemaName);
                Assert.AreEqual("Schema Description 2", environmentSchema.SchemaDescription);
                Assert.IsNull(environmentSchema.SchemaSaidAssetCode); 
                
                environmentDataset = result.EnvironmentDatasets.Last();
                Assert.AreEqual(5, environmentDataset.DatasetId);
                Assert.AreEqual("Description", environmentDataset.DatasetDescription);
                Assert.AreEqual("Category", environmentDataset.CategoryCode);
                Assert.AreEqual("TEST", environmentDataset.NamedEnvironment);
                Assert.AreEqual(NamedEnvironmentType.NonProd.ToString(), environmentDataset.NamedEnvironmentType);
                Assert.AreEqual(DatasetOriginationCode.Internal.ToString(), environmentDataset.OriginationCode);
                Assert.IsFalse(environmentDataset.IsSecured);
                Assert.IsFalse(environmentDataset.FavoriteUserIds.Any());
                Assert.AreEqual(1, environmentDataset.EnvironmentSchemas.Count);

                environmentSchema = environmentDataset.EnvironmentSchemas.First();
                Assert.AreEqual(8, environmentSchema.SchemaId);
                Assert.AreEqual("Schema Name 3", environmentSchema.SchemaName);
                Assert.AreEqual("Schema Description 3", environmentSchema.SchemaDescription);
                Assert.AreEqual("SAID", environmentSchema.SchemaSaidAssetCode);
            });

            GlobalDatasetAdminService service = new GlobalDatasetAdminService(userService.Object, null, datasetContext.Object, globalDatasetProvider.Object, null, dataFeatures.Object);

            IndexGlobalDatasetsDto dto = new IndexGlobalDatasetsDto
            {
                GlobalDatasetIds = new List<int> { 1, 2 }
            };

            IndexGlobalDatasetsResultDto resultDto = await service.IndexGlobalDatasetsAsync(dto);

            Assert.AreEqual(1, resultDto.IndexCount);
            Assert.AreEqual(1, resultDto.DeleteCount);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task IndexGlobalDatasetsAsync_FeatureDisabled_ResourceFeatureDisabledException()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDataFeatures> dataFeatures = mr.Create<IDataFeatures>();
            dataFeatures.Setup(x => x.CLA4789_ImprovedSearchCapability.GetValue()).Returns(false);

            GlobalDatasetAdminService service = new GlobalDatasetAdminService(null, null, null, null, null, dataFeatures.Object);

            await Assert.ThrowsExceptionAsync<ResourceFeatureDisabledException>(() => service.IndexGlobalDatasetsAsync(null));

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task IndexGlobalDatasetsAsync_NotAdmin_ResourceForbiddenException()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDataFeatures> dataFeatures = mr.Create<IDataFeatures>();
            dataFeatures.Setup(x => x.CLA4789_ImprovedSearchCapability.GetValue()).Returns(true);

            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser().IsAdmin).Returns(false);
            userService.Setup(x => x.GetCurrentUser().AssociateId).Returns("000000");

            GlobalDatasetAdminService service = new GlobalDatasetAdminService(userService.Object, null, null, null, null, dataFeatures.Object);

            await Assert.ThrowsExceptionAsync<ResourceForbiddenException>(() => service.IndexGlobalDatasetsAsync(null));

            mr.VerifyAll();
        }
    }
}
