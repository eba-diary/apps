using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Web.Tests
{
    [TestClass]
    public class DatasetExtensionsTests
    {
        [TestMethod]
        public void ToModel_DatasetFileConfigSchemaDto_DatasetFileConfigSchemaModel()
        {
            DatasetFileConfigSchemaDto dto = new DatasetFileConfigSchemaDto()
            {
                ConfigId = 1,
                SchemaId = 2,
                SchemaName = "SchemaName"
            };

            DatasetFileConfigSchemaModel model = dto.ToModel();

            Assert.AreEqual(1, model.ConfigId);
            Assert.AreEqual(2, model.SchemaId);
            Assert.AreEqual("SchemaName", model.SchemaName);
        }

        [TestMethod]
        public void ToDto_DatasetModel_DatasetSchemaDto()
        {
            DatasetModel model = new DatasetModel()
            {
                DatasetId = 1,
                DatasetCategoryIds = new List<int> { 2 },
                DatasetName = "Dataset Name",
                ShortName = "ShortName",
                DatasetDesc = "Description",
                DatasetInformation = "Information",
                PrimaryContactId = "000000",
                PrimaryContactName = "Contact Name",
                AlternateContactEmail = "alt@email.com",
                CreationUserId = "000001",
                UploadUserId = "000002",
                OriginationID = 3,
                ConfigFileName = "Filename",
                ConfigFileDesc = "File description",
                FileExtensionId = 4,
                Delimiter = ",",
                SchemaRootPath = "Root.Path",
                DatasetScopeTypeId = 5,
                DataClassification = DataClassificationType.Public,
                IsSecured = true,
                HasHeader = true,
                CreateCurrentView = true,
                ObjectStatus = ObjectStatusEnum.Active,
                SAIDAssetKeyCode = "SAID",
                DatasetNamedEnvironment = "DEV",
                DatasetNamedEnvironmentType = NamedEnvironmentType.NonProd
            };

            DatasetSchemaDto dto = model.ToDto();

            Assert.AreEqual(1, dto.DatasetId);
            Assert.AreEqual(1, dto.DatasetCategoryIds.Count);
            Assert.AreEqual(2, dto.DatasetCategoryIds.First());
            Assert.AreEqual("Dataset Name", dto.DatasetName);
            Assert.AreEqual("ShortName", dto.ShortName);
            Assert.AreEqual("Description", dto.DatasetDesc);
            Assert.AreEqual("Information", dto.DatasetInformation);
            Assert.AreEqual("000000", dto.PrimaryContactId);
            Assert.AreEqual("Contact Name", dto.PrimaryContactName);
            Assert.AreEqual("alt@email.com", dto.AlternateContactEmail);
            Assert.AreEqual("000001", dto.CreationUserId);
            Assert.AreEqual("000002", dto.UploadUserId);
            Assert.AreEqual(3, dto.OriginationId);
            Assert.AreEqual("Filename", dto.ConfigFileName);
            Assert.AreEqual("File description", dto.ConfigFileDesc);
            Assert.AreEqual(4, dto.FileExtensionId);
            Assert.AreEqual(",", dto.Delimiter);
            Assert.AreEqual("Root.Path", dto.SchemaRootPath);
            Assert.AreEqual(5, dto.DatasetScopeTypeId);
            Assert.AreEqual(DataClassificationType.Public, dto.DataClassification);
            Assert.AreEqual(ObjectStatusEnum.Active, dto.ObjectStatus);
            Assert.AreEqual("SAID", dto.SAIDAssetKeyCode);
            Assert.AreEqual("DEV", dto.NamedEnvironment);
            Assert.AreEqual(NamedEnvironmentType.NonProd, dto.NamedEnvironmentType);
            Assert.IsTrue(dto.IsSecured);
            Assert.IsTrue(dto.HasHeader);
            Assert.IsTrue(dto.CreateCurrentView);
        }
    }
}
