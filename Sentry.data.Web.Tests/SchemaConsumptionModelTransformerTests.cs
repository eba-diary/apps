using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Web.Models.ApiModels.Schema20220609;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class SchemaConsumptionModelTransformerTests
    {

        /// <summary>
        /// Tests that <see cref="SchemaConsumptionModelTransformer"/> can transform a
        /// <see cref="SchemaConsumptionModel"/> list to a <see cref="SchemaConsumptionDto"/> list
        /// </summary>
        [TestMethod]
        public void Transform_SchemaConsumptionModel_Test()
        {
            // Arrange
            var entityList = new List<SchemaConsumptionModel>() {
                new SchemaConsumptionSnowflakeModel()
                {
                    SchemaConsumptionId = 1,
                    SnowflakeDatabase = nameof(SchemaConsumptionSnowflakeModel.SnowflakeDatabase),
                    SnowflakeSchema = nameof(SchemaConsumptionSnowflakeModel.SnowflakeSchema),
                    SnowflakeStage = nameof(SchemaConsumptionSnowflakeModel.SnowflakeStage),
                    SnowflakeStatus = nameof(SchemaConsumptionSnowflakeModel.SnowflakeStatus),
                    SnowflakeTable = nameof(SchemaConsumptionSnowflakeModel.SnowflakeTable),
                    SnowflakeType = SnowflakeConsumptionType.CategorySchemaParquet,
                    SnowflakeWarehouse = nameof(SchemaConsumptionSnowflakeModel.SnowflakeWarehouse)
                }
            };
            var entity = entityList.OfType<SchemaConsumptionSnowflakeModel>().First();

            // Act
            var dtoList = entityList.Select(s => s.Accept(new SchemaConsumptionModelTransformer())).ToList();
            var dto = dtoList.OfType<SchemaConsumptionSnowflakeDto>().First();

            // Assert
            Assert.AreEqual(1, dtoList.Count);
            Assert.AreEqual(entity.SchemaConsumptionId, dto.SchemaConsumptionId);
            Assert.AreEqual(entity.SnowflakeDatabase, dto.SnowflakeDatabase);
            Assert.AreEqual(entity.SnowflakeSchema, dto.SnowflakeSchema);
            Assert.AreEqual(entity.SnowflakeStage, dto.SnowflakeStage);
            Assert.AreEqual(entity.SnowflakeStatus, dto.SnowflakeStatus);
            Assert.AreEqual(entity.SnowflakeTable, dto.SnowflakeTable);
            Assert.AreEqual(entity.SnowflakeType, dto.SnowflakeType);
            Assert.AreEqual(entity.SnowflakeWarehouse, dto.SnowflakeWarehouse);
        }

        /// <summary>
        /// Tests that <see cref="SchemaConsumptionModelTransformer"/> can transform a
        /// <see cref="SchemaConsumptionDto"/> list to a <see cref="SchemaConsumptionModel"/> list
        /// </summary>
        [TestMethod]
        public void Transform_SchemaConsumptionDto_Test()
        {
            // Arrange
            var entityList = new List<SchemaConsumptionDto>() {
                new SchemaConsumptionSnowflakeDto()
                {
                    SchemaConsumptionId = 1,
                    SnowflakeDatabase = nameof(SchemaConsumptionSnowflakeDto.SnowflakeDatabase),
                    SnowflakeSchema = nameof(SchemaConsumptionSnowflakeDto.SnowflakeSchema),
                    SnowflakeStage = nameof(SchemaConsumptionSnowflakeDto.SnowflakeStage),
                    SnowflakeStatus = nameof(SchemaConsumptionSnowflakeDto.SnowflakeStatus),
                    SnowflakeTable = nameof(SchemaConsumptionSnowflakeDto.SnowflakeTable),
                    SnowflakeType = SnowflakeConsumptionType.CategorySchemaParquet,
                    SnowflakeWarehouse = nameof(SchemaConsumptionSnowflakeDto.SnowflakeWarehouse)
                }
            };
            var entity = entityList.OfType<SchemaConsumptionSnowflakeDto>().First();

            // Act
            var dtoList = entityList.Select(s => s.Accept(new SchemaConsumptionModelTransformer())).ToList();
            var dto = dtoList.OfType<SchemaConsumptionSnowflakeModel>().First();

            // Assert
            Assert.AreEqual(1, dtoList.Count);
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
