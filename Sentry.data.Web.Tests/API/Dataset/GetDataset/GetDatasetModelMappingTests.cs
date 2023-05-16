using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core;
using Sentry.data.Web.API;
using System;

namespace Sentry.data.Web.Tests.API
{
    [TestClass]
    public class GetDatasetModelMappingTests : BaseModelMappingTests
    {
        [TestMethod]
        public void Map_DatasetResultDto_To_AddDatasetResponseModel()
        {
            DateTime now = DateTime.Now;

            DatasetResultDto dto = new DatasetResultDto
            {
                DatasetId = 1,
                CreateDateTime = now.AddDays(-1),
                UpdateDateTime = now,
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
            Assert.AreEqual(now.AddDays(-1), model.CreateDateTime);
            Assert.AreEqual(now, model.UpdateDateTime);
            Assert.AreEqual(ObjectStatusEnum.Disabled.ToString(), model.ObjectStatusCode);
            Assert.AreEqual("Name", model.DatasetName);
            Assert.AreEqual("Category", model.CategoryCode);
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
