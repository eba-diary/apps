using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Web.API;
using System;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.Tests.API
{
    [TestClass]
    public class AddSchemaModelMappingTests : BaseModelMappingTests
    {
        [TestMethod]
        public void Map_AddSchemaRequestModel_To_AddSchemaDto_Full()
        {
            AddSchemaRequestModel model = new AddSchemaRequestModel
            {
                DatasetId = 1,
                SchemaName = "Name",
                SaidAssetCode = "SAID",
                NamedEnvironment = "DEV",
                NamedEnvironmentTypeCode = NamedEnvironmentType.NonProd.ToString(),
                SchemaDescription = "Description",
                Delimiter = ",",
                HasHeader = true,
                ScopeTypeCode = "Appending",
                FileTypeCode = "csv",
                SchemaRootPath = "root,path",
                CreateCurrentView = true,
                IngestionTypeCode = IngestionType.Topic.ToString(),
                IsCompressed = true,
                CompressionTypeCode = CompressionTypes.ZIP.ToString(),
                IsPreprocessingRequired = true,
                PreprocessingTypeCode = DataFlowPreProcessingTypes.googleapi.ToString(),
                KafkaTopicName = "TopicName",
                PrimaryContactId = "000001"
            };

            AddSchemaDto dto = _mapper.Map<AddSchemaDto>(model);

            Assert.IsNotNull(dto.SchemaDto);
            Assert.IsNotNull(dto.DatasetFileConfigDto);
            Assert.IsNotNull(dto.DataFlowDto);

            FileSchemaDto fileSchemaDto = dto.SchemaDto;
            Assert.AreEqual(1, fileSchemaDto.ParentDatasetId);
            Assert.AreEqual("Name", fileSchemaDto.Name);
            Assert.AreEqual(ObjectStatusEnum.Active, fileSchemaDto.ObjectStatus);
            Assert.AreEqual("Description", fileSchemaDto.Description);
            Assert.AreEqual(ExtensionNames.CSV, fileSchemaDto.FileExtensionName);
            Assert.IsTrue(fileSchemaDto.CLA1286_KafkaFlag);
            Assert.AreEqual(",", fileSchemaDto.Delimiter);
            Assert.IsTrue(fileSchemaDto.HasHeader);
            Assert.IsTrue(fileSchemaDto.CreateCurrentView);
            Assert.AreEqual("root,path", fileSchemaDto.SchemaRootPath);

            DatasetFileConfigDto fileConfigDto = dto.DatasetFileConfigDto;
            Assert.AreEqual(1, fileConfigDto.ParentDatasetId);
            Assert.AreEqual("Name", fileConfigDto.Name);
            Assert.AreEqual(ObjectStatusEnum.Active, fileConfigDto.ObjectStatus);
            Assert.AreEqual("Appending", fileConfigDto.DatasetScopeTypeName);
            Assert.AreEqual("Description", fileConfigDto.Description);

            DataFlowDto dataFlowDto = dto.DataFlowDto;
            Assert.AreEqual(ObjectStatusEnum.Active, dataFlowDto.ObjectStatus);
            Assert.AreEqual(NamedEnvironmentType.NonProd, dataFlowDto.NamedEnvironmentType);
            Assert.AreEqual("DEV", dataFlowDto.NamedEnvironment);
            Assert.AreEqual("SAID", dataFlowDto.SaidKeyCode);
            Assert.AreEqual((int)DataFlowPreProcessingTypes.googleapi, dataFlowDto.PreProcessingOption);
            Assert.IsTrue(dataFlowDto.IsPreProcessingRequired);
            Assert.AreEqual((int)CompressionTypes.ZIP, dataFlowDto.CompressionType);
            Assert.IsTrue(dataFlowDto.IsCompressed);
            Assert.AreEqual((int)IngestionType.Topic, dataFlowDto.IngestionType);
            Assert.AreEqual("000001", dataFlowDto.PrimaryContactId);
            Assert.AreEqual("TopicName", dataFlowDto.TopicName);
        }

        [TestMethod]
        public void Map_AddSchemaRequestModel_To_AddSchemaDto_Part()
        {
            AddSchemaRequestModel model = new AddSchemaRequestModel
            {
                DatasetId = 1,
                SchemaName = "Name",
                SaidAssetCode = "SAID",
                NamedEnvironment = "DEV",
                NamedEnvironmentTypeCode = NamedEnvironmentType.NonProd.ToString(),
                SchemaDescription = "Description",
                HasHeader = true,
                ScopeTypeCode = "Appending",
                FileTypeCode = ExtensionNames.JSON,
                CreateCurrentView = true,
                IngestionTypeCode = IngestionType.S3_Drop.ToString(),
                IsCompressed = false,
                IsPreprocessingRequired = false,
                PrimaryContactId = "000001"
            };

            AddSchemaDto dto = _mapper.Map<AddSchemaDto>(model);

            Assert.IsNotNull(dto.SchemaDto);
            Assert.IsNotNull(dto.DatasetFileConfigDto);
            Assert.IsNotNull(dto.DataFlowDto);

            FileSchemaDto fileSchemaDto = dto.SchemaDto;
            Assert.AreEqual(1, fileSchemaDto.ParentDatasetId);
            Assert.AreEqual("Name", fileSchemaDto.Name);
            Assert.AreEqual(ObjectStatusEnum.Active, fileSchemaDto.ObjectStatus);
            Assert.AreEqual("Description", fileSchemaDto.Description);
            Assert.AreEqual(ExtensionNames.JSON, fileSchemaDto.FileExtensionName);
            Assert.IsFalse(fileSchemaDto.CLA1286_KafkaFlag);
            Assert.IsNull(fileSchemaDto.Delimiter);
            Assert.IsTrue(fileSchemaDto.HasHeader);
            Assert.IsTrue(fileSchemaDto.CreateCurrentView);
            Assert.IsNull(fileSchemaDto.SchemaRootPath);

            DatasetFileConfigDto fileConfigDto = dto.DatasetFileConfigDto;
            Assert.AreEqual(1, fileConfigDto.ParentDatasetId);
            Assert.AreEqual("Name", fileConfigDto.Name);
            Assert.AreEqual(ObjectStatusEnum.Active, fileConfigDto.ObjectStatus);
            Assert.AreEqual("Appending", fileConfigDto.DatasetScopeTypeName);
            Assert.AreEqual("Description", fileConfigDto.Description);

            DataFlowDto dataFlowDto = dto.DataFlowDto;
            Assert.AreEqual(ObjectStatusEnum.Active, dataFlowDto.ObjectStatus);
            Assert.AreEqual(NamedEnvironmentType.NonProd, dataFlowDto.NamedEnvironmentType);
            Assert.AreEqual("DEV", dataFlowDto.NamedEnvironment);
            Assert.AreEqual("SAID", dataFlowDto.SaidKeyCode);
            Assert.IsNull(dataFlowDto.PreProcessingOption);
            Assert.IsFalse(dataFlowDto.IsPreProcessingRequired);
            Assert.IsNull(dataFlowDto.CompressionType);
            Assert.IsFalse(dataFlowDto.IsCompressed);
            Assert.AreEqual((int)IngestionType.S3_Drop, dataFlowDto.IngestionType);
            Assert.AreEqual("000001", dataFlowDto.PrimaryContactId);
            Assert.IsNull(dataFlowDto.TopicName);
        }

        [TestMethod]
        public void Map_SchemaResultDto_To_AddSchemaResponseModel()
        {
            SchemaResultDto dto = new SchemaResultDto
            {
                SchemaId = 1,
                DatasetId = 2,
                SchemaName = "Name",
                SaidAssetCode = "SAID",
                NamedEnvironment = "DEV",
                NamedEnvironmentType = NamedEnvironmentType.NonProd,
                SchemaDescription = "Description",
                Delimiter = ",",
                HasHeader = true,
                ScopeTypeCode = "Appending",
                FileTypeCode = ExtensionNames.CSV,
                SchemaRootPath = "root,path",
                CreateCurrentView = true,
                IngestionType = IngestionType.Topic,
                IsCompressed = true,
                CompressionTypeCode = CompressionTypes.ZIP.ToString(),
                IsPreprocessingRequired = true,
                PreprocessingTypeCode = DataFlowPreProcessingTypes.googleapi.ToString(),
                KafkaTopicName = "TopicName",
                PrimaryContactId = "000001",
                StorageCode = "1234567",
                DropLocation = "Drop/Location",
                ControlMTriggerName = "TriggerName"
            };

            AddSchemaResponseModel model = _mapper.Map<AddSchemaResponseModel>(dto);

            Assert.AreEqual(1, model.SchemaId);
            Assert.AreEqual(2, model.DatasetId);
            Assert.AreEqual("Name", model.SchemaName);
            Assert.AreEqual("SAID", model.SaidAssetCode);
            Assert.AreEqual("DEV", model.NamedEnvironment);
            Assert.AreEqual(NamedEnvironmentType.NonProd.ToString(), model.NamedEnvironmentTypeCode);
            Assert.AreEqual("Description", model.SchemaDescription);
            Assert.AreEqual(",", model.Delimiter);
            Assert.IsTrue(model.HasHeader);
            Assert.AreEqual("Appending", model.ScopeTypeCode);
            Assert.AreEqual(ExtensionNames.CSV, model.FileTypeCode);
            Assert.AreEqual("root,path", model.SchemaRootPath);
            Assert.IsTrue(model.CreateCurrentView);
            Assert.AreEqual(IngestionType.Topic.ToString(), model.IngestionTypeCode);
            Assert.IsTrue(model.IsCompressed);
            Assert.IsTrue(model.IsPreprocessingRequired);
            Assert.AreEqual(CompressionTypes.ZIP.ToString(), model.CompressionTypeCode);
            Assert.AreEqual(DataFlowPreProcessingTypes.googleapi.ToString(), model.PreprocessingTypeCode);
            Assert.AreEqual("TopicName", model.KafkaTopicName);
            Assert.AreEqual("000001", model.PrimaryContactId);
            Assert.AreEqual("1234567", model.StorageCode);
            Assert.AreEqual("Drop/Location", model.DropLocation);
            Assert.AreEqual("TriggerName", model.ControlMTriggerName);
        }
    }
}
