using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Core.GlobalEnums;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class GlobalDatasetExtensions
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
    }
}
