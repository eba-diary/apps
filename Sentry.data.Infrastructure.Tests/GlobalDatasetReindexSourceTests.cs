using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NHibernate.Mapping;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class GlobalDatasetReindexSourceTests
    {
        [TestMethod]
        public void TryGetNextDocuments_GlobalDatasets_1Page()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();

            List<Dataset> datasets = GetDatasets();
            datasetContext.SetupGet(x => x.Datasets).Returns(datasets.AsQueryable());

            List<DatasetFileConfig> fileConfigs = GetDatasetFileConfigs();
            datasetContext.SetupGet(x => x.DatasetFileConfigs).Returns(fileConfigs.AsQueryable());

            List<DataFlow> dataFlows = GetDataFlows();
            datasetContext.SetupGet(x => x.DataFlow).Returns(dataFlows.AsQueryable());

            GlobalDatasetReindexSource source = new GlobalDatasetReindexSource(datasetContext.Object);

            bool result = source.TryGetNextDocuments(out List<GlobalDataset> globalDatasets);

            Assert.IsTrue(result);

            Assert.AreEqual(2, globalDatasets.Count);

            GlobalDataset globalDataset = globalDatasets.First();
            Assert.AreEqual(1, globalDataset.GlobalDatasetId);
            Assert.AreEqual("Name 1", globalDataset.DatasetName);
            Assert.AreEqual("DATA", globalDataset.DatasetSaidAssetCode);
            Assert.AreEqual(1, globalDataset.EnvironmentDatasets.Count);

            EnvironmentDataset environmentDataset = globalDataset.EnvironmentDatasets.First();
            Assert.AreEqual(6, environmentDataset.DatasetId);
            Assert.AreEqual("Description 1", environmentDataset.DatasetDescription);
            Assert.AreEqual("Category", environmentDataset.CategoryCode);
            Assert.AreEqual("DEV", environmentDataset.NamedEnvironment);
            Assert.AreEqual(NamedEnvironmentType.NonProd.ToString(), environmentDataset.NamedEnvironmentType);
            Assert.AreEqual(DatasetOriginationCode.Internal.ToString(), environmentDataset.OriginationCode);
            Assert.IsTrue(environmentDataset.IsSecured);
            Assert.IsFalse(environmentDataset.FavoriteUserIds.Any());
            Assert.AreEqual(0, environmentDataset.EnvironmentSchemas.Count);
            
            globalDataset = globalDatasets.Last();
            Assert.AreEqual(2, globalDataset.GlobalDatasetId);
            Assert.AreEqual("Name 2", globalDataset.DatasetName);
            Assert.AreEqual("SAID", globalDataset.DatasetSaidAssetCode);
            Assert.AreEqual(2, globalDataset.EnvironmentDatasets.Count);

            environmentDataset = globalDataset.EnvironmentDatasets.First();
            Assert.AreEqual(3, environmentDataset.DatasetId);
            Assert.AreEqual("Description 2", environmentDataset.DatasetDescription);
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

            environmentDataset = globalDataset.EnvironmentDatasets.Last();
            Assert.AreEqual(5, environmentDataset.DatasetId);
            Assert.AreEqual("Description 2", environmentDataset.DatasetDescription);
            Assert.AreEqual("Category", environmentDataset.CategoryCode);
            Assert.AreEqual("TEST", environmentDataset.NamedEnvironment);
            Assert.AreEqual(NamedEnvironmentType.NonProd.ToString(), environmentDataset.NamedEnvironmentType);
            Assert.AreEqual(DatasetOriginationCode.Internal.ToString(), environmentDataset.OriginationCode);
            Assert.IsFalse(environmentDataset.IsSecured);
            Assert.IsFalse(environmentDataset.FavoriteUserIds.Any());
            Assert.AreEqual(1, environmentDataset.EnvironmentSchemas.Count);

            environmentSchema = environmentDataset.EnvironmentSchemas.First();
            Assert.AreEqual(8, environmentSchema.SchemaId);
            Assert.AreEqual("Schema Name 1", environmentSchema.SchemaName);
            Assert.AreEqual("Schema Description 1", environmentSchema.SchemaDescription);
            Assert.AreEqual("SAID", environmentSchema.SchemaSaidAssetCode);

            Assert.IsFalse(source.TryGetNextDocuments(out globalDatasets));
            Assert.IsFalse(globalDatasets.Any());

            mr.VerifyAll();
        }

        private List<Dataset> GetDatasets()
        {
            return new List<Dataset>
            {
                new Dataset
                {
                    GlobalDatasetId = 1,
                    DatasetId = 6,
                    ObjectStatus = ObjectStatusEnum.Active,
                    DatasetName = "Name 1",
                    DatasetDesc = "Description 1",
                    Asset = new Asset { SaidKeyCode = "DATA" },
                    DatasetCategories = new List<Category>
                    {
                        new Category { Name = "Category" }
                    },
                    NamedEnvironment = "DEV",
                    NamedEnvironmentType = NamedEnvironmentType.NonProd,
                    OriginationCode = DatasetOriginationCode.Internal.ToString(),
                    DatasetType = DataEntityCodes.DATASET,
                    IsSecured = true
                },
                new Dataset
                {
                    DatasetId = 7,
                    ObjectStatus = ObjectStatusEnum.Active,
                    DatasetType = DataEntityCodes.REPORT
                },
                new Dataset
                {
                    GlobalDatasetId = 2,
                    DatasetId = 3,
                    ObjectStatus = ObjectStatusEnum.Active,
                    DatasetName = "Name 2",
                    DatasetDesc = "Description 2",
                    Asset = new Asset { SaidKeyCode = "SAID" },
                    DatasetCategories = new List<Category>
                    {
                        new Category { Name = "Category" }
                    },
                    NamedEnvironment = "DEV",
                    NamedEnvironmentType = NamedEnvironmentType.NonProd,
                    OriginationCode = DatasetOriginationCode.Internal.ToString(),
                    DatasetType = DataEntityCodes.DATASET
                },
                new Dataset
                {
                    GlobalDatasetId = 2,
                    DatasetId = 4,
                    ObjectStatus = ObjectStatusEnum.Deleted,
                    DatasetType = DataEntityCodes.DATASET
                },
                new Dataset
                {
                    GlobalDatasetId = 2,
                    DatasetId = 5,
                    ObjectStatus = ObjectStatusEnum.Active,
                    DatasetName = "Name 2",
                    DatasetDesc = "Description 2",
                    Asset = new Asset { SaidKeyCode = "SAID" },
                    DatasetCategories = new List<Category>
                    {
                        new Category { Name = "Category" }
                    },
                    NamedEnvironment = "TEST",
                    NamedEnvironmentType = NamedEnvironmentType.NonProd,
                    OriginationCode = DatasetOriginationCode.Internal.ToString(),
                    DatasetType = DataEntityCodes.DATASET
                }
            };
        }

        private List<DatasetFileConfig> GetDatasetFileConfigs()
        {
            return new List<DatasetFileConfig>
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
                        Name = "Schema Name 1",
                        Description = "Schema Description 1"
                    }
                },
                new DatasetFileConfig
                {
                    ParentDataset = new Dataset { DatasetId = 5 },
                    ObjectStatus = ObjectStatusEnum.Deleted
                },
                new DatasetFileConfig
                {
                    ParentDataset = new Dataset { DatasetId = 20 },
                    ObjectStatus = ObjectStatusEnum.Active
                }
            };
        }

        private List<DataFlow> GetDataFlows()
        {
            return new List<DataFlow>
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
                },
                new DataFlow
                {
                    DatasetId = 20,
                    ObjectStatus = ObjectStatusEnum.Active
                }
            };
        }
    }
}
