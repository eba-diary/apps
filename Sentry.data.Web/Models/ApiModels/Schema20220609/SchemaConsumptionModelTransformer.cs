
using Sentry.data.Core;

namespace Sentry.data.Web.Models.ApiModels.Schema20220609
{
    /// <summary>
    /// Implementation of the Visitor Pattern, in order to convert the polymorphic list of 
    /// <see cref="SchemaConsumption"/> entities into strongly-typed <see cref="SchemaConsumptionModel"/>s,
    /// and a list of <see cref="SchemaConsumptionModel"/>s back into <see cref="SchemaConsumption"/>s
    /// </summary>
    public class SchemaConsumptionModelTransformer : ISchemaConsumptionDtoVisitor<SchemaConsumptionModel>, ISchemaConsumptionModelVisitor<SchemaConsumptionDto>
    {
        public SchemaConsumptionModel Visit(SchemaConsumptionSnowflakeDto consumptionDetails)
        {
            return new SchemaConsumptionSnowflakeModel()
            {
                SchemaConsumptionId = consumptionDetails.SchemaConsumptionId,
                SnowflakeWarehouse = consumptionDetails.SnowflakeWarehouse,
                SnowflakeDatabase = consumptionDetails.SnowflakeDatabase,
                SnowflakeSchema = consumptionDetails.SnowflakeSchema,
                SnowflakeStage = consumptionDetails.SnowflakeStage,
                SnowflakeStatus = consumptionDetails.SnowflakeStatus,
                SnowflakeTable = consumptionDetails.SnowflakeTable,
                SnowflakeType = consumptionDetails.SnowflakeType
            };
        }

        public SchemaConsumptionDto Visit(SchemaConsumptionSnowflakeModel consumptionDetails)
        {
            return new SchemaConsumptionSnowflakeDto()
            {
                SchemaConsumptionId = consumptionDetails.SchemaConsumptionId,
                SnowflakeWarehouse = consumptionDetails.SnowflakeWarehouse,
                SnowflakeDatabase = consumptionDetails.SnowflakeDatabase,
                SnowflakeSchema = consumptionDetails.SnowflakeSchema,
                SnowflakeStage = consumptionDetails.SnowflakeStage,
                SnowflakeStatus = consumptionDetails.SnowflakeStatus,
                SnowflakeTable = consumptionDetails.SnowflakeTable,
                SnowflakeType = consumptionDetails.SnowflakeType
            };
        }
    }

}
