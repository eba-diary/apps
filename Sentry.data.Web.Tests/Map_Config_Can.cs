using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Web.Helpers;
using Sentry.data.Web.Models.ApiModels.Schema;
using System;
using System.Collections.Generic;

namespace Sentry.data.Web.Tests
{
    [TestClass]
    public class Map_Config_Can
    {
        [TestMethod]
        public void Map_From_SchemaInfoModel_To_FileSchemaDto()
        {
            SchemaInfoModel mdl = new SchemaInfoModel()
            {
                ConfigId = 1,
                SchemaId = 1,
                SchemaEntity_NME = "EntityName",
                Name = "SchemaName",
                Description = "Schema Description",
                StorageCode = "SC",
                Format = "csv",
                CurrentView = true,
                Delimiter = ",",
                HasHeader = false,
                HiveTable = "HiveTable",
                HiveDatabase = "HiveDB",
                HiveLocation = "HiveLocation",
                HiveTableStatus = "HiveStatus",
                Options = new List<string>() { "CLA1286_KafkaFlag|true", "CLA1396_NewEtlColumns|false", "CLA3014_LoadDataToSnowflake|true" },
                DeleteInd = false,
                SnowflakeDatabase = "SFDB",
                SnowflakeSchema = "SFSchema",
                SnowflakeTable = "SFTable",
                SnowflakeStage = "SFStage",
                SnowflakeStatus = "SFStatus",
                SnowflakeWarehouse = "SFWarehouse",
                ObjectStatus = "ACTIVE",
                SchemaRootPath = new string[] { "start", "middle", "end" },
                ParquetStorageBucket = "PSB",
                ParquetStoragePrefix = "PSP"
            };

            FileSchemaDto dto = MapConfig.Mapper.Map<FileSchemaDto>(mdl);

            Assert.AreEqual(1, dto.SchemaId);
            Assert.AreEqual("EntityName", dto.SchemaEntity_NME);
            Assert.AreEqual("SchemaName", dto.Name);
            Assert.AreEqual("Schema Description", dto.Description);
            Assert.AreEqual(0, dto.ParentDatasetId);
            Assert.AreEqual(ObjectStatusEnum.Active, dto.ObjectStatus);
            Assert.AreEqual(false, dto.DeleteInd);
            Assert.AreEqual(null, dto.DeleteIssuer);
            Assert.AreEqual(DateTime.MinValue, dto.DeleteIssueDTM);
            Assert.AreEqual(false, dto.CLA1396_NewEtlColumns);
            Assert.AreEqual(true, dto.CLA1286_KafkaFlag);
            Assert.AreEqual(true, dto.CLA3014_LoadDataToSnowflake);
            Assert.AreEqual(false, dto.CLA1580_StructureHive);
            Assert.AreEqual(false, dto.CLA2472_EMRSend);
            Assert.AreEqual(0, dto.FileExtensionId);
            Assert.AreEqual(",", dto.Delimiter);
            Assert.AreEqual(false, dto.HasHeader);
            Assert.AreEqual(true, dto.CreateCurrentView);
            Assert.AreEqual("HiveTable", dto.HiveTable);
            Assert.AreEqual("HiveDB", dto.HiveDatabase);
            Assert.AreEqual("HiveLocation", dto.HiveLocation);
            Assert.AreEqual("HiveStatus", dto.HiveStatus);
            Assert.AreEqual("SC", dto.StorageCode);
            Assert.AreEqual(null, dto.StorageLocation);
            Assert.AreEqual(null, dto.RawQueryStorage);
            Assert.AreEqual("SFDB", dto.SnowflakeDatabase);
            Assert.AreEqual("SFSchema", dto.SnowflakeSchema);
            Assert.AreEqual("SFTable", dto.SnowflakeTable);
            Assert.AreEqual("SFStage", dto.SnowflakeStage);
            Assert.AreEqual("SFStatus", dto.SnowflakeStatus);
            Assert.AreEqual("SFWarehouse", dto.SnowflakeWarehouse);
            Assert.AreEqual("start,middle,end", dto.SchemaRootPath);
            Assert.AreEqual("PSB", dto.ParquetStorageBucket);
            Assert.AreEqual("PSP", dto.ParquetStoragePrefix);
        }
    }
}
