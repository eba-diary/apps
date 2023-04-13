using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
using System.Collections.Generic;
using System.Linq;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class GlobalDatasetExtensionsTests
    {
        [TestMethod]
        public void ToGlobalDataset_Dataset_GlobalDataset()
        {
            Dataset dataset = new Dataset
            {
                GlobalDatasetId = 1,
                DatasetName = "Name",
                Asset = new Asset { SaidKeyCode = "SAID" },
                DatasetId = 2,
                DatasetDesc = "Description",
                DatasetCategories = new List<Category>
                {
                    new Category { Name = "Category" }
                },
                NamedEnvironment = "DEV",
                NamedEnvironmentType = GlobalEnums.NamedEnvironmentType.NonProd,
                OriginationCode = DatasetOriginationCode.Internal.ToString(),
                IsSecured = true,
                Favorities = new List<Favorite>
                {
                    new Favorite { UserId = "000000" },
                    new Favorite { UserId = "000001" }
                }
            };

            GlobalDataset result = dataset.ToGlobalDataset();

            Assert.AreEqual(1, result.GlobalDatasetId);
            Assert.AreEqual("Name", result.DatasetName);
            Assert.AreEqual("SAID", result.DatasetSaidAssetCode);
            Assert.AreEqual(1, result.EnvironmentDatasets.Count);

            EnvironmentDataset environmentDataset = result.EnvironmentDatasets.First();
            Assert.AreEqual(2, environmentDataset.DatasetId);
            Assert.AreEqual("Description", environmentDataset.DatasetDescription);
            Assert.AreEqual("Category", environmentDataset.CategoryCode);
            Assert.AreEqual("DEV", environmentDataset.NamedEnvironment);
            Assert.AreEqual(NamedEnvironmentType.NonProd.ToString(), environmentDataset.NamedEnvironmentType);
            Assert.AreEqual(DatasetOriginationCode.Internal.ToString(), environmentDataset.OriginationCode);
            Assert.IsTrue(environmentDataset.IsSecured);
            Assert.AreEqual(2, environmentDataset.FavoriteUserIds.Count);
            Assert.AreEqual("000000", environmentDataset.FavoriteUserIds.First());
            Assert.AreEqual("000001", environmentDataset.FavoriteUserIds.Last());
            Assert.IsFalse(environmentDataset.EnvironmentSchemas.Any());
        }

        [TestMethod]
        public void ToEnvironmentDataset_Dataset_EnvironmentDataset()
        {
            Dataset dataset = new Dataset
            {
                DatasetId = 2,
                DatasetDesc = "Description",
                DatasetCategories = new List<Category>
                {
                    new Category { Name = "Category" }
                },
                NamedEnvironment = "DEV",
                NamedEnvironmentType = GlobalEnums.NamedEnvironmentType.NonProd,
                OriginationCode = DatasetOriginationCode.Internal.ToString(),
                IsSecured = true,
                Favorities = new List<Favorite>
                {
                    new Favorite { UserId = "000000" },
                    new Favorite { UserId = "000001" }
                }
            };

            EnvironmentDataset result = dataset.ToEnvironmentDataset();

            Assert.AreEqual(2, result.DatasetId);
            Assert.AreEqual("Description", result.DatasetDescription);
            Assert.AreEqual("Category", result.CategoryCode);
            Assert.AreEqual("DEV", result.NamedEnvironment);
            Assert.AreEqual(NamedEnvironmentType.NonProd.ToString(), result.NamedEnvironmentType);
            Assert.AreEqual(DatasetOriginationCode.Internal.ToString(), result.OriginationCode);
            Assert.IsTrue(result.IsSecured);
            Assert.AreEqual(2, result.FavoriteUserIds.Count);
            Assert.AreEqual("000000", result.FavoriteUserIds.First());
            Assert.AreEqual("000001", result.FavoriteUserIds.Last());
            Assert.IsFalse(result.EnvironmentSchemas.Any());
        }

        [TestMethod]
        public void ToEnvironmentSchema_SchemaResultDto_EnvironmentSchema()
        {
            SchemaResultDto dto = new SchemaResultDto
            {
                SchemaId = 1,
                SchemaName = "Name",
                SchemaDescription = "Description",
                SaidAssetCode = "SAID"
            };

            EnvironmentSchema result = dto.ToEnvironmentSchema();

            Assert.AreEqual(1, result.SchemaId);
            Assert.AreEqual("Name", result.SchemaName);
            Assert.AreEqual("Description", result.SchemaDescription);
            Assert.AreEqual("SAID", result.SchemaSaidAssetCode);
        }

        [TestMethod]
        public void ToEnvironmentSchema_FileSchemaDto_EnvironmentSchema()
        {
            FileSchemaDto dto = new FileSchemaDto
            {
                SchemaId = 1,
                Name = "Name",
                Description = "Description",
            };

            EnvironmentSchema result = dto.ToEnvironmentSchema();

            Assert.AreEqual(1, result.SchemaId);
            Assert.AreEqual("Name", result.SchemaName);
            Assert.AreEqual("Description", result.SchemaDescription);
            Assert.IsNull(result.SchemaSaidAssetCode);
        }

        [TestMethod]
        public void ToEnvironmentSchema_FileSchema_EnvironmentSchema()
        {
            FileSchema schema = new FileSchema
            {
                SchemaId = 1,
                Name = "Name",
                Description = "Description",
            };

            EnvironmentSchema result = schema.ToEnvironmentSchema();

            Assert.AreEqual(1, result.SchemaId);
            Assert.AreEqual("Name", result.SchemaName);
            Assert.AreEqual("Description", result.SchemaDescription);
            Assert.IsNull(result.SchemaSaidAssetCode);
        }

        [TestMethod]
        public void ToGlobalDataset_Datasets_GlobalDataset()
        {
            List<Dataset> datasets = GetDatasets();
            List<KeyValuePair<int, FileSchema>> schemas = GetSchemas();
            List<DataFlow> dataFlows = GetDataFlows();

            GlobalDataset globalDataset = datasets.ToGlobalDataset(schemas, dataFlows);

            Assert.AreEqual(2, globalDataset.GlobalDatasetId);
            Assert.AreEqual("Name 2", globalDataset.DatasetName);
            Assert.AreEqual("SAID", globalDataset.DatasetSaidAssetCode);
            Assert.AreEqual(2, globalDataset.EnvironmentDatasets.Count);

            EnvironmentDataset environmentDataset = globalDataset.EnvironmentDatasets.First();
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
        }

        #region Helpers
        private List<Dataset> GetDatasets()
        {
            return new List<Dataset>
            {
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

        private List<KeyValuePair<int, FileSchema>> GetSchemas()
        {
            return new List<KeyValuePair<int, FileSchema>>
            {
                new KeyValuePair<int, FileSchema>(
                    3,
                    new FileSchema
                    {
                        SchemaId = 6,
                        Name = "Schema Name 1",
                        Description = "Schema Description 1"
                    }
                ),
                new KeyValuePair<int, FileSchema>( 
                    3,
                    new FileSchema
                    {
                        SchemaId = 7,
                        Name = "Schema Name 2",
                        Description = "Schema Description 2"
                    }
                ),
                new KeyValuePair<int, FileSchema>(
                    5,
                    new FileSchema
                    {
                        SchemaId = 8,
                        Name = "Schema Name 1",
                        Description = "Schema Description 1"
                    }
                )
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
                    DatasetId = 5,
                    ObjectStatus = ObjectStatusEnum.Active,
                    SchemaId = 8,
                    SaidKeyCode = "SAID"
                }
            };
        }
        #endregion
    }
}
