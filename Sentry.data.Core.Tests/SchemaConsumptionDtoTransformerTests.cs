using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class SchemaConsumptionDtoTransformerTests
    {

        /// <summary>
        /// Tests that <see cref="SchemaConsumptionDtoTransformer"/> can transform a
        /// <see cref="SchemaConsumption"/> list to a <see cref="SchemaConsumptionDto"/> list
        /// </summary>
        [TestMethod]
        public void Transform_SchemaConsumptionSnowflake_Test()
        {
            // Arrange
            var entityList = new List<SchemaConsumption>() { 
                new SchemaConsumptionSnowflake()
                {
                    SchemaConsumptionId = 1,
                    SnowflakeDatabase = nameof(SchemaConsumptionSnowflake.SnowflakeDatabase),
                    SnowflakeSchema = nameof(SchemaConsumptionSnowflake.SnowflakeSchema),
                    SnowflakeStage = nameof(SchemaConsumptionSnowflake.SnowflakeStage),
                    SnowflakeStatus = nameof(SchemaConsumptionSnowflake.SnowflakeStatus),
                    SnowflakeTable = nameof(SchemaConsumptionSnowflake.SnowflakeTable),
                    SnowflakeType = SnowflakeConsumptionType.CategorySchemaParquet,
                    SnowflakeWarehouse = nameof(SchemaConsumptionSnowflake.SnowflakeWarehouse)
                } 
            };
            var entity = entityList.OfType<SchemaConsumptionSnowflake>().First();

            // Act
            var dtoList = entityList.Select(s => s.Accept(new SchemaConsumptionDtoTransformer())).ToList();
            var dto = dtoList.OfType<SchemaConsumptionSnowflakeDto>().First();

            // Assert
            Assert.AreEqual(1,dtoList.Count);
            Assert.AreEqual(entity.SchemaConsumptionId, dto.SchemaConsumptionId);
            Assert.AreEqual(entity.SnowflakeDatabase, dto.SnowflakeDatabase);
            Assert.AreEqual(entity.SnowflakeSchema, dto.SnowflakeSchema);
            Assert.AreEqual(entity.SnowflakeStage, dto.SnowflakeStage);
            Assert.AreEqual(entity.SnowflakeStatus, dto.SnowflakeStatus);
            Assert.AreEqual(entity.SnowflakeTable, dto.SnowflakeTable);
            Assert.AreEqual(entity.SnowflakeType, dto.SnowflakeType);
            Assert.AreEqual(entity.SnowflakeWarehouse, dto.SnowflakeWarehouse);
        }

    }
}
