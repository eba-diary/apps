using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class DatasetExtensionsTests
    {
        [TestMethod]
        public void ToDatasetTileDto_Dataset_DatasetTileDto()
        {
            Dataset dataset = new Dataset()
            {
                DatasetId = 1,
                DatasetName = "DatasetName",
                DatasetDesc = "Description",
                ObjectStatus = ObjectStatusEnum.Active,
                OriginationCode = "Origin",
                NamedEnvironment = "DEV",
                NamedEnvironmentType = NamedEnvironmentType.NonProd,
                IsSecured = true,
                DatasetDtm = new DateTime(2022, 8, 19, 8, 0, 0),
                DatasetCategories = new List<Category>()
                {
                    new Category()
                    {
                        Name = "Category",
                        Color = "Blue"
                    }
                },
                Asset = new Asset() { SaidKeyCode = "SAID" }
            };

            DatasetTileDto dto = dataset.ToDatasetTileDto();

            Assert.AreEqual("1", dto.Id);
            Assert.AreEqual("DatasetName", dto.Name);
            Assert.AreEqual("Description", dto.Description);
            Assert.AreEqual(ObjectStatusEnum.Active, dto.Status);
            Assert.IsTrue(dto.IsSecured);
            Assert.AreEqual("Blue", dto.Color);
            Assert.AreEqual("Category", dto.Category);
            Assert.AreEqual("Origin", dto.OriginationCode);
            Assert.AreEqual("DEV", dto.Environment);
            Assert.AreEqual("NonProd", dto.EnvironmentType);
            Assert.AreEqual("SAID", dto.DatasetAsset);
            Assert.AreEqual(0, dto.ProducerAssets.Count);
        }

        [TestMethod]
        public void ToDatasetResultDto_Dataset_DatasetResultDto()
        {
            Dataset dataset = new Dataset()
            {
                DatasetId = 1,
                DatasetName = "DatasetName",
                DatasetDesc = "Description",
                DatasetCategories = new List<Category>()
                {
                    new Category()
                    {
                        Name = "Category",
                        Color = "Blue"
                    }
                },
                ShortName = "Short",
                Asset = new Asset() { SaidKeyCode = "SAID" },
                NamedEnvironment = "DEV",
                NamedEnvironmentType = NamedEnvironmentType.NonProd,
                DatasetInformation = "Usage",
                DataClassification = DataClassificationType.Public,
                IsSecured = true,
                PrimaryContactId = "000001",
                AlternateContactEmail = "me@sentry.com",
                OriginationCode = DatasetOriginationCode.External.ToString(),
                CreationUserName = "Creator",
                DatasetDtm = new DateTime(2022, 8, 19, 8, 0, 0),
                ChangedDtm = new DateTime(2022, 8, 20, 8, 0, 0),
                ObjectStatus = ObjectStatusEnum.Active
            };

            DatasetResultDto dto = dataset.ToDatasetResultDto();

            Assert.AreEqual(1, dto.DatasetId);
            Assert.AreEqual("DatasetName", dto.DatasetName);
            Assert.AreEqual("Description", dto.DatasetDescription);
            Assert.AreEqual("Category", dto.CategoryName);
            Assert.AreEqual("Short", dto.ShortName);
            Assert.AreEqual("SAID", dto.SaidAssetCode);
            Assert.AreEqual("DEV", dto.NamedEnvironment);
            Assert.AreEqual(NamedEnvironmentType.NonProd, dto.NamedEnvironmentType);
            Assert.AreEqual("Usage", dto.UsageInformation);
            Assert.AreEqual(DataClassificationType.Public, dto.DataClassificationType);
            Assert.IsTrue(dto.IsSecured);
            Assert.AreEqual("000001", dto.PrimaryContactId);
            Assert.AreEqual("me@sentry.com", dto.AlternateContactEmail);
            Assert.AreEqual(DatasetOriginationCode.External, dto.OriginationCode);
            Assert.AreEqual("Creator", dto.OriginalCreator);
            Assert.AreEqual(new DateTime(2022, 8, 19, 8, 0, 0), dto.CreateDateTime);
            Assert.AreEqual(new DateTime(2022, 8, 20, 8, 0, 0), dto.UpdateDateTime);
            Assert.AreEqual(ObjectStatusEnum.Active, dto.ObjectStatus);
        }
    }
}
