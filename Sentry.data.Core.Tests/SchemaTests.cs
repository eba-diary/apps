using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class SchemaTests
    {
        [TestMethod]
        public void AddRevision()
        {
            //Arrange
            FileSchema schema = new FileSchema();
            SchemaRevision revision = new SchemaRevision();

            //Act
            schema.AddRevision(revision);

            //Assert
            Assert.AreEqual(1, schema.Revisions.Count);          
        }

        [TestMethod]
        public void AddOrUpdateSnowflakeConsumptionLayer_New_Item()
        {
            //Arrange
            FileSchema schema = new FileSchema();
            SchemaConsumptionSnowflake schemaConsumptionSnowflake = new SchemaConsumptionSnowflake()
            {
                SnowflakeType = SnowflakeConsumptionType.DatasetSchemaParquet
            };

            //Act
            schema.AddOrUpdateSnowflakeConsumptionLayer(schemaConsumptionSnowflake);

            //Assert
            Assert.AreEqual(1,schema.ConsumptionDetails.Count);
        }

        [TestMethod]
        public void AddOrUpdateSnowflakeConsumptionLayer_Update_Item()
        {
            //Arrange
            FileSchema schema = new FileSchema();
            SchemaConsumptionSnowflake schemaConsumptionSnowflake_Parquet = new SchemaConsumptionSnowflake()
            {
                SchemaConsumptionId = 1,
                SnowflakeWarehouse = "Warehouse_Parquet",
                SnowflakeStage = "Stage_Parquet",
                SnowflakeDatabase = "Database_Parquet",
                SnowflakeSchema = "Schema_Parquet",
                SnowflakeTable = "Table_Parquet",
                SnowflakeStatus = "Available_Parquet",
                SnowflakeType = SnowflakeConsumptionType.DatasetSchemaParquet
            };
            SchemaConsumptionSnowflake schemaConsumptionSnowflake_Raw = new SchemaConsumptionSnowflake()
            {
                SchemaConsumptionId = 2,
                SnowflakeWarehouse = "Warehouse_Raw",
                SnowflakeStage = "Stage_Raw",
                SnowflakeDatabase = "Database_Raw",
                SnowflakeSchema = "Schema_Raw",
                SnowflakeTable = "Table_Raw",
                SnowflakeStatus = "Available_Raw",
                SnowflakeType = SnowflakeConsumptionType.DatasetSchemaRaw
            };

            SchemaConsumptionSnowflake schemaConsumptionSnowflake_Parquet_Updates = new SchemaConsumptionSnowflake()
            {
                SchemaConsumptionId = 0,
                SnowflakeWarehouse = "WH",
                SnowflakeStage = "ST",
                SnowflakeDatabase = "DB",
                SnowflakeSchema = "SC",
                SnowflakeTable = "TB",
                SnowflakeStatus = "Available",
                SnowflakeType = SnowflakeConsumptionType.DatasetSchemaParquet
            };

            schema.ConsumptionDetails = new List<SchemaConsumption>() { schemaConsumptionSnowflake_Parquet, schemaConsumptionSnowflake_Raw };

            //Act
            schema.AddOrUpdateSnowflakeConsumptionLayer(schemaConsumptionSnowflake_Parquet_Updates);

            var datasetSchemaParquet = schema.ConsumptionDetails.Cast<SchemaConsumptionSnowflake>().First(w => w.SnowflakeType == SnowflakeConsumptionType.DatasetSchemaParquet);
            var datasetSchemaRaw = schema.ConsumptionDetails.Cast<SchemaConsumptionSnowflake>().First(w => w.SnowflakeType == SnowflakeConsumptionType.DatasetSchemaRaw);

            //Assert
            Assert.AreEqual(1, datasetSchemaParquet.SchemaConsumptionId);
            Assert.AreEqual("WH", datasetSchemaParquet.SnowflakeWarehouse);
            Assert.AreEqual("ST", datasetSchemaParquet.SnowflakeStage);
            Assert.AreEqual("DB", datasetSchemaParquet.SnowflakeDatabase);
            Assert.AreEqual("SC", datasetSchemaParquet.SnowflakeSchema);
            Assert.AreEqual("TB", datasetSchemaParquet.SnowflakeTable);
            Assert.AreEqual("Available_Parquet", datasetSchemaParquet.SnowflakeStatus);
            Assert.AreEqual(2, datasetSchemaRaw.SchemaConsumptionId);
            Assert.AreEqual("Warehouse_Raw", datasetSchemaRaw.SnowflakeWarehouse);
            Assert.AreEqual("Stage_Raw", datasetSchemaRaw.SnowflakeStage);
            Assert.AreEqual("Database_Raw", datasetSchemaRaw.SnowflakeDatabase);
            Assert.AreEqual("Schema_Raw", datasetSchemaRaw.SnowflakeSchema);
            Assert.AreEqual("Table_Raw", datasetSchemaRaw.SnowflakeTable);
            Assert.AreEqual("Available_Raw", datasetSchemaRaw.SnowflakeStatus);

        }
    }
}
