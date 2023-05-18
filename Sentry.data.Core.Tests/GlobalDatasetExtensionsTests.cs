using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nest;
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

        [TestMethod]
        public void ToSearchResult_GlobalDataset_SearchGlobalDatasetResultDto_ProdEnvironment()
        {
            List<GlobalDataset> globalDatasets = new List<GlobalDataset>
            {
                new GlobalDataset
                {
                    GlobalDatasetId = 1,
                    DatasetName = "Name",
                    DatasetSaidAssetCode = "SAID",
                    EnvironmentDatasets = new List<EnvironmentDataset>
                    {
                        new EnvironmentDataset
                        {
                            DatasetId = 11,
                            DatasetDescription = "Description",
                            CategoryCode = "Category",
                            NamedEnvironment = "DEV",
                            NamedEnvironmentType = NamedEnvironmentType.NonProd.ToString(),
                            OriginationCode = DatasetOriginationCode.Internal.ToString(),
                            IsSecured = true,
                            FavoriteUserIds = new List<string> { "082116" }
                        },
                        new EnvironmentDataset
                        {
                            DatasetId = 12,
                            DatasetDescription = "Description - Prod",
                            CategoryCode = "Category",
                            NamedEnvironment = "PROD",
                            NamedEnvironmentType = NamedEnvironmentType.Prod.ToString(),
                            OriginationCode = DatasetOriginationCode.Internal.ToString(),
                            IsSecured = true,
                            FavoriteUserIds = new List<string>()
                        },
                        new EnvironmentDataset
                        {
                            DatasetId = 13,
                            DatasetDescription = "Description",
                            CategoryCode = "Category",
                            NamedEnvironment = "TEST",
                            NamedEnvironmentType = NamedEnvironmentType.NonProd.ToString(),
                            OriginationCode = DatasetOriginationCode.Internal.ToString(),
                            IsSecured = true,
                            FavoriteUserIds = new List<string>()
                        }
                    },
                    SearchHighlights = new List<SearchHighlight>
                    {
                        new SearchHighlight
                        {
                            PropertyName = "DatasetName",
                            Highlights = new List<string> { "Name" }
                        },
                        new SearchHighlight
                        {
                            PropertyName = "DatasetDescription",
                            Highlights = new List<string> { "Description", "Description - Prod" }
                        }
                    }
                },
                new GlobalDataset
                {
                    GlobalDatasetId = 2,
                    DatasetName = "Name 2",
                    DatasetSaidAssetCode = "DATA",
                    EnvironmentDatasets = new List<EnvironmentDataset>
                    {
                        new EnvironmentDataset
                        {
                            DatasetId = 14,
                            DatasetDescription = "Description 2",
                            CategoryCode = "Category 2",
                            NamedEnvironment = "QUAL",
                            NamedEnvironmentType = NamedEnvironmentType.NonProd.ToString(),
                            OriginationCode = DatasetOriginationCode.External.ToString(),
                            IsSecured = false,
                            FavoriteUserIds = new List<string>()
                        },
                        new EnvironmentDataset
                        {
                            DatasetId = 15,
                            DatasetDescription = "Description 2",
                            CategoryCode = "Category 2",
                            NamedEnvironment = "TEST",
                            NamedEnvironmentType = NamedEnvironmentType.NonProd.ToString(),
                            OriginationCode = DatasetOriginationCode.External.ToString(),
                            IsSecured = false,
                            FavoriteUserIds = new List<string>()
                        }
                    },
                    SearchHighlights = new List<SearchHighlight>
                    {
                        new SearchHighlight
                        {
                            PropertyName = "DatasetDescription",
                            Highlights = new List<string> { "Description 2"}
                        }
                    }
                }
            };

            List<SearchGlobalDatasetDto> results = globalDatasets.ToSearchResults("082116");

            Assert.AreEqual(2, results.Count);

            SearchGlobalDatasetDto result = results.First();

            Assert.AreEqual(1, result.GlobalDatasetId);
            Assert.AreEqual("Name", result.DatasetName);
            Assert.AreEqual("SAID", result.DatasetSaidAssetCode);
            Assert.AreEqual("Description - Prod", result.DatasetDescription);
            Assert.AreEqual("Category", result.CategoryCode);
            Assert.AreEqual(3, result.NamedEnvironments.Count);
            Assert.AreEqual("PROD", result.NamedEnvironments[0]);
            Assert.AreEqual("TEST", result.NamedEnvironments[1]);
            Assert.AreEqual("DEV", result.NamedEnvironments[2]);
            Assert.IsTrue(result.IsSecured);
            Assert.IsTrue(result.IsFavorite);
            Assert.AreEqual(12, result.TargetDatasetId);
            Assert.AreEqual(2, result.SearchHighlights.Count);

            SearchHighlightDto highlight = result.SearchHighlights.First();
            Assert.AreEqual("DatasetName", highlight.PropertyName);
            Assert.AreEqual(1, highlight.Highlights.Count);
            Assert.AreEqual("Name", highlight.Highlights.First());

            highlight = result.SearchHighlights.Last();
            Assert.AreEqual("DatasetDescription", highlight.PropertyName);
            Assert.AreEqual(2, highlight.Highlights.Count);
            Assert.AreEqual("Description", highlight.Highlights.First());
            Assert.AreEqual("Description - Prod", highlight.Highlights.Last());

            result = results.Last();

            Assert.AreEqual(2, result.GlobalDatasetId);
            Assert.AreEqual("Name 2", result.DatasetName);
            Assert.AreEqual("DATA", result.DatasetSaidAssetCode);
            Assert.AreEqual("Description 2", result.DatasetDescription);
            Assert.AreEqual("Category 2", result.CategoryCode);
            Assert.AreEqual(2, result.NamedEnvironments.Count);
            Assert.AreEqual("TEST", result.NamedEnvironments[0]);
            Assert.AreEqual("QUAL", result.NamedEnvironments[1]);
            Assert.IsFalse(result.IsSecured);
            Assert.IsFalse(result.IsFavorite);
            Assert.AreEqual(15, result.TargetDatasetId);
            Assert.AreEqual(1, result.SearchHighlights.Count);

            highlight = result.SearchHighlights.First();
            Assert.AreEqual("DatasetDescription", highlight.PropertyName);
            Assert.AreEqual(1, highlight.Highlights.Count);
            Assert.AreEqual("Description 2", highlight.Highlights.First());
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
