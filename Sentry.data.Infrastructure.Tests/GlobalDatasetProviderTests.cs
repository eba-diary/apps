using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NHibernate.Util;
using Sentry.data.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

            Mock<IElasticContext> elasticContext = mr.Create<IElasticContext>();
            elasticContext.Setup(x => x.IndexAsync(globalDataset)).Returns(Task.CompletedTask);

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticContext.Object, null);

            await globalDatasetProvider.AddUpdateGlobalDatasetAsync(globalDataset);

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

            Mock<IElasticContext> elasticContext = mr.Create<IElasticContext>();
            elasticContext.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);
            elasticContext.Setup(x => x.IndexAsync(globalDataset)).Returns(Task.CompletedTask).Callback<GlobalDataset>(x =>
            {
                Assert.AreEqual(1, x.EnvironmentDatasets.Count);
                Assert.AreEqual(environmentDataset, x.EnvironmentDatasets.First());
            });

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticContext.Object, null);

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

            Mock<IElasticContext> elasticContext = mr.Create<IElasticContext>();
            elasticContext.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);
            elasticContext.Setup(x => x.IndexAsync(globalDataset)).Returns(Task.CompletedTask).Callback<GlobalDataset>(x =>
            {
                Assert.AreEqual(1, x.EnvironmentDatasets.Count);
                Assert.AreEqual(environmentDataset, x.EnvironmentDatasets.First());

                EnvironmentDataset updatedDataset = x.EnvironmentDatasets.First();
                Assert.AreEqual("New Description", updatedDataset.DatasetDescription);
                Assert.AreEqual(environmentSchemas, updatedDataset.EnvironmentSchemas);
            });

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticContext.Object, null);

            await globalDatasetProvider.AddUpdateEnvironmentDatasetAsync(1, environmentDataset);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task AddUpdateEnvironmentDatasetAsync_1_EnvironmentDataset_GlobalDatasetNotFound()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            EnvironmentDataset environmentDataset = new EnvironmentDataset();

            GlobalDataset globalDataset = null;

            Mock<IElasticContext> elasticContext = mr.Create<IElasticContext>();
            elasticContext.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticContext.Object, null);

            await globalDatasetProvider.AddUpdateEnvironmentDatasetAsync(1, environmentDataset);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task DeleteEnvironmentDatasetAsync_2_DeleteEnvironmentDatasetOnly()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticContext> elasticContext = mr.Create<IElasticContext>();

            GlobalDataset globalDataset = new GlobalDataset
            {
                EnvironmentDatasets = new List<EnvironmentDataset>
                {
                    new EnvironmentDataset { DatasetId = 2 },
                    new EnvironmentDataset { DatasetId = 3 }
                }
            };

            elasticContext.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);
            elasticContext.Setup(x => x.IndexAsync(globalDataset)).Returns(Task.CompletedTask).Callback<GlobalDataset>(x =>
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

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticContext.Object, datasetContext.Object);

            await globalDatasetProvider.DeleteEnvironmentDatasetAsync(2);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task DeleteEnvironmentDatasetAsync_2_DeleteWholeDatasetEnvironment()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticContext> elasticContext = mr.Create<IElasticContext>();

            GlobalDataset globalDataset = new GlobalDataset
            {
                GlobalDatasetId = 1,
                EnvironmentDatasets = new List<EnvironmentDataset>
                {
                    new EnvironmentDataset { DatasetId = 2 }
                }
            };

            elasticContext.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);
            elasticContext.Setup(x => x.DeleteByIdAsync<GlobalDataset>(1)).Returns(Task.CompletedTask);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            Dataset dataset = new Dataset
            {
                DatasetId = 2,
                GlobalDatasetId = 1
            };
            datasetContext.SetupGet(x => x.Datasets).Returns(new List<Dataset> { dataset }.AsQueryable());

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticContext.Object, datasetContext.Object);

            await globalDatasetProvider.DeleteEnvironmentDatasetAsync(2);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task DeleteEnvironmentDatasetAsync_2_GlobalDatasetNotFound()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            GlobalDataset globalDataset = null;

            Mock<IElasticContext> elasticContext = mr.Create<IElasticContext>();
            elasticContext.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            Dataset dataset = new Dataset
            {
                DatasetId = 2,
                GlobalDatasetId = 1
            };
            datasetContext.SetupGet(x => x.Datasets).Returns(new List<Dataset> { dataset }.AsQueryable());

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticContext.Object, datasetContext.Object);

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

            Mock<IElasticContext> elasticContext = mr.Create<IElasticContext>();
            elasticContext.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            Dataset dataset = new Dataset
            {
                DatasetId = 2,
                GlobalDatasetId = 1
            };
            datasetContext.SetupGet(x => x.Datasets).Returns(new List<Dataset> { dataset }.AsQueryable());

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticContext.Object, datasetContext.Object);

            await globalDatasetProvider.DeleteEnvironmentDatasetAsync(2);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task AddEnvironmentDatasetFavoriteUserIdAsync_2_NewUserId()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticContext> elasticContext = mr.Create<IElasticContext>();

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

            elasticContext.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);
            elasticContext.Setup(x => x.IndexAsync(globalDataset)).Returns(Task.CompletedTask).Callback<GlobalDataset>(x =>
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

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticContext.Object, datasetContext.Object);

            await globalDatasetProvider.AddEnvironmentDatasetFavoriteUserIdAsync(2, "000000");

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task AddEnvironmentDatasetFavoriteUserIdAsync_2_ExistingUserId()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticContext> elasticContext = mr.Create<IElasticContext>();

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

            elasticContext.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            Dataset dataset = new Dataset
            {
                DatasetId = 2,
                GlobalDatasetId = 1
            };
            datasetContext.SetupGet(x => x.Datasets).Returns(new List<Dataset> { dataset }.AsQueryable());

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticContext.Object, datasetContext.Object);

            await globalDatasetProvider.AddEnvironmentDatasetFavoriteUserIdAsync(2, "000000");

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task AddEnvironmentDatasetFavoriteUserIdAsync_2_GlobalDatasetNotFound()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticContext> elasticContext = mr.Create<IElasticContext>();

            GlobalDataset globalDataset = null;

            elasticContext.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            Dataset dataset = new Dataset
            {
                DatasetId = 2,
                GlobalDatasetId = 1
            };
            datasetContext.SetupGet(x => x.Datasets).Returns(new List<Dataset> { dataset }.AsQueryable());

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticContext.Object, datasetContext.Object);

            await globalDatasetProvider.AddEnvironmentDatasetFavoriteUserIdAsync(2, "000000");

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task RemoveEnvironmentDatasetFavoriteUserIdAsync_2_ExistingUserId()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticContext> elasticContext = mr.Create<IElasticContext>();

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

            elasticContext.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);
            elasticContext.Setup(x => x.IndexAsync(globalDataset)).Returns(Task.CompletedTask).Callback<GlobalDataset>(x =>
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

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticContext.Object, datasetContext.Object);

            await globalDatasetProvider.RemoveEnvironmentDatasetFavoriteUserIdAsync(2, "000000");

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task RemoveEnvironmentDatasetFavoriteUserIdAsync_2_DoesNotContainUserId()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticContext> elasticContext = mr.Create<IElasticContext>();

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

            elasticContext.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            Dataset dataset = new Dataset
            {
                DatasetId = 2,
                GlobalDatasetId = 1
            };
            datasetContext.SetupGet(x => x.Datasets).Returns(new List<Dataset> { dataset }.AsQueryable());

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticContext.Object, datasetContext.Object);

            await globalDatasetProvider.RemoveEnvironmentDatasetFavoriteUserIdAsync(2, "000000");

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task RemoveEnvironmentDatasetFavoriteUserIdAsync_2_GlobalDatasetNotFound()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticContext> elasticContext = mr.Create<IElasticContext>();

            GlobalDataset globalDataset = null;

            elasticContext.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            Dataset dataset = new Dataset
            {
                DatasetId = 2,
                GlobalDatasetId = 1
            };
            datasetContext.SetupGet(x => x.Datasets).Returns(new List<Dataset> { dataset }.AsQueryable());

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticContext.Object, datasetContext.Object);

            await globalDatasetProvider.RemoveEnvironmentDatasetFavoriteUserIdAsync(2, "000000");

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

            Mock<IElasticContext> elasticContext = mr.Create<IElasticContext>(); 
            
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
            elasticContext.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);
            elasticContext.Setup(x => x.IndexAsync(globalDataset)).Returns(Task.CompletedTask).Callback<GlobalDataset>(x =>
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

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticContext.Object, datasetContext.Object);

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

            Mock<IElasticContext> elasticContext = mr.Create<IElasticContext>();

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

            elasticContext.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);
            elasticContext.Setup(x => x.IndexAsync(globalDataset)).Returns(Task.CompletedTask).Callback<GlobalDataset>(x =>
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

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticContext.Object, datasetContext.Object);

            await globalDatasetProvider.AddUpdateEnvironmentSchemaAsync(3, environmentSchema);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task AddUpdateEnvironmentSchemaAsync_3_EnvironmentSchema_GlobalDatasetNotFound()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticContext> elasticContext = mr.Create<IElasticContext>();

            GlobalDataset globalDataset = null;

            elasticContext.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            Dataset dataset = new Dataset
            {
                DatasetId = 3,
                GlobalDatasetId = 1
            };
            datasetContext.SetupGet(x => x.Datasets).Returns(new List<Dataset> { dataset }.AsQueryable());

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticContext.Object, datasetContext.Object);

            await globalDatasetProvider.AddUpdateEnvironmentSchemaAsync(3, null);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task DeleteEnvironmentSchemaAsync_4_Delete()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticContext> elasticContext = mr.Create<IElasticContext>();

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

            elasticContext.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);
            elasticContext.Setup(x => x.IndexAsync(globalDataset)).Returns(Task.CompletedTask).Callback<GlobalDataset>(x =>
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

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticContext.Object, datasetContext.Object);

            await globalDatasetProvider.DeleteEnvironmentSchemaAsync(4);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task DeleteEnvironmentSchemaAsync_4_GlobalDatasetNotFound()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticContext> elasticContext = mr.Create<IElasticContext>();

            GlobalDataset globalDataset = null;

            elasticContext.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);

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

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticContext.Object, datasetContext.Object);

            await globalDatasetProvider.DeleteEnvironmentSchemaAsync(4);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task DeleteEnvironmentSchemaAsync_4_EnvironmentDatasetNotFound()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticContext> elasticContext = mr.Create<IElasticContext>();

            GlobalDataset globalDataset = new GlobalDataset
            {
                EnvironmentDatasets = new List<EnvironmentDataset>()
            };

            elasticContext.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);

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

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticContext.Object, datasetContext.Object);

            await globalDatasetProvider.DeleteEnvironmentSchemaAsync(4);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task DeleteEnvironmentSchemaAsync_4_EnvironmentSchemaNotFound()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticContext> elasticContext = mr.Create<IElasticContext>();

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

            elasticContext.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);

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

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticContext.Object, datasetContext.Object);

            await globalDatasetProvider.DeleteEnvironmentSchemaAsync(4);

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task AddUpdateEnvironmentSchemaSaidAssetCodeAsync_4_SAID()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticContext> elasticContext = mr.Create<IElasticContext>();

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

            elasticContext.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);
            elasticContext.Setup(x => x.IndexAsync(globalDataset)).Returns(Task.CompletedTask).Callback<GlobalDataset>(x =>
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

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticContext.Object, datasetContext.Object);

            await globalDatasetProvider.AddUpdateEnvironmentSchemaSaidAssetCodeAsync(4, "SAID");

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task AddUpdateEnvironmentSchemaSaidAssetCodeAsync_GlobalDatasetNotFound()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticContext> elasticContext = mr.Create<IElasticContext>();

            GlobalDataset globalDataset = null;

            elasticContext.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);

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

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticContext.Object, datasetContext.Object);

            await globalDatasetProvider.AddUpdateEnvironmentSchemaSaidAssetCodeAsync(4, "SAID");

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task AddUpdateEnvironmentSchemaSaidAssetCodeAsync_EnvironmentDatasetNotFound()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticContext> elasticContext = mr.Create<IElasticContext>();

            GlobalDataset globalDataset = new GlobalDataset
            {
                EnvironmentDatasets = new List<EnvironmentDataset>()
            };

            elasticContext.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);

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

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticContext.Object, datasetContext.Object);

            await globalDatasetProvider.AddUpdateEnvironmentSchemaSaidAssetCodeAsync(4, "SAID");

            mr.VerifyAll();
        }

        [TestMethod]
        public async Task AddUpdateEnvironmentSchemaSaidAssetCodeAsync_EnvironmentSchemaNotFound()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticContext> elasticContext = mr.Create<IElasticContext>();

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

            elasticContext.Setup(x => x.GetByIdAsync<GlobalDataset>(1)).ReturnsAsync(globalDataset);

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

            GlobalDatasetProvider globalDatasetProvider = new GlobalDatasetProvider(elasticContext.Object, datasetContext.Object);

            await globalDatasetProvider.AddUpdateEnvironmentSchemaSaidAssetCodeAsync(4, "SAID");

            mr.VerifyAll();
        }
    }
}
