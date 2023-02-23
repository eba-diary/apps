using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core;
using Sentry.data.Web.API;
using System;

namespace Sentry.data.Web.Tests.API
{
    [TestClass]
    public class AddDatasetModelMappingTests : BaseModelMappingsTests
    {
        [TestMethod]
        public void Map_AddDatasetRequestModel_To_DatasetDto()
        {
            AddDatasetRequestModel model = new AddDatasetRequestModel
            {
                DatasetName = "Name",
                CategoryName = "Category",
                ShortName = "Short",
                SaidAssetCode = "said",
                NamedEnvironment = "DEV",
                NamedEnvironmentTypeCode = NamedEnvironmentType.NonProd.ToString(),
                DatasetDescription = "Description",
                UsageInformation = "Usage",
                DataClassificationTypeCode = DataClassificationType.Public.ToString(),
                IsSecured = true,
                PrimaryContactId = "000001",
                AlternateContactEmail = "me@sentry.com",
                OriginationCode = DatasetOriginationCode.External.ToString(),
                OriginalCreator = "Creator"
            };

            DatasetDto dto = _mapper.Map<DatasetDto>(model);

            Assert.AreEqual("Name", dto.DatasetName);
            Assert.AreEqual("Category", dto.CategoryName);
            Assert.AreEqual("Short", dto.ShortName);
            Assert.AreEqual("SAID", dto.SAIDAssetKeyCode);
            Assert.AreEqual("DEV", dto.NamedEnvironment);
            Assert.AreEqual(NamedEnvironmentType.NonProd, dto.NamedEnvironmentType);
            Assert.AreEqual("Description", dto.DatasetDesc);
            Assert.AreEqual("Usage", dto.DatasetInformation);
            Assert.AreEqual(DataClassificationType.Public, dto.DataClassification);
            Assert.IsTrue(dto.IsSecured);
            Assert.AreEqual("000001", dto.PrimaryContactId);
            Assert.AreEqual("me@sentry.com", dto.AlternateContactEmail);
            Assert.AreEqual((int)DatasetOriginationCode.External, dto.OriginationId);
            Assert.AreEqual("Creator", dto.CreationUserId);
            Assert.IsTrue(dto.DatasetDtm >= DateTime.Now);
            Assert.IsTrue(dto.ChangedDtm >= DateTime.Now);
        }

        [TestMethod]
        public void Map_DatasetResultDto_To_AddDatasetResponseModel()
        {
            DateTime now = DateTime.Now;

            DatasetResultDto dto = new DatasetResultDto
            {
                DatasetId = 1,
                CreatedDateTime = now.AddDays(-1),
                UpdatedDateTime = now,
                ObjectStatus = ObjectStatusEnum.Disabled,
                DatasetName = "Name",
                CategoryName = "Category",
                ShortName = "Short",
                SaidAssetCode = "SAID",
                NamedEnvironment = "DEV",
                NamedEnvironmentType = NamedEnvironmentType.NonProd,
                DatasetDescription = "Description",
                UsageInformation = "Usage",
                DataClassificationType = DataClassificationType.Public,
                IsSecured = true,
                PrimaryContactId = "000001",
                AlternateContactEmail = "me@sentry.com",
                OriginationCode = DatasetOriginationCode.External,
                OriginalCreator = "Creator"
            };

            AddDatasetResponseModel model = _mapper.Map<AddDatasetResponseModel>(dto);

            Assert.AreEqual(1, model.DatasetId);
            Assert.AreEqual(now.AddDays(-1), model.CreatedDateTime);
            Assert.AreEqual(now, model.UpdatedDateTime);
            Assert.AreEqual(ObjectStatusEnum.Disabled.ToString(), model.ObjectStatusCode);
            Assert.AreEqual("Name", model.DatasetName);
            Assert.AreEqual("Category", model.CategoryName);
            Assert.AreEqual("Short", model.ShortName);
            Assert.AreEqual("SAID", model.SaidAssetCode);
            Assert.AreEqual("DEV", model.NamedEnvironment);
            Assert.AreEqual(NamedEnvironmentType.NonProd.ToString(), model.NamedEnvironmentTypeCode);
            Assert.AreEqual("Description", model.DatasetDescription);
            Assert.AreEqual("Usage", model.UsageInformation);
            Assert.AreEqual(DataClassificationType.Public.ToString(), model.DataClassificationTypeCode);
            Assert.IsTrue(model.IsSecured);
            Assert.AreEqual("000001", model.PrimaryContactId);
            Assert.AreEqual("me@sentry.com", model.AlternateContactEmail);
            Assert.AreEqual(DatasetOriginationCode.External.ToString(), model.OriginationCode);
            Assert.AreEqual("Creator", model.OriginalCreator);
        }
    }
}
