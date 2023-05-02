using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nest;
using NHibernate.Util;
using Sentry.data.Core;
using Sentry.data.Infrastructure.CherwellService;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class GlobalDatasetProviderTests
    {
        [TestMethod]
        public async Task AddUpdateGlobalDatasetAsync_GlobalDataset()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            GlobalDataset globalDataset = new GlobalDataset();

            Mock<IElasticDocumentClient> elasticDocumentClient = mr.Create<IElasticDocumentClient>();
            elasticDocumentClient.Setup(x => x.IndexAsync(globalDataset)).Returns(Task.CompletedTask);

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticDocumentClient.Object, null);

            await globalDatasetProvider.AddUpdateGlobalDatasetAsync(globalDataset);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task AddUpdateGlobalDatasetsAsync_GlobalDatasets()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            List<GlobalDataset> globalDatasets = new List<GlobalDataset>
            {
                new GlobalDataset(),
                new GlobalDataset()
            };

            Mock<IElasticDocumentClient> elasticDocumentClient = mr.Create<IElasticDocumentClient>();
            elasticDocumentClient.Setup(x => x.IndexManyAsync(globalDatasets)).Returns(Task.CompletedTask).Callback<List<GlobalDataset>>(x =>
            {
                Assert.AreEqual(2, x.Count);
            });

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticDocumentClient.Object, null);

            await globalDatasetProvider.AddUpdateGlobalDatasetsAsync(globalDatasets);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task DeleteGlobalDatasetsAsync_GlobalDatasetIds()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            List<int> globalDatasetIds = new List<int> { 1, 2 };

            Mock<IElasticDocumentClient> elasticDocumentClient = mr.Create<IElasticDocumentClient>();
            elasticDocumentClient.Setup(x => x.DeleteManyAsync(It.IsAny<List<GlobalDataset>>())).Returns(Task.CompletedTask).Callback<List<GlobalDataset>>(x =>
            {
                Assert.AreEqual(2, x.Count);
                Assert.AreEqual(1, x.First().GlobalDatasetId);
                Assert.AreEqual(2, x.Last().GlobalDatasetId);
            });

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticDocumentClient.Object, null);

            await globalDatasetProvider.DeleteGlobalDatasetsAsync(globalDatasetIds);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task AddUpdateEnvironmentDatasetAsync_1_EnvironmentDataset_Add()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            GlobalDataset globalDataset = new GlobalDataset
            {
                EnvironmentDatasets = new List<EnvironmentDataset>()
            };

            EnvironmentDataset environmentDataset = new EnvironmentDataset
            {
                DatasetId = 2
            };

            Mock<IElasticDocumentClient> elasticDocumentClient = mr.Create<IElasticDocumentClient>();
            elasticDocumentClient.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);
            elasticDocumentClient.Setup(x => x.IndexAsync(globalDataset)).Returns(Task.CompletedTask).Callback<GlobalDataset>(x =>
            {
                Assert.AreEqual(1, x.EnvironmentDatasets.Count);
                Assert.AreEqual(environmentDataset, x.EnvironmentDatasets.First());
            });

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticDocumentClient.Object, null);

            await globalDatasetProvider.AddUpdateEnvironmentDatasetAsync(1, environmentDataset);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task AddUpdateEnvironmentDatasetAsync_1_EnvironmentDataset_Update()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            List<EnvironmentSchema> environmentSchemas = new List<EnvironmentSchema>();

            GlobalDataset globalDataset = new GlobalDataset
            {
                EnvironmentDatasets = new List<EnvironmentDataset>
                {
                    new EnvironmentDataset
                    {
                        DatasetId = 2,
                        DatasetDescription = "Description",
                        EnvironmentSchemas = environmentSchemas
                    }
                }
            };

            EnvironmentDataset environmentDataset = new EnvironmentDataset
            {
                DatasetId = 2,
                DatasetDescription = "New Description"
            };

            Mock<IElasticDocumentClient> elasticDocumentClient = mr.Create<IElasticDocumentClient>();
            elasticDocumentClient.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);
            elasticDocumentClient.Setup(x => x.IndexAsync(globalDataset)).Returns(Task.CompletedTask).Callback<GlobalDataset>(x =>
            {
                Assert.AreEqual(1, x.EnvironmentDatasets.Count);
                Assert.AreEqual(environmentDataset, x.EnvironmentDatasets.First());

                EnvironmentDataset updatedDataset = x.EnvironmentDatasets.First();
                Assert.AreEqual("New Description", updatedDataset.DatasetDescription);
                Assert.AreEqual(environmentSchemas, updatedDataset.EnvironmentSchemas);
            });

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticDocumentClient.Object, null);

            await globalDatasetProvider.AddUpdateEnvironmentDatasetAsync(1, environmentDataset);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task AddUpdateEnvironmentDatasetAsync_1_EnvironmentDataset_GlobalDatasetNotFound()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            EnvironmentDataset environmentDataset = new EnvironmentDataset();

            GlobalDataset globalDataset = null;

            Mock<IElasticDocumentClient> elasticDocumentClient = mr.Create<IElasticDocumentClient>();
            elasticDocumentClient.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticDocumentClient.Object, null);

            await globalDatasetProvider.AddUpdateEnvironmentDatasetAsync(1, environmentDataset);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task DeleteEnvironmentDatasetAsync_2_DeleteEnvironmentDatasetOnly()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticDocumentClient> elasticDocumentClient = mr.Create<IElasticDocumentClient>();

            GlobalDataset globalDataset = new GlobalDataset
            {
                EnvironmentDatasets = new List<EnvironmentDataset>
                {
                    new EnvironmentDataset { DatasetId = 2 },
                    new EnvironmentDataset { DatasetId = 3 }
                }
            };

            elasticDocumentClient.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);
            elasticDocumentClient.Setup(x => x.IndexAsync(globalDataset)).Returns(Task.CompletedTask).Callback<GlobalDataset>(x =>
            {
                Assert.AreEqual(1, x.EnvironmentDatasets.Count);
                Assert.AreEqual(3, x.EnvironmentDatasets.First().DatasetId);
            });

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            Dataset dataset = new Dataset
            {
                DatasetId = 2,
                GlobalDatasetId = 1
            };
            datasetContext.SetupGet(x => x.Datasets).Returns(new List<Dataset> { dataset }.AsQueryable());

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticDocumentClient.Object, datasetContext.Object);

            await globalDatasetProvider.DeleteEnvironmentDatasetAsync(2);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task DeleteEnvironmentDatasetAsync_2_DeleteWholeDatasetEnvironment()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticDocumentClient> elasticDocumentClient = mr.Create<IElasticDocumentClient>();

            GlobalDataset globalDataset = new GlobalDataset
            {
                GlobalDatasetId = 1,
                EnvironmentDatasets = new List<EnvironmentDataset>
                {
                    new EnvironmentDataset { DatasetId = 2 }
                }
            };

            elasticDocumentClient.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);
            elasticDocumentClient.Setup(x => x.DeleteByIdAsync<GlobalDataset>(1)).Returns(Task.CompletedTask);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            Dataset dataset = new Dataset
            {
                DatasetId = 2,
                GlobalDatasetId = 1
            };
            datasetContext.SetupGet(x => x.Datasets).Returns(new List<Dataset> { dataset }.AsQueryable());

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticDocumentClient.Object, datasetContext.Object);

            await globalDatasetProvider.DeleteEnvironmentDatasetAsync(2);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task DeleteEnvironmentDatasetAsync_2_GlobalDatasetNotFound()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            GlobalDataset globalDataset = null;

            Mock<IElasticDocumentClient> elasticDocumentClient = mr.Create<IElasticDocumentClient>();
            elasticDocumentClient.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            Dataset dataset = new Dataset
            {
                DatasetId = 2,
                GlobalDatasetId = 1
            };
            datasetContext.SetupGet(x => x.Datasets).Returns(new List<Dataset> { dataset }.AsQueryable());

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticDocumentClient.Object, datasetContext.Object);

            await globalDatasetProvider.DeleteEnvironmentDatasetAsync(2);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task DeleteEnvironmentDatasetAsync_2_EnvironmentDatasetNotFound()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            GlobalDataset globalDataset = new GlobalDataset
            {
                EnvironmentDatasets = new List<EnvironmentDataset>()
            };

            Mock<IElasticDocumentClient> elasticDocumentClient = mr.Create<IElasticDocumentClient>();
            elasticDocumentClient.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            Dataset dataset = new Dataset
            {
                DatasetId = 2,
                GlobalDatasetId = 1
            };
            datasetContext.SetupGet(x => x.Datasets).Returns(new List<Dataset> { dataset }.AsQueryable());

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticDocumentClient.Object, datasetContext.Object);

            await globalDatasetProvider.DeleteEnvironmentDatasetAsync(2);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task AddEnvironmentDatasetFavoriteUserIdAsync_2_NewUserId()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticDocumentClient> elasticDocumentClient = mr.Create<IElasticDocumentClient>();

            GlobalDataset globalDataset = new GlobalDataset
            {
                GlobalDatasetId = 1,
                EnvironmentDatasets = new List<EnvironmentDataset>
                {
                    new EnvironmentDataset 
                    {
                        DatasetId = 2,
                        FavoriteUserIds = new List<string>()
                    }
                }
            };

            elasticDocumentClient.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);
            elasticDocumentClient.Setup(x => x.IndexAsync(globalDataset)).Returns(Task.CompletedTask).Callback<GlobalDataset>(x =>
            {
                EnvironmentDataset updatedDataset = x.EnvironmentDatasets.First();
                Assert.AreEqual(1, updatedDataset.FavoriteUserIds.Count);
                Assert.AreEqual("000000", updatedDataset.FavoriteUserIds.First());
            });

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            Dataset dataset = new Dataset
            {
                DatasetId = 2,
                GlobalDatasetId = 1
            };
            datasetContext.SetupGet(x => x.Datasets).Returns(new List<Dataset> { dataset }.AsQueryable());

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticDocumentClient.Object, datasetContext.Object);

            await globalDatasetProvider.AddEnvironmentDatasetFavoriteUserIdAsync(2, "000000");

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task AddEnvironmentDatasetFavoriteUserIdAsync_2_ExistingUserId()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticDocumentClient> elasticDocumentClient = mr.Create<IElasticDocumentClient>();

            GlobalDataset globalDataset = new GlobalDataset
            {
                GlobalDatasetId = 1,
                EnvironmentDatasets = new List<EnvironmentDataset>
                {
                    new EnvironmentDataset
                    {
                        DatasetId = 2,
                        FavoriteUserIds = new List<string> { "000000" }
                    }
                }
            };

            elasticDocumentClient.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            Dataset dataset = new Dataset
            {
                DatasetId = 2,
                GlobalDatasetId = 1
            };
            datasetContext.SetupGet(x => x.Datasets).Returns(new List<Dataset> { dataset }.AsQueryable());

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticDocumentClient.Object, datasetContext.Object);

            await globalDatasetProvider.AddEnvironmentDatasetFavoriteUserIdAsync(2, "000000");

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task AddEnvironmentDatasetFavoriteUserIdAsync_2_GlobalDatasetNotFound()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticDocumentClient> elasticDocumentClient = mr.Create<IElasticDocumentClient>();

            GlobalDataset globalDataset = null;

            elasticDocumentClient.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            Dataset dataset = new Dataset
            {
                DatasetId = 2,
                GlobalDatasetId = 1
            };
            datasetContext.SetupGet(x => x.Datasets).Returns(new List<Dataset> { dataset }.AsQueryable());

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticDocumentClient.Object, datasetContext.Object);

            await globalDatasetProvider.AddEnvironmentDatasetFavoriteUserIdAsync(2, "000000");

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task RemoveEnvironmentDatasetFavoriteUserIdAsync_2_ExistingUserId()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticDocumentClient> elasticDocumentClient = mr.Create<IElasticDocumentClient>();

            GlobalDataset globalDataset = new GlobalDataset
            {
                GlobalDatasetId = 1,
                EnvironmentDatasets = new List<EnvironmentDataset>
                {
                    new EnvironmentDataset
                    {
                        DatasetId = 2,
                        FavoriteUserIds = new List<string> { "000000" }
                    }
                }
            };

            elasticDocumentClient.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);
            elasticDocumentClient.Setup(x => x.IndexAsync(globalDataset)).Returns(Task.CompletedTask).Callback<GlobalDataset>(x =>
            {
                EnvironmentDataset updatedDataset = x.EnvironmentDatasets.First();
                Assert.AreEqual(0, updatedDataset.FavoriteUserIds.Count);
            });

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            Dataset dataset = new Dataset
            {
                DatasetId = 2,
                GlobalDatasetId = 1
            };
            datasetContext.SetupGet(x => x.Datasets).Returns(new List<Dataset> { dataset }.AsQueryable());

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticDocumentClient.Object, datasetContext.Object);

            await globalDatasetProvider.RemoveEnvironmentDatasetFavoriteUserIdAsync(2, "000000", false);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task RemoveEnvironmentDatasetFavoriteUserIdAsync_2_RemoveForAllEnvironments()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticDocumentClient> elasticDocumentClient = mr.Create<IElasticDocumentClient>();

            GlobalDataset globalDataset = new GlobalDataset
            {
                GlobalDatasetId = 1,
                EnvironmentDatasets = new List<EnvironmentDataset>
                {
                    new EnvironmentDataset
                    {
                        DatasetId = 2,
                        FavoriteUserIds = new List<string> { "000000" }
                    },
                    new EnvironmentDataset
                    {
                        DatasetId = 3,
                        FavoriteUserIds = new List<string> { "000000", "000001" }
                    }
                }
            };

            elasticDocumentClient.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);
            elasticDocumentClient.Setup(x => x.IndexAsync(globalDataset)).Returns(Task.CompletedTask).Callback<GlobalDataset>(x =>
            {
                EnvironmentDataset updatedDataset = x.EnvironmentDatasets.First();
                Assert.AreEqual(0, updatedDataset.FavoriteUserIds.Count);
                
                updatedDataset = x.EnvironmentDatasets.Last();
                Assert.AreEqual(1, updatedDataset.FavoriteUserIds.Count);
                Assert.AreEqual("000001", updatedDataset.FavoriteUserIds.First());
            });

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            Dataset dataset = new Dataset
            {
                DatasetId = 2,
                GlobalDatasetId = 1
            };
            datasetContext.SetupGet(x => x.Datasets).Returns(new List<Dataset> { dataset }.AsQueryable());

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticDocumentClient.Object, datasetContext.Object);

            await globalDatasetProvider.RemoveEnvironmentDatasetFavoriteUserIdAsync(2, "000000", true);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task RemoveEnvironmentDatasetFavoriteUserIdAsync_2_DoesNotContainUserId()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticDocumentClient> elasticDocumentClient = mr.Create<IElasticDocumentClient>();

            GlobalDataset globalDataset = new GlobalDataset
            {
                GlobalDatasetId = 1,
                EnvironmentDatasets = new List<EnvironmentDataset>
                {
                    new EnvironmentDataset
                    {
                        DatasetId = 2,
                        FavoriteUserIds = new List<string>()
                    }
                }
            };

            elasticDocumentClient.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);
            elasticDocumentClient.Setup(x => x.IndexAsync(globalDataset)).Returns(Task.CompletedTask).Callback<GlobalDataset>(x =>
            {
                EnvironmentDataset updatedDataset = x.EnvironmentDatasets.First();
                Assert.AreEqual(0, updatedDataset.FavoriteUserIds.Count);
            });

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            Dataset dataset = new Dataset
            {
                DatasetId = 2,
                GlobalDatasetId = 1
            };
            datasetContext.SetupGet(x => x.Datasets).Returns(new List<Dataset> { dataset }.AsQueryable());

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticDocumentClient.Object, datasetContext.Object);

            await globalDatasetProvider.RemoveEnvironmentDatasetFavoriteUserIdAsync(2, "000000", false);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task RemoveEnvironmentDatasetFavoriteUserIdAsync_2_GlobalDatasetNotFound()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticDocumentClient> elasticDocumentClient = mr.Create<IElasticDocumentClient>();

            GlobalDataset globalDataset = null;

            elasticDocumentClient.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            Dataset dataset = new Dataset
            {
                DatasetId = 2,
                GlobalDatasetId = 1
            };
            datasetContext.SetupGet(x => x.Datasets).Returns(new List<Dataset> { dataset }.AsQueryable());

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticDocumentClient.Object, datasetContext.Object);

            await globalDatasetProvider.RemoveEnvironmentDatasetFavoriteUserIdAsync(2, "000000", false);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task AddUpdateEnvironmentSchemaAsync_3_EnvironmentSchema_Add()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            EnvironmentSchema environmentSchema = new EnvironmentSchema
            {
                SchemaId = 4
            };

            Mock<IElasticDocumentClient> elasticDocumentClient = mr.Create<IElasticDocumentClient>(); 
            
            GlobalDataset globalDataset = new GlobalDataset
            {
                EnvironmentDatasets = new List<EnvironmentDataset>
                {
                    new EnvironmentDataset
                    {
                        DatasetId = 2,
                        EnvironmentSchemas = new List<EnvironmentSchema>()
                    },
                    new EnvironmentDataset
                    {
                        DatasetId = 3,
                        EnvironmentSchemas = new List<EnvironmentSchema>()
                    }
                }
            };
            elasticDocumentClient.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);
            elasticDocumentClient.Setup(x => x.IndexAsync(globalDataset)).Returns(Task.CompletedTask).Callback<GlobalDataset>(x =>
            {
                Assert.AreEqual(2, x.EnvironmentDatasets.Count);

                EnvironmentDataset updatedDataset = x.EnvironmentDatasets.First(f => f.DatasetId == 2);
                Assert.AreEqual(0, updatedDataset.EnvironmentSchemas.Count);

                updatedDataset = x.EnvironmentDatasets.First(f => f.DatasetId == 3);
                Assert.AreEqual(1, updatedDataset.EnvironmentSchemas.Count);

                EnvironmentSchema updatedSchema = updatedDataset.EnvironmentSchemas.First();
                Assert.AreEqual(environmentSchema, updatedSchema);
            });

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            Dataset dataset = new Dataset
            {
                DatasetId = 3,
                GlobalDatasetId = 1
            };
            datasetContext.SetupGet(x => x.Datasets).Returns(new List<Dataset> { dataset }.AsQueryable());

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticDocumentClient.Object, datasetContext.Object);

            await globalDatasetProvider.AddUpdateEnvironmentSchemaAsync(3, environmentSchema);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task AddUpdateEnvironmentSchemaAsync_3_EnvironmentSchema_Update()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            EnvironmentSchema environmentSchema = new EnvironmentSchema
            {
                SchemaId = 4,
                SchemaName = "New Name"
            };

            Mock<IElasticDocumentClient> elasticDocumentClient = mr.Create<IElasticDocumentClient>();

            GlobalDataset globalDataset = new GlobalDataset
            {
                EnvironmentDatasets = new List<EnvironmentDataset>
                {
                    new EnvironmentDataset
                    {
                        DatasetId = 2,
                        EnvironmentSchemas = new List<EnvironmentSchema>()
                    },
                    new EnvironmentDataset
                    {
                        DatasetId = 3,
                        EnvironmentSchemas = new List<EnvironmentSchema>
                        {
                            new EnvironmentSchema
                            {
                                SchemaId = 4,
                                SchemaName = "Name"
                            }
                        }
                    }
                }
            };

            elasticDocumentClient.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);
            elasticDocumentClient.Setup(x => x.IndexAsync(globalDataset)).Returns(Task.CompletedTask).Callback<GlobalDataset>(x =>
            {
                Assert.AreEqual(2, x.EnvironmentDatasets.Count);

                EnvironmentDataset updatedDataset = x.EnvironmentDatasets.First(f => f.DatasetId == 2);
                Assert.AreEqual(0, updatedDataset.EnvironmentSchemas.Count);

                updatedDataset = x.EnvironmentDatasets.First(f => f.DatasetId == 3);
                Assert.AreEqual(1, updatedDataset.EnvironmentSchemas.Count);

                EnvironmentSchema updatedSchema = updatedDataset.EnvironmentSchemas.First();
                Assert.AreEqual(environmentSchema, updatedSchema);
                Assert.AreEqual("New Name", updatedSchema.SchemaName);
            });

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            Dataset dataset = new Dataset
            {
                DatasetId = 3,
                GlobalDatasetId = 1
            };
            datasetContext.SetupGet(x => x.Datasets).Returns(new List<Dataset> { dataset }.AsQueryable());

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticDocumentClient.Object, datasetContext.Object);

            await globalDatasetProvider.AddUpdateEnvironmentSchemaAsync(3, environmentSchema);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task AddUpdateEnvironmentSchemaAsync_3_EnvironmentSchema_GlobalDatasetNotFound()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            EnvironmentSchema environmentSchema = new EnvironmentSchema
            {
                SchemaId = 4
            };

            Mock<IElasticDocumentClient> elasticDocumentClient = mr.Create<IElasticDocumentClient>();

            GlobalDataset globalDataset = null;

            elasticDocumentClient.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            Dataset dataset = new Dataset
            {
                DatasetId = 3,
                GlobalDatasetId = 1
            };
            datasetContext.SetupGet(x => x.Datasets).Returns(new List<Dataset> { dataset }.AsQueryable());

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticDocumentClient.Object, datasetContext.Object);

            await globalDatasetProvider.AddUpdateEnvironmentSchemaAsync(3, environmentSchema);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task DeleteEnvironmentSchemaAsync_4_Delete()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticDocumentClient> elasticDocumentClient = mr.Create<IElasticDocumentClient>();

            GlobalDataset globalDataset = new GlobalDataset
            {
                EnvironmentDatasets = new List<EnvironmentDataset>
                {
                    new EnvironmentDataset
                    {
                        DatasetId = 2,
                        EnvironmentSchemas = new List<EnvironmentSchema>()
                    },
                    new EnvironmentDataset
                    {
                        DatasetId = 3,
                        EnvironmentSchemas = new List<EnvironmentSchema>
                        {
                            new EnvironmentSchema
                            {
                                SchemaId = 4,
                                SchemaName = "Name"
                            }
                        }
                    }
                }
            };

            elasticDocumentClient.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);
            elasticDocumentClient.Setup(x => x.IndexAsync(globalDataset)).Returns(Task.CompletedTask).Callback<GlobalDataset>(x =>
            {
                Assert.AreEqual(2, x.EnvironmentDatasets.Count);

                EnvironmentDataset updatedDataset = x.EnvironmentDatasets.First(f => f.DatasetId == 2);
                Assert.AreEqual(0, updatedDataset.EnvironmentSchemas.Count);

                updatedDataset = x.EnvironmentDatasets.First(f => f.DatasetId == 3);
                Assert.AreEqual(0, updatedDataset.EnvironmentSchemas.Count);
            });

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            DatasetFileConfig fileConfig = new DatasetFileConfig
            {
                Schema = new FileSchema { SchemaId = 4 },
                ParentDataset = new Dataset
                {
                    DatasetId = 2,
                    GlobalDatasetId = 1
                }
            };
            datasetContext.SetupGet(x => x.DatasetFileConfigs).Returns(new List<DatasetFileConfig> { fileConfig }.AsQueryable());

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticDocumentClient.Object, datasetContext.Object);

            await globalDatasetProvider.DeleteEnvironmentSchemaAsync(4);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task DeleteEnvironmentSchemaAsync_4_GlobalDatasetNotFound()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticDocumentClient> elasticDocumentClient = mr.Create<IElasticDocumentClient>();

            GlobalDataset globalDataset = null;

            elasticDocumentClient.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            DatasetFileConfig fileConfig = new DatasetFileConfig
            {
                Schema = new FileSchema { SchemaId = 4 },
                ParentDataset = new Dataset
                {
                    DatasetId = 2,
                    GlobalDatasetId = 1
                }
            };
            datasetContext.SetupGet(x => x.DatasetFileConfigs).Returns(new List<DatasetFileConfig> { fileConfig }.AsQueryable());

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticDocumentClient.Object, datasetContext.Object);

            await globalDatasetProvider.DeleteEnvironmentSchemaAsync(4);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task DeleteEnvironmentSchemaAsync_4_EnvironmentDatasetNotFound()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticDocumentClient> elasticDocumentClient = mr.Create<IElasticDocumentClient>();

            GlobalDataset globalDataset = new GlobalDataset
            {
                EnvironmentDatasets = new List<EnvironmentDataset>()
            };

            elasticDocumentClient.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            DatasetFileConfig fileConfig = new DatasetFileConfig
            {
                Schema = new FileSchema { SchemaId = 4 },
                ParentDataset = new Dataset
                {
                    DatasetId = 2,
                    GlobalDatasetId = 1
                }
            };
            datasetContext.SetupGet(x => x.DatasetFileConfigs).Returns(new List<DatasetFileConfig> { fileConfig }.AsQueryable());

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticDocumentClient.Object, datasetContext.Object);

            await globalDatasetProvider.DeleteEnvironmentSchemaAsync(4);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task DeleteEnvironmentSchemaAsync_4_EnvironmentSchemaNotFound()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticDocumentClient> elasticDocumentClient = mr.Create<IElasticDocumentClient>();

            GlobalDataset globalDataset = new GlobalDataset
            {
                EnvironmentDatasets = new List<EnvironmentDataset>
                {
                    new EnvironmentDataset
                    {
                        DatasetId = 2,
                        EnvironmentSchemas = new List<EnvironmentSchema>()
                    }
                }
            };

            elasticDocumentClient.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            DatasetFileConfig fileConfig = new DatasetFileConfig
            {
                Schema = new FileSchema { SchemaId = 4 },
                ParentDataset = new Dataset
                {
                    DatasetId = 2,
                    GlobalDatasetId = 1
                }
            };
            datasetContext.SetupGet(x => x.DatasetFileConfigs).Returns(new List<DatasetFileConfig> { fileConfig }.AsQueryable());

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticDocumentClient.Object, datasetContext.Object);

            await globalDatasetProvider.DeleteEnvironmentSchemaAsync(4);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task AddUpdateEnvironmentSchemaSaidAssetCodeAsync_4_SAID()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticDocumentClient> elasticDocumentClient = mr.Create<IElasticDocumentClient>();

            GlobalDataset globalDataset = new GlobalDataset
            {
                EnvironmentDatasets = new List<EnvironmentDataset>
                {
                    new EnvironmentDataset
                    {
                        DatasetId = 2,
                        EnvironmentSchemas = new List<EnvironmentSchema>()
                    },
                    new EnvironmentDataset
                    {
                        DatasetId = 3,
                        EnvironmentSchemas = new List<EnvironmentSchema>
                        {
                            new EnvironmentSchema
                            {
                                SchemaId = 4
                            }
                        }
                    }
                }
            };

            elasticDocumentClient.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);
            elasticDocumentClient.Setup(x => x.IndexAsync(globalDataset)).Returns(Task.CompletedTask).Callback<GlobalDataset>(x =>
            {
                EnvironmentDataset updatedDataset = x.EnvironmentDatasets.First(f => f.DatasetId == 3);
                Assert.AreEqual(1, updatedDataset.EnvironmentSchemas.Count);

                EnvironmentSchema updatedSchema = updatedDataset.EnvironmentSchemas.First();
                Assert.AreEqual("SAID", updatedSchema.SchemaSaidAssetCode);
            });

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            DatasetFileConfig fileConfig = new DatasetFileConfig
            {
                Schema = new FileSchema { SchemaId = 4 },
                ParentDataset = new Dataset
                {
                    DatasetId = 2,
                    GlobalDatasetId = 1
                }
            };
            datasetContext.SetupGet(x => x.DatasetFileConfigs).Returns(new List<DatasetFileConfig> { fileConfig }.AsQueryable());

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticDocumentClient.Object, datasetContext.Object);

            await globalDatasetProvider.AddUpdateEnvironmentSchemaSaidAssetCodeAsync(4, "SAID");

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task AddUpdateEnvironmentSchemaSaidAssetCodeAsync_GlobalDatasetNotFound()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticDocumentClient> elasticDocumentClient = mr.Create<IElasticDocumentClient>();

            GlobalDataset globalDataset = null;

            elasticDocumentClient.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            DatasetFileConfig fileConfig = new DatasetFileConfig
            {
                Schema = new FileSchema { SchemaId = 4 },
                ParentDataset = new Dataset
                {
                    DatasetId = 2,
                    GlobalDatasetId = 1
                }
            };
            datasetContext.SetupGet(x => x.DatasetFileConfigs).Returns(new List<DatasetFileConfig> { fileConfig }.AsQueryable());

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticDocumentClient.Object, datasetContext.Object);

            await globalDatasetProvider.AddUpdateEnvironmentSchemaSaidAssetCodeAsync(4, "SAID");

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task AddUpdateEnvironmentSchemaSaidAssetCodeAsync_EnvironmentDatasetNotFound()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticDocumentClient> elasticDocumentClient = mr.Create<IElasticDocumentClient>();

            GlobalDataset globalDataset = new GlobalDataset
            {
                EnvironmentDatasets = new List<EnvironmentDataset>()
            };

            elasticDocumentClient.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            DatasetFileConfig fileConfig = new DatasetFileConfig
            {
                Schema = new FileSchema { SchemaId = 4 },
                ParentDataset = new Dataset
                {
                    DatasetId = 2,
                    GlobalDatasetId = 1
                }
            };
            datasetContext.SetupGet(x => x.DatasetFileConfigs).Returns(new List<DatasetFileConfig> { fileConfig }.AsQueryable());

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticDocumentClient.Object, datasetContext.Object);

            await globalDatasetProvider.AddUpdateEnvironmentSchemaSaidAssetCodeAsync(4, "SAID");

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task AddUpdateEnvironmentSchemaSaidAssetCodeAsync_EnvironmentSchemaNotFound()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticDocumentClient> elasticDocumentClient = mr.Create<IElasticDocumentClient>();

            GlobalDataset globalDataset = new GlobalDataset
            {
                EnvironmentDatasets = new List<EnvironmentDataset>
                {
                    new EnvironmentDataset
                    {
                        DatasetId = 2,
                        EnvironmentSchemas = new List<EnvironmentSchema>()
                    }
                }
            };

            elasticDocumentClient.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            DatasetFileConfig fileConfig = new DatasetFileConfig
            {
                Schema = new FileSchema { SchemaId = 4 },
                ParentDataset = new Dataset
                {
                    DatasetId = 2,
                    GlobalDatasetId = 1
                }
            };
            datasetContext.SetupGet(x => x.DatasetFileConfigs).Returns(new List<DatasetFileConfig> { fileConfig }.AsQueryable());

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticDocumentClient.Object, datasetContext.Object);

            await globalDatasetProvider.AddUpdateEnvironmentSchemaSaidAssetCodeAsync(4, "SAID");

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task SearchGlobalDatasetsAsync_BaseFilterSearchDto_GlobalDatasets()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticDocumentClient> elasticDocumentClient = mr.Create<IElasticDocumentClient>();

            ElasticResult<GlobalDataset> elasticResult = new ElasticResult<GlobalDataset>
            {
                Documents = new List<GlobalDataset>
                {
                    new GlobalDataset()
                }
            };

            elasticDocumentClient.Setup(x => x.SearchAsync(It.IsAny<SearchRequest<GlobalDataset>>())).ReturnsAsync(elasticResult).Callback<SearchRequest<GlobalDataset>>(s =>
            {
                Assert.AreEqual(10000, s.Size);

                IBoolQuery query = ((IQueryContainer)s.Query).Bool;
                Assert.AreEqual(2, query.Should.Count());

                IQueryStringQuery stringQuery = ((IQueryContainer)query.Should.First()).QueryString;
                Assert.AreEqual("search", stringQuery.Query);
                Assert.AreEqual(4, stringQuery.Fields.Count());
                Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "datasetname" && x.Boost == 5));
                Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "environmentdatasets.datasetdescription"));
                Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "environmentdatasets.environmentschemas.schemaname"));
                Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "environmentdatasets.environmentschemas.schemadescription"));

                stringQuery = ((IQueryContainer)query.Should.Last()).QueryString;
                Assert.AreEqual("*search*", stringQuery.Query);
                Assert.AreEqual(4, stringQuery.Fields.Count());
                Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "datasetname" && x.Boost == 5));
                Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "environmentdatasets.datasetdescription"));
                Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "environmentdatasets.environmentschemas.schemaname"));
                Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "environmentdatasets.environmentschemas.schemadescription"));

                Assert.AreEqual(3, query.Filter.Count());

                List<IQueryContainer> filters = query.Filter.Select(x => (IQueryContainer)x).ToList();

                ITermsQuery termsQuery = filters.FirstOrDefault(x => x.Terms.Field.Name == "datasetsaidassetcode.keyword").Terms;
                Assert.IsNotNull(termsQuery);
                Assert.AreEqual(2, termsQuery.Terms.Count());
                Assert.AreEqual("SAID", termsQuery.Terms.First().ToString());
                Assert.AreEqual("DATA", termsQuery.Terms.Last().ToString());

                termsQuery = filters.FirstOrDefault(x => x.Terms.Field.Name == "environmentdatasets.issecured").Terms;
                Assert.IsNotNull(termsQuery);
                Assert.AreEqual(1, termsQuery.Terms.Count());
                Assert.AreEqual("true", termsQuery.Terms.First().ToString());

                termsQuery = filters.FirstOrDefault(x => x.Terms.Field.Name == "environmentdatasets.environmentschemas.schemasaidassetcode.keyword").Terms;
                Assert.IsNotNull(termsQuery);
                Assert.AreEqual("TEST", termsQuery.Terms.First().ToString());
            });

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticDocumentClient.Object, null);

            SearchGlobalDatasetsDto filterSearchDto = new SearchGlobalDatasetsDto
            {
                SearchText = "search",
                FilterCategories = new List<FilterCategoryDto>
                {
                    new FilterCategoryDto
                    {
                        CategoryName = FilterCategoryNames.Dataset.PRODUCERASSET,
                        CategoryOptions = new List<FilterCategoryOptionDto>
                        {
                            new FilterCategoryOptionDto
                            {
                                OptionValue = "TEST",
                                Selected = true
                            }
                        }
                    },
                    new FilterCategoryDto
                    {
                        CategoryName = FilterCategoryNames.Dataset.SECURED,
                        CategoryOptions = new List<FilterCategoryOptionDto>
                        {
                            new FilterCategoryOptionDto
                            {
                                OptionValue = "true",
                                Selected = true
                            },
                            new FilterCategoryOptionDto
                            {
                                OptionValue = "false",
                                Selected = false
                            }
                        }
                    },
                    new FilterCategoryDto
                    {
                        CategoryName = FilterCategoryNames.Dataset.DATASETASSET,
                        CategoryOptions = new List<FilterCategoryOptionDto>
                        {
                            new FilterCategoryOptionDto
                            {
                                OptionValue = "SAID",
                                Selected = true
                            },
                            new FilterCategoryOptionDto
                            {
                                OptionValue = "DATA",
                                Selected = true
                            }
                        }
                    }
                }
            };

            List<GlobalDataset> results = await globalDatasetProvider.SearchGlobalDatasetsAsync(filterSearchDto);

            Assert.AreEqual(1, results.Count);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task GetGlobalDatasetFiltersAsync_BaseFilterSearchDto_GlobalDatasets()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticDocumentClient> elasticDocumentClient = mr.Create<IElasticDocumentClient>();

            ElasticResult<GlobalDataset> elasticResult = new ElasticResult<GlobalDataset>
            {
                Documents = new List<GlobalDataset>
                {
                    new GlobalDataset()
                },
                Aggregations = new AggregateDictionary(new Dictionary<string, IAggregate>
                {
                    [FilterCategoryNames.Dataset.DATASETASSET] = new BucketAggregate()
                    {
                        SumOtherDocCount = 0,
                        Items = new List<KeyedBucket<object>>
                        {
                            new KeyedBucket<object>(new Dictionary<string, IAggregate>())
                            {
                                DocCount = 5,
                                Key = "SAID"
                            },
                            new KeyedBucket<object>(new Dictionary<string, IAggregate>())
                            {
                                DocCount = 3,
                                Key = "DATA"
                            }
                        }.AsReadOnly()
                    },
                    [FilterCategoryNames.Dataset.SECURED] = new BucketAggregate()
                    {
                        SumOtherDocCount = 0,
                        Items = new List<KeyedBucket<object>>
                        {
                            new KeyedBucket<object>(new Dictionary<string, IAggregate>())
                            {
                                DocCount = 6,
                                Key = "1",
                                KeyAsString = "true"
                            }
                        }.AsReadOnly()
                    },
                    [FilterCategoryNames.Dataset.PRODUCERASSET] = new BucketAggregate()
                    {
                        SumOtherDocCount = 0,
                        Items = new List<KeyedBucket<object>>
                        {
                            new KeyedBucket<object>(new Dictionary<string, IAggregate>())
                            {
                                DocCount = 2,
                                Key = "SAID"
                            },
                            new KeyedBucket<object>(new Dictionary<string, IAggregate>())
                            {
                                DocCount = 8,
                                Key = "OTHR"
                            }
                        }.AsReadOnly()
                    },
                    [FilterCategoryNames.Dataset.ORIGIN] = new BucketAggregate()
                    {
                        SumOtherDocCount = 0,
                        Items = new List<KeyedBucket<object>>().AsReadOnly()
                    },
                    [FilterCategoryNames.Dataset.CATEGORY] = new BucketAggregate()
                    {
                        SumOtherDocCount = 0,
                        Items = new List<KeyedBucket<object>>
                        {
                            new KeyedBucket<object>(new Dictionary<string, IAggregate>())
                            {
                                DocCount = 2,
                                Key = "Sentry"
                            },
                            new KeyedBucket<object>(new Dictionary<string, IAggregate>())
                            {
                                DocCount = 8,
                                Key = "Industry"
                            },
                            new KeyedBucket<object>(new Dictionary<string, IAggregate>())
                            {
                                DocCount = 8,
                                Key = "Claim"
                            }
                        }.AsReadOnly()
                    },
                    [FilterCategoryNames.Dataset.ENVIRONMENT] = new BucketAggregate()
                    {
                        SumOtherDocCount = 0,
                        Items = new List<KeyedBucket<object>>
                        {
                            new KeyedBucket<object>(new Dictionary<string, IAggregate>())
                            {
                                DocCount = 2,
                                Key = "DEV"
                            },
                            new KeyedBucket<object>(new Dictionary<string, IAggregate>())
                            {
                                DocCount = 8,
                                Key = "TEST"
                            }
                        }.AsReadOnly()
                    },
                    [FilterCategoryNames.Dataset.ENVIRONMENTTYPE] = new BucketAggregate()
                    {
                        SumOtherDocCount = 0,
                        Items = new List<KeyedBucket<object>>
                        {
                            new KeyedBucket<object>(new Dictionary<string, IAggregate>())
                            {
                                DocCount = 2,
                                Key = "NonProd"
                            },
                            new KeyedBucket<object>(new Dictionary<string, IAggregate>())
                            {
                                DocCount = 8,
                                Key = "Prod"
                            }
                        }.AsReadOnly()
                    }
                })
            };

            elasticDocumentClient.Setup(x => x.SearchAsync(It.IsAny<SearchRequest<GlobalDataset>>())).ReturnsAsync(elasticResult).Callback<SearchRequest<GlobalDataset>>(s =>
            {
                Assert.AreEqual(0, s.Size);

                IBoolQuery query = ((IQueryContainer)s.Query).Bool;
                Assert.AreEqual(2, query.Should.Count());

                IQueryStringQuery stringQuery = ((IQueryContainer)query.Should.First()).QueryString;
                Assert.AreEqual("search", stringQuery.Query);
                Assert.AreEqual(4, stringQuery.Fields.Count());
                Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "datasetname" && x.Boost == 5));
                Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "environmentdatasets.datasetdescription"));
                Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "environmentdatasets.environmentschemas.schemaname"));
                Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "environmentdatasets.environmentschemas.schemadescription"));

                stringQuery = ((IQueryContainer)query.Should.Last()).QueryString;
                Assert.AreEqual("*search*", stringQuery.Query);
                Assert.AreEqual(4, stringQuery.Fields.Count());
                Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "datasetname" && x.Boost == 5));
                Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "environmentdatasets.datasetdescription"));
                Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "environmentdatasets.environmentschemas.schemaname"));
                Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "environmentdatasets.environmentschemas.schemadescription"));

                Assert.IsNull(query.Filter);

                Assert.IsNotNull(s.Aggregations);

                AggregationDictionary fields = s.Aggregations;
                Assert.AreEqual(7, fields.Count());

                Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.Dataset.DATASETASSET));
                Assert.AreEqual("datasetsaidassetcode.keyword", fields[FilterCategoryNames.Dataset.DATASETASSET].Terms.Field.Name);

                Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.Dataset.CATEGORY));
                Assert.AreEqual("environmentdatasets.categorycode.keyword", fields[FilterCategoryNames.Dataset.CATEGORY].Terms.Field.Name);

                Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.Dataset.ENVIRONMENT));
                Assert.AreEqual("environmentdatasets.namedenvironment.keyword", fields[FilterCategoryNames.Dataset.ENVIRONMENT].Terms.Field.Name);

                Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.Dataset.ENVIRONMENTTYPE));
                Assert.AreEqual("environmentdatasets.namedenvironmenttype.keyword", fields[FilterCategoryNames.Dataset.ENVIRONMENTTYPE].Terms.Field.Name);

                Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.Dataset.ORIGIN));
                Assert.AreEqual("environmentdatasets.originationcode.keyword", fields[FilterCategoryNames.Dataset.ORIGIN].Terms.Field.Name);

                Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.Dataset.SECURED));
                Assert.AreEqual("environmentdatasets.issecured", fields[FilterCategoryNames.Dataset.SECURED].Terms.Field.Name);

                Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.Dataset.PRODUCERASSET));
                Assert.AreEqual("environmentdatasets.environmentschemas.schemasaidassetcode.keyword", fields[FilterCategoryNames.Dataset.PRODUCERASSET].Terms.Field.Name);
            });

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticDocumentClient.Object, null);

            SearchGlobalDatasetsDto filterSearchDto = new SearchGlobalDatasetsDto
            {
                SearchText = "search",
                FilterCategories = new List<FilterCategoryDto>
                {
                    new FilterCategoryDto
                    {
                        CategoryName = FilterCategoryNames.Dataset.PRODUCERASSET,
                        CategoryOptions = new List<FilterCategoryOptionDto>
                        {
                            new FilterCategoryOptionDto
                            {
                                OptionValue = "OTHR",
                                Selected = true
                            }
                        }
                    },
                    new FilterCategoryDto
                    {
                        CategoryName = FilterCategoryNames.Dataset.SECURED,
                        CategoryOptions = new List<FilterCategoryOptionDto>
                        {
                            new FilterCategoryOptionDto
                            {
                                OptionValue = "true",
                                Selected = true
                            }
                        }
                    },
                    new FilterCategoryDto
                    {
                        CategoryName = FilterCategoryNames.Dataset.DATASETASSET,
                        CategoryOptions = new List<FilterCategoryOptionDto>
                        {
                            new FilterCategoryOptionDto
                            {
                                OptionValue = "SAID",
                                Selected = true
                            },
                            new FilterCategoryOptionDto
                            {
                                OptionValue = "DATA",
                                Selected = true
                            }
                        }
                    }
                }
            };

            List<FilterCategoryDto> results = await globalDatasetProvider.GetGlobalDatasetFiltersAsync(filterSearchDto);

            Assert.AreEqual(6, results.Count);

            FilterCategoryDto filter = results[0];
            Assert.AreEqual(FilterCategoryNames.Dataset.DATASETASSET, filter.CategoryName);
            Assert.AreEqual(2, filter.CategoryOptions.Count);
            Assert.IsTrue(filter.HideResultCounts);
            Assert.IsFalse(filter.DefaultCategoryOpen);

            FilterCategoryOptionDto option = filter.CategoryOptions[0];
            Assert.AreEqual("SAID", option.OptionValue);
            Assert.AreEqual(5, option.ResultCount);
            Assert.AreEqual(FilterCategoryNames.Dataset.DATASETASSET, option.ParentCategoryName);
            Assert.IsTrue(option.Selected);

            option = filter.CategoryOptions[1];
            Assert.AreEqual("DATA", option.OptionValue);
            Assert.AreEqual(3, option.ResultCount);
            Assert.AreEqual(FilterCategoryNames.Dataset.DATASETASSET, option.ParentCategoryName);
            Assert.IsTrue(option.Selected);

            filter = results[1];
            Assert.AreEqual(FilterCategoryNames.Dataset.CATEGORY, filter.CategoryName);
            Assert.AreEqual(3, filter.CategoryOptions.Count);
            Assert.IsFalse(filter.HideResultCounts);
            Assert.IsTrue(filter.DefaultCategoryOpen);

            option = filter.CategoryOptions[0];
            Assert.AreEqual("Sentry", option.OptionValue);
            Assert.AreEqual(2, option.ResultCount);
            Assert.AreEqual(FilterCategoryNames.Dataset.CATEGORY, option.ParentCategoryName);
            Assert.IsFalse(option.Selected);

            option = filter.CategoryOptions[1];
            Assert.AreEqual("Industry", option.OptionValue);
            Assert.AreEqual(8, option.ResultCount);
            Assert.AreEqual(FilterCategoryNames.Dataset.CATEGORY, option.ParentCategoryName);
            Assert.IsFalse(option.Selected);

            option = filter.CategoryOptions[2];
            Assert.AreEqual("Claim", option.OptionValue);
            Assert.AreEqual(8, option.ResultCount);
            Assert.AreEqual(FilterCategoryNames.Dataset.CATEGORY, option.ParentCategoryName);
            Assert.IsFalse(option.Selected);

            filter = results[2];
            Assert.AreEqual(FilterCategoryNames.Dataset.ENVIRONMENT, filter.CategoryName);
            Assert.AreEqual(2, filter.CategoryOptions.Count);
            Assert.IsTrue(filter.HideResultCounts);
            Assert.IsFalse(filter.DefaultCategoryOpen);

            option = filter.CategoryOptions[0];
            Assert.AreEqual("DEV", option.OptionValue);
            Assert.AreEqual(2, option.ResultCount);
            Assert.AreEqual(FilterCategoryNames.Dataset.ENVIRONMENT, option.ParentCategoryName);
            Assert.IsFalse(option.Selected);

            option = filter.CategoryOptions[1];
            Assert.AreEqual("TEST", option.OptionValue);
            Assert.AreEqual(8, option.ResultCount);
            Assert.AreEqual(FilterCategoryNames.Dataset.ENVIRONMENT, option.ParentCategoryName);
            Assert.IsFalse(option.Selected);

            filter = results[3];
            Assert.AreEqual(FilterCategoryNames.Dataset.ENVIRONMENTTYPE, filter.CategoryName);
            Assert.AreEqual(2, filter.CategoryOptions.Count);
            Assert.IsTrue(filter.HideResultCounts);
            Assert.IsTrue(filter.DefaultCategoryOpen);

            option = filter.CategoryOptions[0];
            Assert.AreEqual("NonProd", option.OptionValue);
            Assert.AreEqual(2, option.ResultCount);
            Assert.AreEqual(FilterCategoryNames.Dataset.ENVIRONMENTTYPE, option.ParentCategoryName);
            Assert.IsFalse(option.Selected);

            option = filter.CategoryOptions[1];
            Assert.AreEqual("Prod", option.OptionValue);
            Assert.AreEqual(8, option.ResultCount);
            Assert.AreEqual(FilterCategoryNames.Dataset.ENVIRONMENTTYPE, option.ParentCategoryName);
            Assert.IsFalse(option.Selected);

            filter = results[4];
            Assert.AreEqual(FilterCategoryNames.Dataset.SECURED, filter.CategoryName);
            Assert.AreEqual(1, filter.CategoryOptions.Count);
            Assert.IsTrue(filter.HideResultCounts);
            Assert.IsFalse(filter.DefaultCategoryOpen);

            option = filter.CategoryOptions[0];
            Assert.AreEqual(FilterCategoryNames.Dataset.SECURED, filter.CategoryName);
            Assert.AreEqual(1, filter.CategoryOptions.Count);
            Assert.IsTrue(filter.HideResultCounts);
            Assert.IsFalse(filter.DefaultCategoryOpen);

            option = filter.CategoryOptions[0];
            Assert.AreEqual("true", option.OptionValue);
            Assert.AreEqual(6, option.ResultCount);
            Assert.AreEqual(FilterCategoryNames.Dataset.SECURED, option.ParentCategoryName);
            Assert.IsTrue(option.Selected);

            filter = results[5];
            Assert.AreEqual(FilterCategoryNames.Dataset.PRODUCERASSET, filter.CategoryName);
            Assert.AreEqual(2, filter.CategoryOptions.Count);
            Assert.IsTrue(filter.HideResultCounts);
            Assert.IsFalse(filter.DefaultCategoryOpen);

            option = filter.CategoryOptions[0];
            Assert.AreEqual("SAID", option.OptionValue);
            Assert.AreEqual(2, option.ResultCount);
            Assert.AreEqual(FilterCategoryNames.Dataset.PRODUCERASSET, option.ParentCategoryName);
            Assert.IsFalse(option.Selected);

            option = filter.CategoryOptions[1];
            Assert.AreEqual("OTHR", option.OptionValue);
            Assert.AreEqual(8, option.ResultCount);
            Assert.AreEqual(FilterCategoryNames.Dataset.PRODUCERASSET, option.ParentCategoryName);
            Assert.IsTrue(option.Selected);

            mr.VerifyAll();
        }
    }
}
