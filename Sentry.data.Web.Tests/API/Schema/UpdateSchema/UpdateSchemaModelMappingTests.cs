using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core;
using Sentry.data.Web.API;
using System;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.Tests.API
{
    [TestClass]
    public class UpdateSchemaModelMappingTests : BaseModelMappingTests
    {
        [TestMethod]
        public void Map_UpdateSchemaRequestModel_To_SchemaFlowDto()
        {
            UpdateSchemaRequestModel model = new UpdateSchemaRequestModel
            {
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

            SchemaFlowDto dto = _mapper.Map<SchemaFlowDto>(model);

            Assert.IsNotNull(dto.SchemaDto);
            Assert.IsNotNull(dto.DatasetFileConfigDto);
            Assert.IsNotNull(dto.DataFlowDto);

            FileSchemaDto fileSchemaDto = dto.SchemaDto;
            Assert.AreEqual(0, fileSchemaDto.ParentDatasetId);
            Assert.IsNull(fileSchemaDto.Name);
            Assert.AreEqual("Description", fileSchemaDto.Description);
            Assert.AreEqual(ExtensionNames.CSV, fileSchemaDto.FileExtensionName);
            Assert.IsTrue(fileSchemaDto.CLA1286_KafkaFlag);
            Assert.AreEqual(",", fileSchemaDto.Delimiter);
            Assert.IsTrue(fileSchemaDto.HasHeader);
            Assert.IsTrue(fileSchemaDto.CreateCurrentView);
            Assert.AreEqual("root,path", fileSchemaDto.SchemaRootPath);

            DatasetFileConfigDto fileConfigDto = dto.DatasetFileConfigDto;
            Assert.AreEqual(0, fileConfigDto.ParentDatasetId);
            Assert.IsNull(fileConfigDto.Name);
            Assert.AreEqual("Appending", fileConfigDto.DatasetScopeTypeName);
            Assert.AreEqual("Description", fileConfigDto.Description);
            Assert.AreEqual((int)FileType.DataFile, fileConfigDto.FileTypeId);

            DataFlowDto dataFlowDto = dto.DataFlowDto;
            Assert.IsNull(dataFlowDto.NamedEnvironment);
            Assert.IsNull(dataFlowDto.SaidKeyCode);
            Assert.AreEqual((int)DataFlowPreProcessingTypes.googleapi, dataFlowDto.PreProcessingOption);
            Assert.IsTrue(dataFlowDto.IsPreProcessingRequired);
            Assert.AreEqual((int)CompressionTypes.ZIP, dataFlowDto.CompressionType);
            Assert.IsTrue(dataFlowDto.IsCompressed);
            Assert.AreEqual((int)IngestionType.Topic, dataFlowDto.IngestionType);
            Assert.AreEqual("000001", dataFlowDto.PrimaryContactId);
            Assert.AreEqual("TopicName", dataFlowDto.TopicName);
            Assert.IsNotNull(dataFlowDto.CompressionJob);
            Assert.AreEqual(CompressionTypes.ZIP, dataFlowDto.CompressionJob.CompressionType);
        }

        [TestMethod]
        public void Map_UpdateSchemaRequestModel_To_SchemaFlowDto_Part()
        {
            UpdateSchemaRequestModel model = new UpdateSchemaRequestModel
            {
                SchemaDescription = "Description",
                HasHeader = true,
                ScopeTypeCode = "Appending",
                FileTypeCode = "json",
                CreateCurrentView = true,
                IngestionTypeCode = IngestionType.S3_Drop.ToString(),
                IsCompressed = false,
                IsPreprocessingRequired = false,
                PrimaryContactId = "000001"
            };

            SchemaFlowDto dto = _mapper.Map<SchemaFlowDto>(model);

            Assert.IsNotNull(dto.SchemaDto);
            Assert.IsNotNull(dto.DatasetFileConfigDto);
            Assert.IsNotNull(dto.DataFlowDto);

            FileSchemaDto fileSchemaDto = dto.SchemaDto;
            Assert.AreEqual(0, fileSchemaDto.ParentDatasetId);
            Assert.IsNull(fileSchemaDto.Name);
            Assert.AreEqual("Description", fileSchemaDto.Description);
            Assert.AreEqual(ExtensionNames.JSON, fileSchemaDto.FileExtensionName);
            Assert.IsFalse(fileSchemaDto.CLA1286_KafkaFlag);
            Assert.IsNull(fileSchemaDto.Delimiter);
            Assert.IsTrue(fileSchemaDto.HasHeader);
            Assert.IsTrue(fileSchemaDto.CreateCurrentView);
            Assert.IsNull(fileSchemaDto.SchemaRootPath);

            DatasetFileConfigDto fileConfigDto = dto.DatasetFileConfigDto;
            Assert.AreEqual(0, fileConfigDto.ParentDatasetId);
            Assert.IsNull(fileConfigDto.Name);
            Assert.AreEqual("Appending", fileConfigDto.DatasetScopeTypeName);
            Assert.AreEqual("Description", fileConfigDto.Description);
            Assert.AreEqual((int)FileType.DataFile, fileConfigDto.FileTypeId);

            DataFlowDto dataFlowDto = dto.DataFlowDto;
            Assert.IsNull(dataFlowDto.NamedEnvironment);
            Assert.IsNull(dataFlowDto.SaidKeyCode);
            Assert.AreEqual(0, dataFlowDto.PreProcessingOption);
            Assert.IsFalse(dataFlowDto.IsPreProcessingRequired);
            Assert.IsNull(dataFlowDto.CompressionType);
            Assert.IsFalse(dataFlowDto.IsCompressed);
            Assert.AreEqual((int)IngestionType.S3_Drop, dataFlowDto.IngestionType);
            Assert.AreEqual("000001", dataFlowDto.PrimaryContactId);
            Assert.IsNull(dataFlowDto.TopicName);
            Assert.IsNull(dataFlowDto.CompressionJob);
        }


        [TestMethod]
        public void Map_SchemaResultDto_To_UpdateSchemaResponseModel()
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

            UpdateSchemaResponseModel model = _mapper.Map<UpdateSchemaResponseModel>(dto);

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
