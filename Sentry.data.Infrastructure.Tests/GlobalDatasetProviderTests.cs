using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nest;
using NHibernate.Util;
using Sentry.data.Core;
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

            elasticDocumentClient.Setup(x => x.SearchAsync(It.IsAny<SearchRequest<GlobalDataset>>())).ReturnsAsync(elasticResult);

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
    }
}
